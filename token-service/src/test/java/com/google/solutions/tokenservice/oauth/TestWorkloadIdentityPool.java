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

package com.google.solutions.tokenservice.oauth;

import com.google.solutions.tokenservice.platform.IntegrationTestEnvironment;
import com.google.solutions.tokenservice.platform.NotAuthenticatedException;
import org.junit.jupiter.api.Test;

import java.time.Duration;
import java.time.Instant;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertThrows;

public class TestWorkloadIdentityPool {

  private static final String CLOUD_PLATFORM_SCOPE
    = "https://www.googleapis.com/auth/cloud-platform";

  // -------------------------------------------------------------------------
  // IssueAccessToken.
  // -------------------------------------------------------------------------

  @Test
  public void whenPoolInvalid_thenIssueAccessTokenThrowsException()
    throws Exception {

    var options = new WorkloadIdentityPool.Options(
      1,
      "doesnotexist",
      "doesnotexist");

    var sts = new WorkloadIdentityPool(options);

    assertThrows(
      IllegalArgumentException.class,
      () -> sts.issueAccessToken(
        new IdToken("id-token", Instant.now(), Instant.MAX),
        CLOUD_PLATFORM_SCOPE));
  }


  // -------------------------------------------------------------------------
  // impersonateServiceAccount.
  // -------------------------------------------------------------------------

  @Test
  public void whenStsTokenInvalid_thenImpersonateServiceAccountThrowsException() {
    var options = new WorkloadIdentityPool.Options(
      1,
      "doesnotexist",
      "doesnotexist");

    var invalidAccessToken = new StsAccessToken(
      "invalid",
      null,
      Instant.now(),
      Instant.now().plus(Duration.ofMinutes(10)));

    var serviceAccount = new WorkloadIdentityPool(options)
      .impersonateServiceAccount(
        IntegrationTestEnvironment.SERVICE_ACCOUNT.id(),
        invalidAccessToken);

    assertThrows(
      NotAuthenticatedException.class,
      () -> serviceAccount.generateAccessToken(
        List.of(CLOUD_PLATFORM_SCOPE),
        Duration.ofMinutes(5)));
  }
}
