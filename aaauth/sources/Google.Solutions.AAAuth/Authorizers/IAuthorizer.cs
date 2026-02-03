//
// Copyright 2026 Google LLC
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

using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Authorizers
{
    /// <summary>
    /// A strategy to perform an OAuth authorization flow using
    /// a specific identity provider, in a specific way.
    /// </summary>
    public interface IAuthorizer
    {
        /// <summary>
        /// Get metadata required for verifying the tokens obtained
        /// by this authorizer.
        /// </summary>
        Task<AuthorizerMetadata> GetMetadataAsync(
            CancellationToken cancellationToken);

        /// <summary>
        /// Create an authorize-URI to redirect the user to.
        /// </summary>
        /// <param name="clientId">Client ID, provided by user</param>
        /// <param name="scope">Scope, provided by user</param>
        /// <param name="passthroughState">State to incorporate in URI</param>
        /// <param name="redirectUri">Redirect URI to incorporate in URI</param>
        /// <returns></returns>
        Task<Uri> CreateAuthorizeUriAsync(
            string clientId,
            string? scope,
            string? passthroughState,
            Uri redirectUri,
            CancellationToken cancellationToken); // contains original state

        /// <summary>
        /// Redeem an authorization code and obtain tokens.
        /// </summary>
        Task<TokenResult> GetTokenAsync(
            ClientSecrets clientCredentials,
            string authorizationCode,
            Uri redirectUri,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Metadata for verifying the tokens obtained by this authorizer. 
    /// This metadata is typically a subset of the OpenID Connect
    /// of the identity provider used by this authorizer.
    /// </summary>
    public record AuthorizerMetadata(
        Uri Issuer,
        Uri JsonWebKeySetEndpoint,
        IReadOnlyList<string> SubjectTypesSupported,
        IReadOnlyList<string> IdTokenSigningAlgValuesSupported)
    { }
}
