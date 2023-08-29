//
// Copyright 2023 Google LLC
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

package com.google.solutions.tokenservice.platform;

import com.google.auth.oauth2.AccessToken;
import com.google.auth.oauth2.GoogleCredentials;
import com.google.auth.oauth2.ImpersonatedCredentials;
import com.google.solutions.tokenservice.ProjectId;
import com.google.solutions.tokenservice.UserId;
import com.google.solutions.tokenservice.oauth.ServiceAccount;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.Date;
import java.util.List;
import java.util.Properties;

public class IntegrationTestEnvironment {
  private IntegrationTestEnvironment() {
  }

  private static final String SETTINGS_FILE = "test.properties";
  public static final GoogleCredentials INVALID_CREDENTIAL =
    new GoogleCredentials(new AccessToken("ey00", new Date(Long.MAX_VALUE))) {
      @Override
      public void refresh() {
      }
    };

  public static final ProjectId PROJECT_ID;

  public static ServiceAccount SERVICE_ACCOUNT;

  static {
    //
    // Open test settings file.
    //
    if (!new File(SETTINGS_FILE).exists()) {
      throw new RuntimeException(
        String.format(
          "Cannot find %s. Create file to specify which test project to use.", SETTINGS_FILE));
    }

    try (FileInputStream in = new FileInputStream(SETTINGS_FILE)) {
      Properties settings = new Properties();
      settings.load(in);

      PROJECT_ID = new ProjectId(getMandatory(settings, "test.project"));
      SERVICE_ACCOUNT = new ServiceAccount(
        new UserId(getMandatory(settings, "test.serviceaccount")),
        GoogleCredentials.getApplicationDefault());
    }
    catch (IOException e) {
      throw new RuntimeException("Failed to load test settings", e);
    }
  }

  private static String getMandatory(Properties properties, String property) {
    String value = properties.getProperty(property);
    if (value == null || value.isEmpty()) {
      throw new RuntimeException(
        String.format("Settings file %s lacks setting for %s", SETTINGS_FILE, property));
    }

    return value;
  }

  private static GoogleCredentials impersonate(GoogleCredentials source, String serviceAccount) {
    return ImpersonatedCredentials.create(
      source, serviceAccount, null, List.of("https://www.googleapis.com/auth/cloud-platform"), 0);
  }
}
