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
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Xml;

namespace Google.Solutions.WWAuth.Data.Saml2
{
    /// <summary>
    /// SAML 2.0 assertion.
    /// </summary>
    internal class Assertion : ISubjectToken
    {
        private readonly Saml2SecurityToken token;

        public SubjectTokenType Type => SubjectTokenType.Saml2;

        public bool IsEncrypted => false;

        public string Value { get; }

        public string Audience
            => this.token.Assertion?
                    .Conditions?
                    .AudienceRestrictions
                    .EnsureNotNull()
                    .FirstOrDefault()?
                    .Audiences?
                    .EnsureNotNull()
                    .FirstOrDefault()?
                    .ToString();

        public string Issuer
            => this.token.Assertion.Issuer.Value;

        public DateTimeOffset? Expiry
            => this.token.Assertion.Conditions?.NotOnOrAfter;

        public IDictionary<string, object> Attributes
        {
            get
            {
                var attributes = new Dictionary<string, object>();

                //
                // Return NameID.
                //
                if (this.token.Assertion.Subject?.NameId?.Value is var nameId && nameId != null)
                {
                    attributes["assertion.subject"] = nameId;
                }

                //
                // Return all attributes.
                //
                // NB. Attribute names are typically URLs. Therefore, we have
                // to use
                //
                //   assertion.attributes['<name'>]
                //
                // instead of
                //
                //   assertion.<name>
                //
                // Treat multi-valued attributes as arrays:
                //
                //   assertion.<name>[<index>]
                //
                // This is the syntax used by pool provider mappings.
                //

                foreach (var statement in this.token.Assertion.Statements
                    .EnsureNotNull()
                    .OfType<Saml2AttributeStatement>()
                    .SelectMany(a => a.Attributes))
                {
                    var mangledName = $"assertion.attributes['{statement.Name}']";
                    if (statement.Values.Count > 1)
                    {
                        for (int i = 0; i < statement.Values.Count; i++)
                        {
                            attributes[$"{mangledName}[{i}]"] = statement.Values[i];
                        }
                    }
                    else if (statement.Values.Count == 1)
                    {
                        attributes[$"{mangledName}"] = statement.Values[0];
                    }
                }

                return attributes;
            }
        }

        private static Saml2SecurityToken ParseSaml2SecurityToken(XmlElement xml)
        {
            using (var reader = new XmlNodeReader(xml))
            {
                var tokenHandler = new Saml2SecurityTokenHandler()
                {
                    Configuration = new SecurityTokenHandlerConfiguration()
                };

                return (Saml2SecurityToken)tokenHandler.ReadToken(reader);
            }
        }

        internal Assertion(
            Saml2SecurityToken token,
            string encodedToken)
        {
            this.token = token;
            this.Value = encodedToken;
        }

        public Assertion(GenericXmlSecurityToken token)
            : this(
                  ParseSaml2SecurityToken(token.TokenXml),
                  Convert.ToBase64String(
                      Encoding.UTF8.GetBytes(
                          token.TokenXml.OuterXml)))
        {
        }
    }
}
