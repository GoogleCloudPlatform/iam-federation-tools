//
// Copyright 2022 Google LLC
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

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Google.Solutions.WWAuth.Data
{
    /// <summary>
    /// Generic interface representing any kind of security token.
    /// </summary>
    public interface ISubjectToken
    {
        /// <summary>
        /// Type of token.
        /// </summary>
        SubjectTokenType Type { get; }

        /// <summary>
        /// Raw token value.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Indended audience.
        /// </summary>
        string Audience { get; }

        /// <summary>
        /// Issuer of token.
        /// </summary>
        string Issuer { get; }

        /// <summary>
        /// Expiry time of token.
        /// </summary>
        DateTimeOffset? Expiry { get; }

        /// <summary>
        /// Returns the flattened list of (unverified) attributes
        /// in the syntax expected by workload identity pool providers.
        /// </summary>
        IDictionary<string, object> Attributes { get; }

        /// <summary>
        /// Checks if the token is encrypted. For encrypted tokens,
        /// attributes, expiry, etc will be null.
        /// </summary>
        bool IsEncrypted { get; }
    }

    public enum SubjectTokenType
    {
        [Description("urn:ietf:params:oauth:token-type:jwt")]
        Jwt,

        [Description("urn:ietf:params:oauth:token-type:id_token")]
        IdToken,

        [Description("urn:ietf:params:oauth:token-type:saml2")]
        Saml2
    }
}
