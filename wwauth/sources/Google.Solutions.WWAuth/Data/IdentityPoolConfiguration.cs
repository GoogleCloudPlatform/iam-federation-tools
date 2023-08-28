//
// Copyright 2022 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using System.Text.RegularExpressions;

namespace Google.Solutions.WWAuth.Data
{
    /// <summary>
    /// Common base class for workload and workforce
    /// identity pools.
    /// </summary>
    internal abstract class IdentityPoolConfiguration
    {
        public string PoolName { get; set; }
        public string Location { get; set; } = "global";
        public string ProviderName { get; set; }

        public abstract string Audience { get; }

        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(this.PoolName))
            {
                throw new InvalidCredentialConfigurationException(
                    "Missing identity pool name.");
            }

            if (string.IsNullOrEmpty(this.ProviderName))
            {
                throw new InvalidCredentialConfigurationException(
                    "Missing identity pool provider name.");
            }
        }

        public bool IsValid
        {
            get
            {
                try
                {
                    Validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Configuration for a workload identity pool/provider.
    /// </summary>
    internal class WorkloadIdentityPoolConfiguration : IdentityPoolConfiguration
    {
        public ulong? ProjectNumber { get; set; }

        public override string Audience =>
            $"//iam.googleapis.com/projects/{this.ProjectNumber?.ToString() ?? "-"}/locations" +
            $"/{this.Location}/workloadIdentityPools/" +
            $"{this.PoolName ?? "-"}/providers/{this.ProviderName ?? "-"}";

        public override void Validate()
        {
            if (this.ProjectNumber == null || this.ProjectNumber == 0)
            {
                throw new InvalidCredentialConfigurationException(
                    "Missing project number for the workload identity pool.");
            }

            base.Validate();
        }

        public static bool TryParse(
            string audience,
            out WorkloadIdentityPoolConfiguration configuration)
        {
            configuration = null;

            if (audience == null)
            {
                return false;
            }

            var audienceMatch = new Regex(
                "^//iam.googleapis.com/projects/(\\d+)/locations/(.+)/workloadIdentityPools/" +
                "(.+)/providers/(.+)$").Match(audience);
            if (audienceMatch.Success)
            {
                configuration = new WorkloadIdentityPoolConfiguration()
                {
                    ProjectNumber = ulong.Parse(audienceMatch.Groups[1].Value),
                    Location = audienceMatch.Groups[2].Value,
                    PoolName = audienceMatch.Groups[3].Value,
                    ProviderName = audienceMatch.Groups[4].Value
                };

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Configuration for a workforce identity pool/provider.
    /// </summary>
    internal class WorkforceIdentityPoolConfiguration : IdentityPoolConfiguration
    {
        public ulong? UserProjectNumber { get; set; }

        public override string Audience =>
            $"//iam.googleapis.com/locations" +
            $"/{this.Location}/workforcePools/" +
            $"{this.PoolName ?? "-"}/providers/{this.ProviderName ?? "-"}";

        public override void Validate()
        {
            if (this.UserProjectNumber == null || this.UserProjectNumber == 0)
            {
                throw new InvalidCredentialConfigurationException(
                    "Missing user project number for workforce identity.");
            }

            base.Validate();
        }

        public static bool TryParse(
            string audience,
            out WorkforceIdentityPoolConfiguration configuration)
        {
            configuration = null;

            if (audience == null)
            {
                return false;
            }

            var audienceMatch = new Regex(
                "^//iam.googleapis.com/locations/(.+)/workforcePools/" +
                "(.+)/providers/(.+)$").Match(audience);
            if (audienceMatch.Success)
            {
                configuration = new WorkforceIdentityPoolConfiguration()
                {
                    Location = audienceMatch.Groups[1].Value,
                    PoolName = audienceMatch.Groups[2].Value,
                    ProviderName = audienceMatch.Groups[3].Value
                };

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
