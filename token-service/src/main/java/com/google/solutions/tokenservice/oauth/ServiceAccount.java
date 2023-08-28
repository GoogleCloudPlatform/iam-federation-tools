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

import com.google.api.client.googleapis.json.GoogleJsonResponseException;
import com.google.api.client.http.HttpRequestInitializer;
import com.google.api.client.json.gson.GsonFactory;
import com.google.api.client.json.webtoken.JsonWebToken;
import com.google.api.services.iamcredentials.v1.IAMCredentials;
import com.google.api.services.iamcredentials.v1.model.GenerateAccessTokenRequest;
import com.google.api.services.iamcredentials.v1.model.SignJwtRequest;
import com.google.auth.http.HttpCredentialsAdapter;
import com.google.auth.oauth2.GoogleCredentials;
import com.google.common.base.Preconditions;
import com.google.solutions.tokenservice.ApplicationVersion;
import com.google.solutions.tokenservice.URLHelper;
import com.google.solutions.tokenservice.UserId;
import com.google.solutions.tokenservice.platform.AccessDeniedException;
import com.google.solutions.tokenservice.platform.ApiException;
import com.google.solutions.tokenservice.platform.HttpTransport;
import com.google.solutions.tokenservice.platform.NotAuthenticatedException;

import java.io.IOException;
import java.net.URL;
import java.security.GeneralSecurityException;
import java.time.Duration;
import java.time.Instant;
import java.util.List;

/**
 * A service account.
 */
public class ServiceAccount {
  public static final String OAUTH_SCOPE = "https://www.googleapis.com/auth/cloud-platform";

  private final UserId id;
  private final HttpRequestInitializer requestInitializer;

  private String resourceName() {
    return String.format("projects/-/serviceAccounts/%s", this.id);
  }

  private IAMCredentials createClient() throws IOException
  {
    try {
      return new IAMCredentials
        .Builder(
          HttpTransport.newTransport(),
          new GsonFactory(),
          this.requestInitializer)
        .setApplicationName(ApplicationVersion.USER_AGENT)
        .build();
    }
    catch (GeneralSecurityException e) {
      throw new IOException("Creating a IAMCredentials client failed", e);
    }
  }

  public ServiceAccount(
    UserId id,
    GoogleCredentials credentials
  )  {
    Preconditions.checkNotNull(id, "email");
    Preconditions.checkNotNull(credentials, "credentials");

    this.id = id;
    this.requestInitializer = new HttpCredentialsAdapter(credentials);
  }

  public ServiceAccount(
    UserId id,
    StsAccessToken stsAccessToken
  )  {
    Preconditions.checkNotNull(id, "email");
    Preconditions.checkNotNull(stsAccessToken, "stsAccessToken");

    this.id = id;
    this.requestInitializer = httpRequest -> httpRequest
      .getHeaders()
      .put("Authorization", String.format("Bearer %s", stsAccessToken.value()));
  }

  /**
   * Sign a JWT using the Google-managed service account key.
   */
  public String signJwt(
    JsonWebToken.Payload payload
  ) throws ApiException, IOException {
    Preconditions.checkNotNull(payload, "payload");

    try
    {
      if (payload.getFactory() == null) {
        payload.setFactory(new GsonFactory());
      }

      var payloadJson = payload.toString();
      assert (payloadJson.startsWith("{"));

      var request = new SignJwtRequest()
        .setPayload(payloadJson);

      return createClient()
        .projects()
        .serviceAccounts()
        .signJwt(resourceName(), request)
        .execute()
        .getSignedJwt();
    }
    catch (GoogleJsonResponseException e) {
      switch (e.getStatusCode()) {
        case 400:
          throw new IllegalArgumentException(
            "Signing JWT failed",
            ApiException.from(e));
        case 401:
          throw new NotAuthenticatedException(
            "Not authenticated",
            ApiException.from(e));
        case 403:
          throw new AccessDeniedException(
            String.format("Access to service account '%s' was denied", this.id),
            ApiException.from(e));
        default:
          throw ApiException.from((GoogleJsonResponseException)e.fillInStackTrace());
      }
    }
  }

  /**
   * Impersonate the service account and obtain an access token.
   *
   * @param scopes requested scopes, fully qualified.
   * @param lifetime lifetime of requested token
   */
  public ServiceAccountAccessToken generateAccessToken(
    List<String> scopes,
    Duration lifetime
  ) throws ApiException, IOException {
    Preconditions.checkNotNull(scopes, "scopes");
    Preconditions.checkNotNull(lifetime, "lifetime");
    Preconditions.checkArgument(!lifetime.isNegative(), "lifetime");

    try {
      var request = new GenerateAccessTokenRequest()
        .setScope(scopes)
        .setLifetime(lifetime.toSeconds() + "s");

      var issueTime = Instant.now();
      var response = createClient()
        .projects()
        .serviceAccounts()
        .generateAccessToken(resourceName(), request)
        .execute();

      return new ServiceAccountAccessToken(
        response.getAccessToken(),
        String.join(" ", scopes),
        issueTime,
        Instant.parse(response.getExpireTime()));
    }
    catch (GoogleJsonResponseException e) {
      switch (e.getStatusCode()) {
        case 400:
          throw new IllegalArgumentException(
            "Generating access token failed",
            ApiException.from(e));
        case 401:
          throw new NotAuthenticatedException(
            "Not authenticated",
            ApiException.from(e));
        case 403:
          throw new AccessDeniedException(
            String.format("Access to service account '%s' was denied", this.id),
            ApiException.from(e));
        default:
          throw ApiException.from((GoogleJsonResponseException)e.fillInStackTrace());
      }
    }
  }

  /**
   * Get JWKS location for service account key set.
   */
  public URL jwksUrl() {
    return URLHelper.fromString(
      String.format("https://www.googleapis.com/service_accounts/v1/metadata/jwk/%s", this.id));
  }

  public UserId id() {
    return id;
  }

  @Override
  public String toString() {
    return this.id.toString();
  }
}
