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

import com.google.common.base.Preconditions;
import com.google.common.base.Strings;
import com.google.solutions.tokenservice.Base64Helper;
import com.google.solutions.tokenservice.oauth.AuthenticationRequest;
import com.google.solutions.tokenservice.oauth.IdTokenIssuer;
import com.google.solutions.tokenservice.oauth.WorkloadIdentityPool;
import com.google.solutions.tokenservice.oauth.client.ClientPolicy;
import com.google.solutions.tokenservice.platform.LogAdapter;
import com.google.solutions.tokenservice.web.LogEvents;
import io.vertx.core.http.HttpServerRequest;
import org.apache.commons.codec.binary.Base64;

import jakarta.enterprise.context.Dependent;
import jakarta.ws.rs.ForbiddenException;
import java.time.OffsetDateTime;
import java.util.Set;

/**
 * Flow that authenticates clients using mTLS, terminated
 * by an external Google Cloud load balancer (XLB).
 *
 * The XLB verifies the certificate chain against a trusted CA,
 * which corresponds to the "PKI Mutual-TLS Method" described in
 * RFC8705.
 */
@Dependent
public class XlbMtlsClientCredentialsFlow extends MtlsClientCredentialsFlow {
  public static final String NAME = "xlb-mtls-client-credentials";

  private final Options options;
  private final HttpServerRequest request;

  private static OffsetDateTime parseIfNotNull(String date) {
    if (!Strings.isNullOrEmpty(date)) {
      return OffsetDateTime.parse(date);
    }
    else {
      return null;
    }
  }

  public XlbMtlsClientCredentialsFlow(
    Options options,
    ClientPolicy clientRepository,
    IdTokenIssuer issuer,
    WorkloadIdentityPool workloadIdentityPool,
    HttpServerRequest request,
    LogAdapter logAdapter
  ) {
    super(clientRepository, issuer, workloadIdentityPool, logAdapter);

    Preconditions.checkNotNull(request, "request");
    Preconditions.checkNotNull(options, "options");

    this.request = request;
    this.options = options;
  }

  private void addHeaderLabels(LogAdapter.LogEntry entry) {
    var headers = this.request.headers();
    for (var headerName : this.options.allClientCertHeaders())
    {
      entry.addLabel("header/" + headerName.toLowerCase(), headers.get(headerName));
    }
  }

  private static String decodeSanHeader(String headerValue) {
    //
    // SAN header contain a Base64-escaped, comma-separated list of SANs
    //
    if (headerValue == null) {
      return null;
    }

    return Base64Helper.unescape(headerValue).split(",")[0];
  }

  //---------------------------------------------------------------------------
  // Overrides.
  //---------------------------------------------------------------------------

  @Override
  public String name() {
    return NAME;
  }

  @Override
  public boolean canAuthenticate(AuthenticationRequest request) {
    Preconditions.checkNotNull(request, "request");

    var headers = this.request.headers();

    var certPresent = headers.get(this.options.clientCertPresentHeaderName);
    if (Strings.isNullOrEmpty(certPresent))
    {
      this.logAdapter
        .newWarningEntry(
          LogEvents.API_TOKEN,
          String.format(
            "The header %s is missing, verify that mTLS is enabled for the load balancer backend",
            this.options.clientCertPresentHeaderName))
        .write();

      return false;
    }
    else if (!"true".equalsIgnoreCase(certPresent))
    {
      this.logAdapter
        .newWarningEntry(
          LogEvents.API_TOKEN,
          String.format(
            "The request did not include a client certificate (%s: %s)",
            this.options.clientCertPresentHeaderName,
            certPresent))
        .write();

      return false;
    }

    return super.canAuthenticate(request);
  }

  public MtlsClientAttributes getVerifiedClientAttributes(AuthenticationRequest request)
  {
    Preconditions.checkNotNull(request, "request");

    //
    // Verify that the client presented a certificate and that the
    // load balancer reported it as "verified".
    //
    var headers = this.request.headers();
    if (!canAuthenticate(request))
    {
      throw new ForbiddenException(
        "The request did not include a client certificate");
    }

    if (!"true".equalsIgnoreCase(headers.get(this.options.clientCertChainVerifiedHeaderName())))
    {
      this.logAdapter
        .newErrorEntry(
          LogEvents.API_TOKEN,
          "The client certificate did not pass verification")
        .addLabels(this::addHeaderLabels)
        .write();

      throw new ForbiddenException("The client certificate did not pass verification");
    }

    this.logAdapter
      .newInfoEntry(
        LogEvents.API_TOKEN,
        "The client certificate was verified by the load balancer")
      .addLabels(this::addHeaderLabels)
      .write();

    //
    // The load balancer verified the certificate, and we can now read the other
    // headers to obtain the certificate attributes.
    //
    // NB. There's no way for us to confirm that it was really the load balancer
    // that added the header. But if the application has been deployed correctly,
    // then there shouldn't be any way for clients to sidestep the load balancer.
    //

    //
    // Read whatever header has been configured to contain the OAuth client ID.
    //
    var clientId = headers.get(this.options.clientIdHeaderName());
    if (Strings.isNullOrEmpty(clientId)) {
      throw new ForbiddenException(
        String.format(
          "The client presented a valid certificate, but the header '%s' does not contain a client ID",
          this.options.clientIdHeaderName()));
    }

    this.logAdapter
      .newInfoEntry(
        LogEvents.API_TOKEN,
        String.format("Authenticated client '%s' using mTLS headers", clientId))
      .addLabels(this::addHeaderLabels)
      .write();


    //
    // Return all attributes from HTTP headers. Note that some
    // attributes might be empty.
    //
    return new MtlsClientAttributes(
      clientId,
      headers.get(this.options.clientCertSpiffeIdHeaderName),
      decodeSanHeader(headers.get(this.options.clientCertDnsSansHeaderName)),
      decodeSanHeader(headers.get(this.options.clientCertUriSansHeaderName)),
      headers.get(this.options.clientCertHashHeaderName),
      headers.get(this.options.clientCertSerialNumberHeaderName),
      parseIfNotNull(headers.get(this.options.clientCertNotBeforeHeaderName)),
      parseIfNotNull(headers.get(this.options.clientCertNotAfterHeaderName)));
  }

  // -------------------------------------------------------------------------
  // Inner classes.
  // -------------------------------------------------------------------------

  public record Options(
    String clientIdHeaderName,
    String clientCertPresentHeaderName,
    String clientCertChainVerifiedHeaderName,
    String clientCertErrorHeaderName,
    String clientCertSpiffeIdHeaderName,
    String clientCertDnsSansHeaderName,
    String clientCertUriSansHeaderName,
    String clientCertHashHeaderName,
    String clientCertSerialNumberHeaderName,
    String clientCertNotBeforeHeaderName,
    String clientCertNotAfterHeaderName
  ) {
    public Set<String> allClientCertHeaders() {
      return Set.of(
        clientCertPresentHeaderName,
        clientCertChainVerifiedHeaderName,
        clientCertErrorHeaderName,
        clientCertSpiffeIdHeaderName,
        clientCertDnsSansHeaderName,
        clientCertUriSansHeaderName,
        clientCertHashHeaderName,
        clientCertSerialNumberHeaderName,
        clientCertNotBeforeHeaderName,
        clientCertNotAfterHeaderName);
    }
  }
}
