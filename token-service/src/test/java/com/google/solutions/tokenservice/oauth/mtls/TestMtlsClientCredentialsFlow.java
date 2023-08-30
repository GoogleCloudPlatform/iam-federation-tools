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

package com.google.solutions.tokenservice.oauth.mtls;

import com.google.auth.oauth2.TokenVerifier;
import com.google.solutions.tokenservice.URLHelper;
import com.google.solutions.tokenservice.oauth.Authentication;
import com.google.solutions.tokenservice.oauth.AuthenticationRequest;
import com.google.solutions.tokenservice.oauth.IdTokenIssuer;
import com.google.solutions.tokenservice.oauth.WorkloadIdentityPool;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.oauth.client.ClientPolicy;
import com.google.solutions.tokenservice.platform.IntegrationTestEnvironment;
import com.google.solutions.tokenservice.platform.LogAdapter;
import org.junit.jupiter.api.Test;
import org.mockito.Mockito;

import jakarta.ws.rs.core.MultivaluedHashMap;
import java.net.URL;
import java.time.Duration;
import java.time.Instant;
import java.util.ArrayList;
import java.util.HashMap;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;

public class TestMtlsClientCredentialsFlow {
  private static final String ISSUER_ID = "http://example.com";

  private static class Flow extends MtlsClientCredentialsFlow
  {
    public Flow(ClientPolicy clientPolicy, IdTokenIssuer issuer) {
      super(
        clientPolicy,
        issuer,
        Mockito.mock(WorkloadIdentityPool.class),
        new LogAdapter());
    }

    @Override
    public String name() {
      return "TEST";
    }

    @Override
    protected MtlsClientAttributes getVerifiedClientAttributes(AuthenticationRequest request) {
      return new MtlsClientAttributes(null, null, null, null, null, null, null, null);
    }
  }

  private static AuthenticationRequest createRequest(String clientId)
  {
    var parameters = new MultivaluedHashMap<String, String>();
    if (clientId != null) {
      parameters.add("client_id", clientId);
    }

    return new AuthenticationRequest(
      "client_credentials",
      parameters);
  }

  // -------------------------------------------------------------------------
  // authenticate.
  // -------------------------------------------------------------------------

  @Test
  public void whenClientRepositoryFailsToAuthenticate_thenAuthenticateThrowsException() {
    var clientRepository = Mockito.mock(ClientPolicy.class);
    when(clientRepository.authenticateClient(any()))
      .thenThrow(new RuntimeException("mock"));

    var flow = new Flow(
      clientRepository,
      Mockito.mock(IdTokenIssuer.class));

    assertThrows(
      Authentication.InvalidClientException.class,
      () -> flow.authenticate(createRequest("client-1")));
  }

  @Test
  public void whenClientRepositoryAuthenticatesClient_thenAuthenticateReturnsToken() throws Exception {
    var client = new AuthenticatedClient(
      "client-1",
      Instant.ofEpochSecond(1000),
      new HashMap<>());

    var clientRepository = Mockito.mock(ClientPolicy.class);
    when(clientRepository.authenticateClient(any()))
      .thenReturn(client);

    var tokenAudience = "https://example.com";
    var issuer = new IdTokenIssuer(
      new IdTokenIssuer.Options(
        URLHelper.fromString(ISSUER_ID),
        new URL(tokenAudience),
        Duration.ofMinutes(1)),
      IntegrationTestEnvironment.SERVICE_ACCOUNT);

    var flow = new Flow(
      clientRepository,
      issuer);

    var response = flow.authenticate(createRequest("client-1"));
    assertSame(client, response.client());
    assertNotNull(response.idToken());

    var verifiedPayload = TokenVerifier
      .newBuilder()
      .setCertificatesLocation(issuer.jwksUrl().toString())
      .setIssuer(issuer.id().toString())
      .setAudience(tokenAudience)
      .build()
      .verify(response.idToken().value())
      .getPayload();
    assertEquals(
      flow.name().toLowerCase(),
      ((ArrayList)verifiedPayload.get("amr")).get(0));
  }
}
