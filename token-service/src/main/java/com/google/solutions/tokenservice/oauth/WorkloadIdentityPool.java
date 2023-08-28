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

import com.google.api.client.googleapis.json.GoogleJsonError;
import com.google.api.client.googleapis.json.GoogleJsonResponseException;
import com.google.api.client.http.HttpResponse;
import com.google.api.client.http.HttpResponseException;
import com.google.api.client.json.gson.GsonFactory;
import com.google.api.client.util.Key;
import com.google.api.services.sts.v1.CloudSecurityToken;
import com.google.api.services.sts.v1.model.GoogleIdentityStsV1ExchangeTokenRequest;
import com.google.common.base.Preconditions;
import com.google.solutions.tokenservice.ApplicationVersion;
import com.google.solutions.tokenservice.URLHelper;
import com.google.solutions.tokenservice.UserId;
import com.google.solutions.tokenservice.platform.HttpTransport;

import javax.enterprise.context.ApplicationScoped;
import java.io.IOException;
import java.net.URL;
import java.security.GeneralSecurityException;
import java.time.Instant;

/**
 * A workload identity pool.
 */
@ApplicationScoped
public class WorkloadIdentityPool {
  private static final String GRANT_TYPE = "urn:ietf:params:oauth:grant-type:token-exchange";
  private static final String ACCESS_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:access_token";
  private static final String ID_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:id_token";

  private final Options options;

  public WorkloadIdentityPool(Options options) {
    this.options = options;
  }

  private CloudSecurityToken createStsClient() throws IOException
  {
    try {
      return new CloudSecurityToken
        .Builder(
          HttpTransport.newTransport(),
          new GsonFactory(),
          httpRequest -> {})
        .setApplicationName(ApplicationVersion.USER_AGENT)
        .build();
    }
    catch (GeneralSecurityException e) {
      throw new IOException("Creating an STS client failed", e);
    }
  }

  /**
   * Exchange an ID token for an STS access token.
   */
  public StsAccessToken issueAccessToken(
    IdToken idToken,
    String scope
  ) throws IOException {
    Preconditions.checkNotNull(idToken, "idToken");
    Preconditions.checkNotNull(scope, "scope");

    try {
      var client = createStsClient();
      var requestBody = new GoogleIdentityStsV1ExchangeTokenRequest()
        .setGrantType(GRANT_TYPE)
        .setAudience(this.options.audience())
        .setScope(scope)
        .setRequestedTokenType(ACCESS_TOKEN_TYPE)
        .setSubjectToken(idToken.value())
        .setSubjectTokenType(ID_TOKEN_TYPE);

      //
      // The token API returns errors in OAuth format, not in the standard
      // Google API error format as the client library expects.
      //
      // Add hook to extract error details.
      //
      var request = client.v1().new Token(requestBody) {
        @Override
        protected GoogleJsonResponseException newExceptionOnError(HttpResponse response) {
          try {
            var builder = new HttpResponseException.Builder(
              response.getStatusCode(),
              response.getStatusMessage(),
              response.getHeaders());

            var errorDetails = client
              .getJsonFactory()
              .fromInputStream(response.getContent(), TokenErrorDetails.class);

            return new GoogleJsonResponseException(builder, errorDetails.toError());
          }
          catch (IOException e)
          {
            return super.newExceptionOnError(response);
          }
        }
      };

      var issueTime = Instant.now();
      var response = request.execute();

      return new StsAccessToken(
        response.getAccessToken(),
        scope,
        issueTime,
        issueTime.plusSeconds(response.getExpiresIn()));
    }
    catch (GoogleJsonResponseException e) {
      switch (e.getStatusCode()) {
        case 400:
          throw new IllegalArgumentException(e.getDetails() != null
            ? e.getDetails().getMessage()
            : e.getMessage());

        default:
          throw (GoogleJsonResponseException) e.fillInStackTrace();
      }
    }
  }

  /**
   * Use an STS token to impersonate a service account.
   *
   * @param serviceAccountId
   * @param accessToken
   * @return
   */
  public ServiceAccount impersonateServiceAccount(
    UserId serviceAccountId,
    StsAccessToken accessToken
  ) {
    return new ServiceAccount(serviceAccountId, accessToken);
  }

  // -------------------------------------------------------------------------
  // Inner classes.
  // -------------------------------------------------------------------------

  public record Options(
    long projectNumber,
    String poolId,
    String providerId
  ) {
    public String audience() {
      return String.format(
        "//iam.googleapis.com/projects/%d/locations/global/workloadIdentityPools/%s/providers/%s",
        this.projectNumber(),
        this.poolId(),
        this.providerId());
    }

    public URL expectedTokenAudience() {
      return URLHelper.fromString("https:" + audience());
    }
  }

  public static class TokenErrorDetails {
    @Key
    private String error;

    @Key
    private String error_description;

    public GoogleJsonError toError() {
      var error = new GoogleJsonError();
      error.setMessage(String.format(
        "Token exchange failed with code %s: %s",
        this.error,
        this.error_description));
      return error;
    }
  }
}
