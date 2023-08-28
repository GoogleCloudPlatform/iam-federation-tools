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

package com.google.solutions.tokenservice.web;

import com.google.solutions.tokenservice.URLHelper;
import com.google.solutions.tokenservice.oauth.*;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.platform.IntegrationTestEnvironment;
import com.google.solutions.tokenservice.platform.LogAdapter;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.Mockito;

import javax.enterprise.inject.Instance;
import javax.ws.rs.core.Response;
import java.net.URL;
import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.when;

public class TestOAuthResource {

  private static final URL ISSUER_ID = URLHelper.fromString("http://example.com/");

  private OAuthResource resource;

  @BeforeEach
  public void before() {
    this.resource = new OAuthResource();
    this.resource.logAdapter = new LogAdapter();
    this.resource.runtimeEnvironment = Mockito.mock(RuntimeEnvironment.class);
    this.resource.tokenIssuer = new IdTokenIssuer(
      new IdTokenIssuer.Options(ISSUER_ID, null, Duration.ofMinutes(5)),
      IntegrationTestEnvironment.SERVICE_ACCOUNT);

    this.resource.flows = Mockito.mock(Instance.class);
  }

  private void setFlow(AuthenticationFlow flow) {
    when(this.resource.flows.stream()).thenReturn(List.of(flow).stream());

    resource.configuration = Mockito.mock(RuntimeConfiguration.class);
    when (resource.configuration.authenticationFlows())
      .thenReturn(Set.of(flow.name()));
  }

  private abstract class TestFlow implements AuthenticationFlow {

    @Override
    public String name() {
      return "test";
    }

    @Override
    public String grantType() {
      return "test";
    }

    @Override
    public String authenticationMethod() {
      return "test";
    }

    @Override
    public boolean canAuthenticate(AuthenticationRequest request) {
      return true;
    }

    @Override
    public abstract Authentication authenticate(AuthenticationRequest request)
      throws Authentication.AuthenticationException;
  }

  // -------------------------------------------------------------------------
  // invalid.
  // -------------------------------------------------------------------------

  @Test
  public void whenPathNotMapped_ThenGetReturnsError() throws Exception {
    var response = new RestDispatcher<>(this.resource)
      .get("/api/unknown", OAuthResource.TokenErrorResponse.class);

    assertEquals(404, response.getStatus());
  }

  // -------------------------------------------------------------------------
  // Metadata.
  // -------------------------------------------------------------------------

  @Test
  public void getMetadata() throws Exception {
    var response = new RestDispatcher<>(this.resource)
      .get("/.well-known/openid-configuration", OAuthResource.ProviderMetadata.class);

    assertEquals(200, response.getStatus());

    assertEquals(ISSUER_ID, response.getBody().issuerEndpoint());
    assertEquals(new URL(ISSUER_ID, "/token"), response.getBody().tokenEndpoint());
    assertEquals(new URL(ISSUER_ID, "/token"), response.getBody().authorizationEndpoint());
  }

  // -------------------------------------------------------------------------
  // Token.
  // -------------------------------------------------------------------------

  @Test
  public void whenGrantTypeMissing_thenTokenReturnsError() throws Exception {
    var response = new RestDispatcher<>(this.resource)
      .postForm("/token", Map.ofEntries(), OAuthResource.TokenErrorResponse.class);

    assertEquals(400, response.getStatus());
    assertEquals("invalid_request", response.getBody().error());
  }

  @Test
  public void whenGrantTypeNotSupported_thenTokenReturnsError() throws Exception {
    var response = new RestDispatcher<>(this.resource)
      .postForm("/token", Map.ofEntries(), OAuthResource.TokenErrorResponse.class);

    assertEquals(400, response.getStatus());
    assertEquals("invalid_request", response.getBody().error());
  }

  @Test
  public void whenContentTypeWrong_thenTokenReturnsError() throws Exception {
    var response = new RestDispatcher<>(this.resource)
      .post("/token", OAuthResource.TokenErrorResponse.class);

    assertEquals(415, response.getStatus());
  }

  // -------------------------------------------------------------------------
  // Token - OAuth flow.
  // -------------------------------------------------------------------------

  @Test
  public void whenFlowFailsWithIllegalArgumentException_thenTokenReturnsError() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) {
        throw new IllegalArgumentException();
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of("grant_type", flow.grantType()),
        OAuthResource.TokenErrorResponse.class);

    assertEquals(400, response.getStatus());
    assertEquals(OAuthResource.TokenErrorResponse.INVALID_REQUEST, response.getBody().error());
  }

  @Test
  public void whenFlowFailsWithInvalidClientException_thenTokenReturnsError() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) throws Authentication.AuthenticationException {
        throw new Authentication.InvalidClientException("invalid", null);
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of("grant_type", flow.grantType()),
        OAuthResource.TokenErrorResponse.class);

    assertEquals(403, response.getStatus());
    assertEquals(OAuthResource.TokenErrorResponse.UNAUTHORIZED_CLIENT, response.getBody().error());
  }

  @Test
  public void whenFlowFailsWithTokenIssuanceException_thenTokenReturnsError() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) throws Authentication.AuthenticationException {
        throw new Authentication.TokenIssuanceException("invalid", null);
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of("grant_type", flow.grantType()),
        OAuthResource.TokenErrorResponse.class);

    assertEquals(403, response.getStatus());
    assertEquals(OAuthResource.TokenErrorResponse.ACCESS_DENIED, response.getBody().error());
  }

  @Test
  public void whenFlowFailsWithOtherException_thenTokenReturnsError() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) {
        throw new IllegalStateException();
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of("grant_type", flow.grantType()),
        OAuthResource.TokenErrorResponse.class);

    assertEquals(500, response.getStatus());
    assertEquals(OAuthResource.TokenErrorResponse.SERVER_ERROR, response.getBody().error());
  }

  @Test
  public void whenFlowSucceedsWithIdToken_thenTokenSucceeds() throws Exception {
    var iat = Instant.now();
    var exp = iat.plus(Duration.ofMinutes(1));
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) throws Authentication.AuthenticationException {
        return new Authentication(
          new AuthenticatedClient("client", iat, Map.of()),
          new IdToken("id-token", iat, exp),
          null);
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of("grant_type", flow.grantType()),
        OAuthResource.TokenResponse.class);

    assertEquals(200, response.getStatus());
    assertEquals("id-token", response.getBody().idToken());
    assertNull(response.getBody().accessToken());
    assertNull( response.getBody().tokenType());
    assertNull(response.getBody().expiresInSeconds());
    assertNull(response.getBody().scope());
  }

  @Test
  public void whenFlowSucceedsWithIdAndAccessTokens_thenTokenSucceeds() throws Exception {
    var iat = Instant.now();
    var exp = iat.plus(Duration.ofMinutes(1));
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) throws Authentication.AuthenticationException {
        return new Authentication(
          new AuthenticatedClient("client", iat, Map.of()),
          new IdToken("id-token", iat, exp),
          new StsAccessToken("access-token", "scope", iat, exp));
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of("grant_type", flow.grantType()),
        OAuthResource.TokenResponse.class);

    assertEquals(200, response.getStatus());
    assertEquals("id-token", response.getBody().idToken());
    assertEquals("access-token", response.getBody().accessToken());
    assertEquals("Bearer", response.getBody().tokenType());
    assertEquals(60, response.getBody().expiresInSeconds());
    assertEquals("scope", response.getBody().scope());
  }


  // -------------------------------------------------------------------------
  // Token - OAuth flow with external_credential format.
  // -------------------------------------------------------------------------

  @Test
  public void whenFlowFailsWithIllegalArgumentException_thenTokenReturnsErrorInExtCredFormat() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) {
        throw new IllegalArgumentException();
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of(
          "grant_type", flow.grantType(),
          "format", "external_credential"),
        OAuthResource.ExternalCredentialErrorResponse.class);

    assertEquals(400, response.getStatus());
    assertFalse(response.getBody().success());
    assertEquals(OAuthResource.TokenErrorResponse.INVALID_REQUEST, response.getBody().code());
  }

  @Test
  public void whenFlowFailsWithInvalidClientException_thenTokenReturnsErrorInExtCredFormat() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) throws Authentication.AuthenticationException {
        throw new Authentication.InvalidClientException("invalid", null);
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of(
          "grant_type", flow.grantType(),
          "format", "external_credential"),
        OAuthResource.ExternalCredentialErrorResponse.class);

    assertEquals(403, response.getStatus());
    assertFalse(response.getBody().success());
    assertEquals(OAuthResource.TokenErrorResponse.UNAUTHORIZED_CLIENT, response.getBody().code());
  }

  @Test
  public void whenFlowFailsWithTokenIssuanceException_thenTokenReturnsErrorInExtCredFormat() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) throws Authentication.AuthenticationException {
        throw new Authentication.TokenIssuanceException("invalid", null);
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of(
          "grant_type", flow.grantType(),
          "format", "external_credential"),
        OAuthResource.ExternalCredentialErrorResponse.class);

    assertEquals(403, response.getStatus());
    assertFalse(response.getBody().success());
    assertEquals(OAuthResource.TokenErrorResponse.ACCESS_DENIED, response.getBody().code());
  }

  @Test
  public void whenFlowFailsWithOtherException_thenTokenReturnsErrorInExtCredFormat() throws Exception {
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) {
        throw new IllegalStateException();
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of(
          "grant_type", flow.grantType(),
          "format", "external_credential"),
        OAuthResource.ExternalCredentialErrorResponse.class);

    assertEquals(500, response.getStatus());
    assertFalse(response.getBody().success());
    assertEquals(OAuthResource.TokenErrorResponse.SERVER_ERROR, response.getBody().code());
  }

  @Test
  public void whenFlowSucceedsWithIdToken_thenTokenSucceedsInExtCredFormat() throws Exception {
    var iat = Instant.now();
    var exp = iat.plus(Duration.ofMinutes(1));
    var flow = new TestFlow()
    {
      @Override
      public Authentication authenticate(AuthenticationRequest request
      ) {
        return new Authentication(
          new AuthenticatedClient("client", iat, Map.of()),
          new IdToken("id-token", iat, exp),
          null);
      }
    };

    setFlow(flow);

    var response = new RestDispatcher<>(this.resource)
      .postForm(
        "/token",
        Map.of(
          "grant_type", flow.grantType(),
          "format", "external_credential"),
        OAuthResource.ExternalCredentialResponse.class);

    assertEquals(200, response.getStatus());
    assertTrue(response.getBody().success());
    assertEquals("urn:ietf:params:oauth:token-type:id_token", response.getBody().tokenType());
    assertEquals("id-token", response.getBody().idToken());
    assertEquals(60, response.getBody().expirationTime());
  }
}