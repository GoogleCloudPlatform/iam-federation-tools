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

namespace Google.Solutions.AAAuth.Iam
{
    /// <summary>
    /// Token type identifiers as defined in RFC 8693.
    /// </summary>
    public static class TokenTypes
    {
        /// <summary>
        /// Indicates that the token is an ID Token as defined in 
        /// Section 2 of [OpenID.Core].
        /// </summary>
        public const string IdToken
            = "urn:ietf:params:oauth:token-type:id_token";

        /// <summary>
        /// Indicates that the token is an OAuth 2.0 access token.
        /// </summary>
        public const string AccessToken
            = "urn:ietf:params:oauth:token-type:access_token";

        /// <summary>
        /// Indicates that the token is an OAuth 2.0 refresh token.
        /// </summary>
        public const string RefreshToken
            = "urn:ietf:params:oauth:token-type:refresh_token";
    }
}
