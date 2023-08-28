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
import com.google.solutions.tokenservice.oauth.AuthenticationRequest;
import com.google.solutions.tokenservice.oauth.ClientCredentialsFlow;
import com.google.solutions.tokenservice.oauth.IdTokenIssuer;
import com.google.solutions.tokenservice.oauth.WorkloadIdentityPool;
import com.google.solutions.tokenservice.oauth.client.AuthenticatedClient;
import com.google.solutions.tokenservice.oauth.client.ClientPolicy;
import com.google.solutions.tokenservice.platform.LogAdapter;

/**
 * Flow that authenticates clients using mTLS.
 *
 * Based on RFC8705 (OAuth 2.0 Mutual-TLS Client Authentication
 * and Certificate-Bound Access Tokens).
 */
public abstract class MtlsClientCredentialsFlow extends ClientCredentialsFlow {

  protected final ClientPolicy clientPolicy;

  public MtlsClientCredentialsFlow(
    ClientPolicy clientPolicy,
    IdTokenIssuer issuer,
    WorkloadIdentityPool workloadIdentityPool,
    LogAdapter logAdapter
  ) {
    super(issuer, workloadIdentityPool, logAdapter);

    Preconditions.checkNotNull(clientPolicy, "clientPolicy");

    this.clientPolicy = clientPolicy;
  }

  /**
   * Return the verified set of attributes from the client's mTLS certificate.
   */
  protected abstract MtlsClientAttributes getVerifiedClientAttributes(
    AuthenticationRequest request
  );

  //---------------------------------------------------------------------------
  // Overrides.
  //---------------------------------------------------------------------------

  @Override
  public String authenticationMethod() {
    return "tls_client_auth";
  }

  @Override
  protected AuthenticatedClient authenticateClient(AuthenticationRequest request) {
    Preconditions.checkNotNull(request, "request");

    var clientAttributes = getVerifiedClientAttributes(request);
    return this.clientPolicy.authenticateClient(clientAttributes);
  }
}
