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

using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Json;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Google.Solutions.WWAuth.Data
{
    /// <summary>
    /// JWT-formatted ID token or access token.
    /// </summary>
    internal class JsonWebToken : ISubjectToken
    {
        public SubjectTokenType Type => SubjectTokenType.Jwt;

        public bool IsEncrypted => false;

        public string Value { get; }

        public DateTimeOffset? Expiry { get; }

        public string Audience
            => this.Attributes.TryGet("assertion.aud") as string;

        public string Issuer
            => this.Attributes.TryGet("assertion.iss") as string;

        public IDictionary<string, object> Attributes { get; }

        public JsonWebToken(string value, DateTimeOffset expiry)
        {
            this.Value = value.ThrowIfNull(nameof(value));
            this.Expiry = expiry;
            this.Attributes = ExtractClaims(value);
        }

        //---------------------------------------------------------------------
        // Private methods.
        //---------------------------------------------------------------------

        private static IDictionary<string, object> ExtractClaims(string jwt)
        {
            var jwtParts = jwt
                .ThrowIfNullOrEmpty(nameof(jwt))
                .Split('.');
            if (jwtParts.Length != 3)
            {
                throw new InvalidJwtException(
                    "JWT must consist of Header, Payload, and Signature");
            }

            //
            // Deserialize the body, which might contain nested objects.
            //
            var body = NewtonsoftJsonSerializer
                .Instance
                .Deserialize<Dictionary<string, object>>(
                    Encoding.UTF8.GetString(UrlSafeBase64.Decode(jwtParts[1])));

            //
            // Flatten the body into a key/value pair list:
            //
            //   assertion.claim
            //   assertion.claim.nestedclaim
            //   ...
            //
            // Treat multi-valued attributes as arrays:
            //
            //   assertion.claim[<index>]
            //
            // This is the syntax used by pool provider mappings.
            //

            var claims = new Dictionary<string, object>();

            void Visit(string name, object value)
            {
                if (value is JArray jarray)
                {
                    for (var i = 0; i < jarray.Count; i++)
                    {
                        Visit($"{name}[{i}]", jarray[i]);
                    }
                }
                else if (value is JObject jobject)
                {
                    foreach (var property in jobject)
                    {
                        Visit($"{name}.{property.Key}", property.Value);
                    }
                }
                else if (value is JValue jvalue)
                {
                    claims.Add(name, jvalue.Value);
                }
                else
                {
                    claims.Add(name, value);
                }
            }

            foreach (var claim in body)
            {
                Visit($"assertion.{claim.Key}", claim.Value);
            }

            return claims;
        }

        //---------------------------------------------------------------------
        // Public methods.
        //---------------------------------------------------------------------

        public static JsonWebToken FromResponse(TokenResponse response)
        {
            return new JsonWebToken(
                response.AccessToken,
                DateTimeOffset.UtcNow.AddSeconds(response.ExpiresInSeconds.Value));
        }

    }
}
