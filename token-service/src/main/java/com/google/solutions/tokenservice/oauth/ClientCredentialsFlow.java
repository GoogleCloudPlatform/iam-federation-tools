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
import com.google.api.client.util.GenericData;
import com.google.common.base.Preconditions;
import com.google.common.base.Strings;
import com.google.solutions.tokenservice.UserId;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.platform.ApiException;
import com.google.solutions.tokenservice.platform.LogAdapter;
import com.google.solutions.tokenservice.web.LogEvents;

import java.io.IOException;
import java.time.Duration;
import java.util.List;

/**
 * Abstract implementation of the OAuth client credentials flow.
 */
public abstract class ClientCredentialsFlow implements AuthenticationFlow {
  private final IdTokenIssuer issuer;
  private final WorkloadIdentityPool workloadIdentityPool;
  protected final LogAdapter logAdapter;

  public ClientCredentialsFlow(
    IdTokenIssuer issuer,
    WorkloadIdentityPool workloadIdentityPool,
    LogAdapter logAdapter
  ) {
    Preconditions.checkNotNull(issuer, "issuer");
    Preconditions.checkNotNull(workloadIdentityPool, "workloadIdentityPool");
    Preconditions.checkNotNull(logAdapter, "logAdapter");

    this.issuer = issuer;
    this.workloadIdentityPool = workloadIdentityPool;
    this.logAdapter = logAdapter;
  }

  /**
   * Identify and authenticate the client.
   */
  protected abstract AuthenticatedClient authenticateClient(
    AuthenticationRequest request
  );

  /**
   * Issue an ID token for an authenticated client.
   */
  protected IdToken issueIdToken(
    AuthenticatedClient client
  ) throws ApiException, IOException {
    Preconditions.checkNotNull(client, "client");

    //
    // In addition to the standard iss/exp/iat that all ID tokens need
    // to contain, we include the following "extra" claims:
    //
    // - amr:    the name of the flow, could be used in a workload identity pool
    //           provider's attribute condition.
    // - client: JSON object containing claims about the client. The exact set
    //           of claims depends on the flow/subclass.
    //
    // NB. This is a client-credentials flow, so we're authenticating clients, not
    // end users. Thus, we don't set a 'sub' claim.
    //

    var idTokenPayload = new JsonWebToken.Payload()
      .set("amr", new String[] { name().toLowerCase() })
      .set("client_id", client.clientId());

    var clientClaims = new GenericData();
    clientClaims.putAll(client.additionalClaims());
    idTokenPayload.put("client", clientClaims);

    return this.issuer.issueIdToken(
      client,
      idTokenPayload);
  }

  /**
   * Issue an access token.
   */
  protected AccessToken issueAccessToken(
    AuthenticationRequest request,
    AuthenticatedClient client,
    IdToken idToken
  )  throws ApiException, IOException {
    Preconditions.checkNotNull(request, "request");
    Preconditions.checkNotNull(client, "client");
    Preconditions.checkNotNull(idToken, "idToken");

    var scope = request.parameters().getFirst("scope");
    if (Strings.isNullOrEmpty(scope)) {
      //
      // No scope specified, the client isn't interested in an access token.
      //
      return null;
    }

    //
    // Use the ID token to request an access token from the
    // workload identity pool.
    //
    var stsAccessToken = this.workloadIdentityPool.issueAccessToken(idToken, scope);

    //
    // If requested, use the STS token to impersonate a service
    // account.
    //
    var serviceAccountEmail = request.parameters().getFirst("service_account");
    if (!Strings.isNullOrEmpty(serviceAccountEmail)) {

      var serviceAccount = this.workloadIdentityPool.impersonateServiceAccount(
        new UserId(serviceAccountEmail),
        stsAccessToken);

      //
      // Apply duration from ID token.
      //
      return serviceAccount.generateAccessToken(
        List.of(scope),
        Duration.between(idToken.issueTime(), idToken.expiryTime()));
    }
    else {
      return stsAccessToken;
    }
  }

  //---------------------------------------------------------------------------
  // AuthenticationFlow.
  //---------------------------------------------------------------------------

  @Override
  public String grantType() {
    return "client_credentials";
  }

  @Override
  public boolean canAuthenticate(AuthenticationRequest request) {
    Preconditions.checkNotNull(request, "request");
    return true;
  }

  @Override
  public final Authentication authenticate(
    AuthenticationRequest request
  ) throws Authentication.AuthenticationException {
    Preconditions.checkNotNull(request, "request");

    //
    // Authenticate the client.
    //
    AuthenticatedClient client;
    try
    {
      client = authenticateClient(request);
    }
    catch (Exception e) {
      throw new Authentication.InvalidClientException(
        "The client or its credentials are invalid", e);
    }

    //
    // Issue an ID token.
    //
    IdToken idToken;
    try {
      idToken = issueIdToken(client);
    }
    catch (Exception e) {
      throw new Authentication.TokenIssuanceException(
        String.format("Issuing ID token for client '%s' failed", client.clientId()),
        e);
    }

    //
    // Issue an access token (if requested).
    //
    try {
      var accessToken = issueAccessToken(request, client, idToken);

      if (accessToken instanceof StsAccessToken stsAccessToken)
      {
        this.logAdapter
          .newInfoEntry(
            LogEvents.API_TOKEN,
            String.format(
              "Issued ID token and STS access token for client '%s' and scope '%s'",
              client.clientId(),
              stsAccessToken.scope()))
          .write();

      }
      else if (accessToken instanceof ServiceAccountAccessToken saAccessToken)
      {
        this.logAdapter
          .newInfoEntry(
            LogEvents.API_TOKEN,
            String.format(
              "Issued ID token and service account access token for client '%s' and scope '%s'",
              client.clientId(),
              saAccessToken.scope()))
          .write();

      }
      else {
        this.logAdapter
          .newInfoEntry(
            LogEvents.API_TOKEN,
            String.format("Issued ID token for client '%s'", client.clientId()))
          .write();
      }

      return new Authentication(client, idToken, accessToken);
    }
    catch (Exception e) {
      throw new Authentication.TokenIssuanceException(
        String.format("Issuing access token for client '%s' failed", client.clientId()),
        e);
    }
  }
}
