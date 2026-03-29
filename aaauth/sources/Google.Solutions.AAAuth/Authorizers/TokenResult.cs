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

using System.Text.Json.Serialization;

namespace Google.Solutions.AAAuth.Authorizers
{
    public class TokenResult
    {
        /// <summary>
        /// Token type as specified in 
        /// http://tools.ietf.org/html/rfc6749#section-7.1.
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "bearer";

        /// <summary>
        /// Access token, if provided.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh token, if provided.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// ID token, as specified in 
        /// http://tools.ietf.org/html/draft-ietf-oauth-json-web-token
        /// </summary>
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        /// <summary>
        /// Remaining lifetime of the access token, in seconds.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public long? ExpiresInSeconds { get; set; }

        /// <summary>
        /// Scope, if provided
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
