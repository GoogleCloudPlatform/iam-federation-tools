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
import com.google.common.base.Preconditions;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.platform.ApiException;

import jakarta.enterprise.context.ApplicationScoped;
import java.io.IOException;
import java.net.URL;
import java.time.Duration;
import java.time.Instant;
import java.util.UUID;

/**
 * Issuer (and signer) for ID tokens.
 *
 * To avoid having to manage a dedicated signing key pair, the class uses
 * a service account and its Google-managed key pair to sign tokens.
 */
@ApplicationScoped
public class IdTokenIssuer {
  private final Options options;
  private final ServiceAccount serviceAccount;

  public IdTokenIssuer(
    Options options,
    ServiceAccount serviceAccount
  ) {
    Preconditions.checkNotNull(serviceAccount, "serviceAccount");
    Preconditions.checkNotNull(options, "options");
    Preconditions.checkArgument(!options.tokenExiry.isNegative());

    this.options = options;
    this.serviceAccount = serviceAccount;
  }

  /**
   * @return public URL to JWKS that can be used to verify tokens.
   */
  public URL jwksUrl() {
    return serviceAccount.jwksUrl();
  }

  /**
   * @return OIDC-compliant issuer ID.
   */
  public String id() {
    var issuer = this.options.id().toString();
    if (issuer.endsWith("/")) {
      issuer = issuer.substring(0, issuer.length() - 1);
    }

    return issuer;
  }

  /**
   * Issue a signed ID token.
   *
   * @param client
   * @param payload extra claims
   * @return signed token
   */
  public IdToken issueIdToken(
    AuthenticatedClient client,
    JsonWebToken.Payload payload
  ) throws ApiException, IOException {
    Preconditions.checkNotNull(client, "client");
    Preconditions.checkNotNull(payload, "payload");

    //
    // Add standard set of JWT claims based on
    // https://datatracker.ietf.org/doc/html/rfc7519#section-4
    //
    // - iss: the base URL of this service (so that OIDC Disovery works).
    // - aud: the audience, which is always a workload identity pool provider.
    // - iat: the time of issue.
    // - exp: the time of expiry.
    // - jti: a unique identifier for the JWT.
    //
    var issueTime = Instant.now();
    var expiryTime = issueTime.plus(this.options.tokenExiry);

    var jwtPayload = payload
      .setIssuer(id())
      .setIssuedAtTimeSeconds(issueTime.getEpochSecond())
      .setAudience(this.options.tokenAudience.toString())
      .setExpirationTimeSeconds(expiryTime.getEpochSecond())
      .setJwtId(UUID.randomUUID().toString());

    return new IdToken(
      this.serviceAccount.signJwt(jwtPayload),
      issueTime,
      expiryTime);
  }

  // -------------------------------------------------------------------------
  // Inner classes.
  // -------------------------------------------------------------------------

  public record Options(
    URL id,
    URL tokenAudience,
    Duration tokenExiry
  ) {}
}
