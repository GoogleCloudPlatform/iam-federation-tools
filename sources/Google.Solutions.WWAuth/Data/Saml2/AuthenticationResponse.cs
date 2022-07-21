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

using Google.Apis.Util;
using Google.Solutions.WWAuth.Util;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Google.Solutions.WWAuth.Data.Saml2
{
    /// <summary>
    /// Saml 2.0 authentication response, possibly containing
    /// an assertion.
    /// </summary>
    internal abstract class AuthenticationResponse : ISubjectToken
    {
        public SubjectTokenType Type => SubjectTokenType.Saml2;

        public string Value { get; }

        public string Issuer { get; }

        public abstract bool IsEncrypted { get; }

        public abstract string Audience { get; }

        public abstract DateTimeOffset? Expiry { get; }

        public abstract IDictionary<string, object> Attributes { get; }

        protected AuthenticationResponse(
            string issuer,
            string rawValue)
        {
            this.Issuer = issuer.ThrowIfNullOrEmpty(nameof(issuer));
            this.Value = rawValue.ThrowIfNullOrEmpty(nameof(rawValue));
        }

        public static AuthenticationResponse Parse(string encodedResponse)
        {
            try
            {
                var xml = Encoding.UTF8.GetString(
                    Convert.FromBase64String(encodedResponse));

                var deserializer = new XmlSerializer(typeof(Saml2Schema.Response));
                var response = (Saml2Schema.Response)deserializer.Deserialize(
                    new StringReader(xml));

                if (response.Version != "2.0")
                {
                    throw new InvalidSamlResponseException(
                        "Unsupported SAML version");
                }
                else if (string.IsNullOrEmpty(response.ID))
                {
                    throw new InvalidSamlResponseException(
                        "Malformed SAML response: Missing required attribute: ID");
                }
                else if (string.IsNullOrEmpty(response.Issuer))
                {
                    throw new InvalidSamlResponseException(
                        "Malformed SAML response: Missing required attribute: Issuer");
                }
                else if (response.IssueInstant == DateTime.MinValue)
                {
                    throw new InvalidSamlResponseException(
                        "Malformed SAML response: Missing required attribute: IssueInstant");
                }
                else if (string.IsNullOrEmpty(response.Destination))
                {
                    throw new InvalidSamlResponseException(
                        "Malformed SAML response: Missing required attribute: Destination");
                }
                else if (response.Status?.StatusCode?.Value == "urn:oasis:names:tc:SAML:2.0:status:Responder" &&
                    string.IsNullOrEmpty(response.Status?.StatusMessage) &&
                    string.IsNullOrEmpty(response.Status.StatusCode?.StatusCode?.Value))
                {
                    throw new InvalidSamlResponseException(
                        $"Server rejected request, possible because of mismatched signing settings");
                }
                else if (response.Status?.StatusCode?.Value !=
                    "urn:oasis:names:tc:SAML:2.0:status:Success")
                {
                    throw new InvalidSamlResponseException(
                        $"Request failed with status code {response.Status?.StatusCode?.Value}\n\n" +
                        $"Details: {response.Status?.StatusMessage ?? "-"}\n" +
                        $"Detail status code {response.Status.StatusCode?.StatusCode?.Value ?? "-"}");
                }

                //
                // There should be either an <Assertion/> or <EncryptedAssertion/> element,
                // but we don't know which.
                //

                var assertionElement = response.Assertion
                    .EnsureNotNull()
                    .FirstOrDefault(n => n.NamespaceURI == "urn:oasis:names:tc:SAML:2.0:assertion");

                switch (assertionElement.Name)
                {
                    case "Assertion":
                        //
                        // Response contains non-encrypted assertion. We can parse
                        // this to extract attributes, etc.
                        //

                        using (var assertionReader = new XmlNodeReader(assertionElement))
                        {
                            var tokenHandler = new Saml2SecurityTokenHandler()
                            {
                                Configuration = new SecurityTokenHandlerConfiguration()
                            };

                            tokenHandler.Configuration.CertificateValidationMode =
                                System.ServiceModel.Security.X509CertificateValidationMode.None;

                            var assertion = new Assertion(
                                (Saml2SecurityToken)tokenHandler.ReadToken(assertionReader),
                                null);

                            return new Saml2ResponseWithPlaintextAssertion(
                                encodedResponse,
                                assertion);
                        }

                    case "EncryptedAssertion":
                        //
                        // Response contains an encrypted assertion. We can't do
                        // much with that.
                        //
                        return new Saml2ResponseWithEncryptedAssertion(
                            response.Issuer,
                            encodedResponse);

                    default:
                        throw new InvalidSamlResponseException(
                            $"SAML Response does not contain an assertion: {assertionElement}");
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidSamlResponseException("Failed to parse SAML respose", e);
            }
        }

        /// <summary>
        /// SAML 2.0 response with an embedded encrypted SAML assertion.
        /// </summary>
        private class Saml2ResponseWithEncryptedAssertion : AuthenticationResponse
        {
            public override bool IsEncrypted => true;

            public override string Audience => null;

            public override DateTimeOffset? Expiry => null;

            public override IDictionary<string, object> Attributes
                => new Dictionary<string, object>();

            public Saml2ResponseWithEncryptedAssertion(
                string issuer,
                string rawValue)
                : base(issuer, rawValue)
            {
            }
        }

        /// <summary>
        /// SAML 2.0 response with an embedded plaintext SAML assertion. The
        /// assertion may or may not be signed, we don't care.
        /// </summary>
        private class Saml2ResponseWithPlaintextAssertion : AuthenticationResponse
        {
            private readonly Assertion assertion;

            public override bool IsEncrypted => false;

            public override string Audience => this.assertion.Audience;

            public override DateTimeOffset? Expiry => this.assertion.Expiry;

            public override IDictionary<string, object> Attributes => this.assertion.Attributes;

            public Saml2ResponseWithPlaintextAssertion(
                string rawValue,
                Assertion assertion)
                : base(assertion.Issuer, rawValue)
            {
                this.assertion = assertion.ThrowIfNull(nameof(assertion));
            }
        }
    }

    public class InvalidSamlResponseException : Exception
    {
        public InvalidSamlResponseException(string message) : base(message)
        {
        }

        public InvalidSamlResponseException(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }
    }
}
