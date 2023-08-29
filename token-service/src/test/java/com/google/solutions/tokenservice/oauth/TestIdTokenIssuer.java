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

import com.google.api.client.json.webtoken.JsonWebToken;
import com.google.auth.oauth2.TokenVerifier;
import com.google.solutions.tokenservice.URLHelper;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.platform.IntegrationTestEnvironment;
import org.junit.jupiter.api.Test;

import java.net.URL;
import java.time.Duration;
import java.time.Instant;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.wildfly.common.Assert.assertTrue;

public class TestIdTokenIssuer {
  private static final URL ISSUER_ID = URLHelper.fromString("http://issuer.example.com/");

  // -------------------------------------------------------------------------
  // issueToken.
  // -------------------------------------------------------------------------

  @Test
  public void issueTokenCreatesTokenForAudience() throws Exception {
    var serviceAccount = IntegrationTestEnvironment.SERVICE_ACCOUNT;

    var issuer = new IdTokenIssuer(
      new IdTokenIssuer.Options(
        ISSUER_ID,
        URLHelper.fromString("https://example.com/"),
        Duration.ofMinutes(1)),
      serviceAccount);

    var payload = new JsonWebToken.Payload()
      .set("test", "value");

    var client = new AuthenticatedClient("client-1", Instant.now(), Map.of());
    var token = issuer.issueIdToken(
      client,
      payload);

    var verifiedPayload = TokenVerifier
      .newBuilder()
      .setCertificatesLocation(serviceAccount.jwksUrl().toString())
      .setIssuer("http://issuer.example.com")
      .setAudience("https://example.com/")
      .build()
      .verify(token.value())
      .getPayload();

    assertEquals("http://issuer.example.com", verifiedPayload.getIssuer());
    assertEquals("https://example.com/", verifiedPayload.getAudience());
    assertNotNull(verifiedPayload.getIssuedAtTimeSeconds());
    assertNotNull(verifiedPayload.getExpirationTimeSeconds());
    assertTrue(token.expiryTime().isAfter(Instant.now()));
    assertEquals("value", verifiedPayload.get("test"));
  }
}
