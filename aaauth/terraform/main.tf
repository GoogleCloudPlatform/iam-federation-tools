#
# Copyright 2026 Google LLC
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#

#------------------------------------------------------------------------------
# Input variables.
#------------------------------------------------------------------------------

variable "project_id" {
  description      = "Project to deploy to"
  type             = string
}

variable "region" {
  description      = "Region to deploy resources in"
  type             = string
}

variable "repository" {
  description      = "Name of the Artifact Registry repository"
  type             = string
  default          = "docker"
}

variable "entra_tenant" {
  description      = "Entra Tenant ID, in format 00000000-0000-0000-0000-000000000000"
  type             = string
}

variable "entra_provider" {
  description      = "Workforce identity provider, in format locations/global/workforcePools/POOL/providers/PROVIDER"
  type             = string
}

#------------------------------------------------------------------------------
# Provider.
#------------------------------------------------------------------------------

terraform {
  provider_meta "google" {
    module_name    = "cloud-solutions/aaauth-v1.0"
  }
}

provider "google" {
  project          = var.project_id
}

#------------------------------------------------------------------------------
# Local variables.
#------------------------------------------------------------------------------

locals {
  sources         = "${path.module}/../sources"
  image_name      = "${var.project_id}/${var.repository}/aaauth"
  docker_registry = "${var.region}-docker.pkg.dev"
  image_tag       = data.external.git.result.sha
  
  #
  # Base image to use for the container, must use the right ASP.NET version.
  #
  base_image      = "mcr.microsoft.com/dotnet/aspnet:8.0-jammy"
}

#
# Get current commit SHA.
#
data "external" "git" {
  program = [
    "sh", "-c", "echo {\\\"sha\\\": \\\"$(git rev-parse HEAD)\\\"}"
  ]
  working_dir = local.sources
}

#------------------------------------------------------------------------------
# Required APIs
#------------------------------------------------------------------------------

resource "google_project_service" "iam" {
  service            = "iam.googleapis.com"
  disable_on_destroy = false
}

resource "google_project_service" "artifactregistry" {
  service            = "artifactregistry.googleapis.com"
  disable_on_destroy = false
}

resource "google_project_service" "run" {
  service            = "run.googleapis.com"
  disable_on_destroy = false
}

#------------------------------------------------------------------------------
# Project.
#------------------------------------------------------------------------------

data "google_project" "project" {
  project_id = var.project_id
}

#------------------------------------------------------------------------------
# Docker registry.
#------------------------------------------------------------------------------

#
# Create a Docker repository.
#
resource "google_artifact_registry_repository" "registry" {
  depends_on    = [google_project_service.artifactregistry]
  format        = "DOCKER"
  repository_id = var.repository
  location      = var.region
}

#------------------------------------------------------------------------------
# Service account.
#------------------------------------------------------------------------------

#
# Service account for the service.
#
resource "google_service_account" "aaauth" {
  depends_on   = [google_project_service.iam]
  account_id   = "aaauth-service"
  display_name = "AAAuth service"
}

#------------------------------------------------------------------------------
# Cloud Run service.
#------------------------------------------------------------------------------

#
# Build .NET solution into a Docker image and publish it to Artifact Registry.
#

resource "null_resource" "aaauth_image" {
  depends_on = [google_artifact_registry_repository.registry]
  triggers = {
    always_rebuild = timestamp()
  }
  provisioner "local-exec" {
    command = "dotnet publish --os linux --arch x64 /t:PublishContainer -p ContainerRegistry=${local.docker_registry} -p ContainerBaseImage=${local.base_image} -p ContainerRepository=${local.image_name} -p ContainerImageTag=${local.image_tag} ${local.sources}"
    interpreter = ["sh", "-c"]
  }
}

#
# Deploy a Cloud Run service.
#
resource "google_cloud_run_v2_service" "service" {
  depends_on = [null_resource.aaauth_image, google_project_service.run]

  provider         = google
  location         = var.region
  name             = "service"
  deletion_protection = false

  template {
    service_account       = google_service_account.aaauth.email
    execution_environment = "EXECUTION_ENVIRONMENT_GEN2"

    scaling {
      max_instance_count = 2
    }

    containers {
      image = "${local.docker_registry}/${local.image_name}:${local.image_tag}"

      env {
        name = "Entra__TenantId"
        value = var.entra_tenant
      }
      env {
        name = "Entra__WorkforceIdentityProviderName"
        value = var.entra_provider
      }
    }
  }
}

#
# Allow anonymous access to the Cloud Run service.
#
resource "google_cloud_run_service_iam_binding" "default" {
  location = google_cloud_run_v2_service.service.location
  service  = google_cloud_run_v2_service.service.name
  role     = "roles/run.invoker"
  members  = ["allUsers"]
}

#------------------------------------------------------------------------------
# Outputs.
#------------------------------------------------------------------------------

output "url" {
  description = "Cloud Run service URI"
  value       = google_cloud_run_v2_service.service.uri
}
