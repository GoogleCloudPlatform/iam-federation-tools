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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Google.Solutions.AAAuth.Web
{
    /// <summary>
    /// OpenID provider metadata.
    /// </summary>
    /// <see href="https://openid.net/specs/openid-connect-discovery-1_0.html#ProviderMetadata."/>
    public class OpenIdProviderMetadata
    {
        [JsonPropertyName("issuer")]
        public required string Issuer { get; init; }

        [JsonPropertyName("authorization_endpoint")]
        public required string AuthorizationEndpoint { get; init; }

        [JsonPropertyName("token_endpoint")]
        public required string TokenEndpoint { get; init; }

        [JsonPropertyName("jwks_uri")]
        public required string JsonWebKeySetEndpoint { get; init; }

        [JsonPropertyName("response_types_supported")]
        public required IReadOnlyList<string> ResponseTypesSupported { get; init; }

        [JsonPropertyName("subject_types_supported")]
        public required IReadOnlyList<string> SubjectTypesSupported { get; init; }

        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public required IReadOnlyList<string> IdTokenSigningAlgValuesSupported { get; init; }
    }
}
