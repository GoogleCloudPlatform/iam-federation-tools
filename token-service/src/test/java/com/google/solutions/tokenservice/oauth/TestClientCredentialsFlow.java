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

import com.google.solutions.tokenservice.UserId;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.platform.LogAdapter;
import org.junit.jupiter.api.Test;
import org.mockito.Mockito;

import jakarta.ws.rs.core.MultivaluedHashMap;
import java.time.Instant;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.when;

public class TestClientCredentialsFlow {
  private static class Flow extends ClientCredentialsFlow
  {
    public Flow(IdTokenIssuer issuer, WorkloadIdentityPool pool) {
      super(
        issuer,
        pool,
        new LogAdapter());
    }

    @Override
    public String name() {
      return "TEST";
    }

    @Override
    public String authenticationMethod() {
      return "TEST";
    }

    @Override
    protected AuthenticatedClient authenticateClient(AuthenticationRequest request) {
      return new AuthenticatedClient("client-1", Instant.now(), Map.of());
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

  //---------------------------------------------------------------------------
  // authenticate.
  //---------------------------------------------------------------------------

  @Test
  public void whenAuthenticationFails_thenAuthenticateThrowsException() {
    var flow = new Flow(
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class)
    ) {
      @Override
      protected AuthenticatedClient authenticateClient(AuthenticationRequest request) {
        throw new RuntimeException("fail");
      }
    };

    assertThrows(
      Authentication.InvalidClientException.class,
      () -> flow.authenticate(createRequest("client-1")));
  }

  @Test
  public void whenAuthenticationSucceeds_thenAuthenticateIssuesIdToken() throws Exception {
    var idToken = new IdToken("id-token", Instant.now(), Instant.MAX);

    var issuer = Mockito.mock(IdTokenIssuer.class);
    when(issuer.issueIdToken(any(), any())).thenReturn(idToken);

    var flow = new Flow(
      issuer,
      Mockito.mock(WorkloadIdentityPool.class));
    var authentication = flow.authenticate(createRequest("client-1"));

    assertNotNull(authentication.client());
    assertNotNull(authentication.idToken());
    assertSame(idToken, authentication.idToken());
    assertNull(authentication.accessToken());
  }

  @Test
  public void whenScopeProvided_thenAuthenticateIssuesStsAccessToken() throws Exception {
    var idToken = new IdToken("id-token", Instant.now(), Instant.MAX);
    var accessToken = new StsAccessToken("access-token", "scope-1", Instant.now(), Instant.MAX);

    var issuer = Mockito.mock(IdTokenIssuer.class);
    when(issuer.issueIdToken(any(), any())).thenReturn(idToken);

    var pool = Mockito.mock(WorkloadIdentityPool.class);
    when(pool.issueAccessToken(same(idToken), eq("scope-1"))).thenReturn(accessToken);

    var flow = new Flow(issuer, pool);

    var parameters = new MultivaluedHashMap<String, String>();
    parameters.add("client_id", "client-1");
    parameters.add("scope", "scope-1");

    var authentication = flow.authenticate(
      new AuthenticationRequest("client_credentials", parameters));

    assertNotNull(authentication.client());
    assertNotNull(authentication.idToken());
    assertNotNull(authentication.accessToken());
    assertSame(accessToken, authentication.accessToken());
  }

  @Test
  public void whenScopeAndServiceAccountProvided_thenAuthenticateIssuesServiceAccountToken() throws Exception {
    var idToken = new IdToken("id-token", Instant.now(), Instant.MAX);
    var accessToken = new StsAccessToken("access-token", "scope-1", Instant.now(), Instant.MAX);
    var saAccessToken = new ServiceAccountAccessToken("access-token", "scope", Instant.now(), Instant.MAX);

    var issuer = Mockito.mock(IdTokenIssuer.class);
    when(issuer.issueIdToken(any(), any())).thenReturn(idToken);

    var impersonatedSaId = new UserId("sa@project.iam.gserviceaccount.com");
    var impersonatedSa = Mockito.mock(ServiceAccount.class);
    when(impersonatedSa.generateAccessToken(any(), any())).thenReturn(saAccessToken);

    var pool = Mockito.mock(WorkloadIdentityPool.class);
    when(pool.issueAccessToken(same(idToken), eq("scope-1"))).thenReturn(accessToken);
    when(pool.impersonateServiceAccount(eq(impersonatedSaId), any())).thenReturn(impersonatedSa);

    var flow = new Flow(issuer, pool);

    var parameters = new MultivaluedHashMap<String, String>();
    parameters.add("client_id", "client-1");
    parameters.add("scope", "scope-1");
    parameters.add("service_account", impersonatedSaId.email());

    var authentication = flow.authenticate(
      new AuthenticationRequest("client_credentials", parameters));

    assertNotNull(authentication.client());
    assertNotNull(authentication.idToken());
    assertNotNull(authentication.accessToken());
    assertSame(saAccessToken, authentication.accessToken());
  }
}
