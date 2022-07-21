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

using Google.Solutions.WWAuth.Util;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace Google.Solutions.WWAuth.Data
{
    /// <summary>
    /// Result JSON.
    /// </summary>
    internal class PluggableAuthResult
    {
        [JsonProperty("version")]
        public int Version => 1;

        /// <summary>
        /// The status of the response. When true, the response must 
        /// contain the 3rd party token, token type, and expiration.
        /// The executable must also exit with exit code 0. When false, 
        /// the response must contain the error code and message fields 
        /// and exit with a non-zero value.
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; }

        /// <summary>
        /// The 3rd party subject token type. Must be 
        /// urn:ietf:params:oauth:token-type:jwt, 
        /// urn:ietf:params:oauth:token-type:id_token, or
        /// urn:ietf:params:oauth:token-type:saml2.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; }

        /// <summary>
        /// The 3rd party OIDC token.
        /// </summary>
        [JsonProperty("id_token")]
        public string IdToken { get; }

        /// <summary>
        /// The 3rd party SAML response.
        /// </summary>
        [JsonProperty("saml_response")]
        public string SamlResponse { get; }

        /// <summary>
        /// The 3rd party subject token expiration time in seconds 
        /// (unix epoch time).
        /// </summary>
        [JsonProperty("expiration_time")]
        public long ExpirationTime { get; }

        /// <summary>
        /// The error code string.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; }

        /// <summary>
        /// The error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; }

        public PluggableAuthResult(Exception exception)
        {
            exception = exception.Unwrap();

            this.Success = false;
            this.Message = exception.FullMessage();
            this.Code = exception.GetType().Name;
        }

        public PluggableAuthResult(ISubjectToken token)
        {
            this.Success = true;
            this.TokenType = token.Type.GetDescription();

            //
            // In case of a non-encrypted token, we should
            // have an expiry time. But even when using encrypted
            // tokens, we have to specify _some_ time (b/238142763).
            //
            Debug.Assert(token.IsEncrypted || token.Expiry != null);

            var expiry = token.Expiry ?? DateTimeOffset.UtcNow.AddMinutes(5);
            this.ExpirationTime = expiry.ToUnixTimeSeconds();

            if (token.Type == Data.SubjectTokenType.Saml2)
            {
                this.SamlResponse = token.Value;
            }
            else
            {
                this.IdToken = token.Value;
            }
        }

        public void WriteTo(TextWriter writer)
        {
            writer.Write(JsonConvert.SerializeObject(this,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
        }
    }
}
