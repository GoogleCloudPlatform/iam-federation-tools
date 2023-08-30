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

import com.google.solutions.tokenservice.Base64Helper;
import com.google.solutions.tokenservice.oauth.AuthenticationRequest;
import com.google.solutions.tokenservice.oauth.IdTokenIssuer;
import com.google.solutions.tokenservice.oauth.WorkloadIdentityPool;
import com.google.solutions.tokenservice.oauth.client.ClientPolicy;
import com.google.solutions.tokenservice.platform.LogAdapter;
import io.vertx.core.http.HttpServerRequest;
import io.vertx.core.http.impl.headers.HeadersMultiMap;
import org.junit.jupiter.api.Test;
import org.mockito.Mockito;

import javax.ws.rs.ForbiddenException;
import javax.ws.rs.core.MultivaluedHashMap;

import java.util.Base64;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.when;
import static org.wildfly.common.Assert.assertTrue;

public class TestXlbMtlsClientCredentialsFlow {
  private static final XlbMtlsClientCredentialsFlow.Options OPTIONS
    = new XlbMtlsClientCredentialsFlow.Options(
      "X-ClientId",
      "X-CertPresent",
      "X-CertChainVerified",
      "X-CertError",
      "X-CertSpiffeId",
      "X-CertDnsSans",
      "X-CertUriSans",
      "X-CertHash",
      "X-CertSerialNumber",
      "X-CertNotBefore",
      "X-CertNotAfter"
    );

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
  // canAuthenticate.
  // -------------------------------------------------------------------------

  @Test
  public void whenMtlsHeadersMissing_thenCanAuthenticateReturnsFalse()
  {
    var headers = new HeadersMultiMap();
    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
      OPTIONS,
      Mockito.mock(ClientPolicy.class),
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class),
      httpRequest,
      new LogAdapter());

    var request = createRequest("client-1");
    assertFalse(flow.canAuthenticate(request));
  }

  @Test
  public void whenMtlsCertPresentHeaderIsFalse_thenCanAuthenticateReturnsFalse()
  {
    var headers = new HeadersMultiMap();
    headers.add(OPTIONS.clientCertPresentHeaderName(), "false");

    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
      OPTIONS,
      Mockito.mock(ClientPolicy.class),
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class),
      httpRequest,
      new LogAdapter());

    var request = createRequest("client-1");
    assertFalse(flow.canAuthenticate(request));
  }

  @Test
  public void whenMtlsCertPresentHeaderIsTrue_thenCanAuthenticateReturnsTrue()
  {
    var headers = new HeadersMultiMap();
    headers.add(OPTIONS.clientCertPresentHeaderName(), "TRuE");

    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
      OPTIONS,
      Mockito.mock(ClientPolicy.class),
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class),
      httpRequest,
      new LogAdapter());

    var request = createRequest("client-1");
    assertTrue(flow.canAuthenticate(request));
  }

  // -------------------------------------------------------------------------
  // getVerifiedClientAttributes.
  // -------------------------------------------------------------------------

  @Test
  public void whenMtlsCertChainVerifiedHeaderIsFalse_thenGetVerifiedClientAttributesThrowsException()
  {
    var headers = new HeadersMultiMap();
    headers.add(OPTIONS.clientCertPresentHeaderName(), "TRuE");
    headers.add(OPTIONS.clientCertChainVerifiedHeaderName(), "nottrue");

    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
      OPTIONS,
      Mockito.mock(ClientPolicy.class),
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class),
      httpRequest,
      new LogAdapter());

    var request = createRequest("client-1");
    assertThrows(
      ForbiddenException.class,
      () -> flow.getVerifiedClientAttributes(request));
  }

  @Test
  public void whenMtlsCertChainVerifiedHeaderIsTrueButClientIdMissing_thenGetVerifiedClientAttributesThrowsException()
  {
    var headers = new HeadersMultiMap();
    headers.add(OPTIONS.clientCertPresentHeaderName(), "TRuE");
    headers.add(OPTIONS.clientCertChainVerifiedHeaderName(), "TRue");
    headers.add(OPTIONS.clientCertSpiffeIdHeaderName(), "spiffe-1");

    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
      OPTIONS,
      Mockito.mock(ClientPolicy.class),
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class),
      httpRequest,
      new LogAdapter());

    var request = createRequest("client-1");
    assertThrows(
      ForbiddenException.class,
      () -> flow.getVerifiedClientAttributes(request));
  }

  @Test
  public void whenMtlsCertChainVerifiedHeaderIsTrue_thenGetVerifiedClientAttributesReturnsAttributes()
  {
    var headers = new HeadersMultiMap();
    headers.add(OPTIONS.clientCertPresentHeaderName(), "TRuE");
    headers.add(OPTIONS.clientCertChainVerifiedHeaderName(), "TRue");
    headers.add(OPTIONS.clientCertSpiffeIdHeaderName(), "spiffe-1");
    headers.add(OPTIONS.clientIdHeaderName(), "spiffe-1");

    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
      OPTIONS,
      Mockito.mock(ClientPolicy.class),
      Mockito.mock(IdTokenIssuer.class),
      Mockito.mock(WorkloadIdentityPool.class),
      httpRequest,
      new LogAdapter());

    var request = createRequest("client-1");

    var attributes = flow.getVerifiedClientAttributes(request);

    assertNotNull(attributes);
    assertEquals("spiffe-1", attributes.spiffeId());
  }

  @Test
  public void whenMultipleSansPresent_thenGetVerifiedClientAttributesReturnsFirstSan()
  {
    var headers = new HeadersMultiMap();
    headers.add(OPTIONS.clientCertPresentHeaderName(), "true");
    headers.add(OPTIONS.clientCertChainVerifiedHeaderName(), "true");
    headers.add(OPTIONS.clientCertDnsSansHeaderName(), Base64Helper.escape("dns-1, dns-2"));
    headers.add(OPTIONS.clientCertUriSansHeaderName(), Base64Helper.escape("https://uri-1,https://uri-2"));
    headers.add(OPTIONS.clientIdHeaderName(), "client-1");

    var httpRequest = Mockito.mock(HttpServerRequest.class);
    when(httpRequest.headers()).thenReturn(headers);

    var flow = new XlbMtlsClientCredentialsFlow(
            OPTIONS,
            Mockito.mock(ClientPolicy.class),
            Mockito.mock(IdTokenIssuer.class),
            Mockito.mock(WorkloadIdentityPool.class),
            httpRequest,
            new LogAdapter());

    var request = createRequest("client-1");

    var attributes = flow.getVerifiedClientAttributes(request);

    assertNotNull(attributes);
    assertEquals("dns-1", attributes.sanDns());
    assertEquals("https://uri-1", attributes.sanUri());
  }
}
