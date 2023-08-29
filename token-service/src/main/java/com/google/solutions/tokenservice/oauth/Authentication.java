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

import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;

/**
 * Result of a successful authentication flow.
 *
 * @param client Client that was used to authenticate.
 * @param idToken ID Token for the authenticated principal.
 * @param accessToken Access token for the authenticated principal, can be null.
 */
public record Authentication(
  AuthenticatedClient client,

  IdToken idToken,

  AccessToken accessToken
) {

  public static abstract class AuthenticationException extends Exception {
    public AuthenticationException(String message) {
      super(message);
    }

    public AuthenticationException(String message, Exception cause) {
      super(message, cause);
    }
  }

  public static class InvalidClientException extends AuthenticationException {
    public InvalidClientException(String message, Exception cause) {
      super(message, cause);
    }
  }

  public static class TokenIssuanceException extends AuthenticationException {
    public TokenIssuanceException(String message, Exception cause) {
      super(message, cause);
    }
  }
}