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
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Google.Solutions.WWAuth.Data.Saml2
{
    /// <summary>
    /// Saml 2.0 authentication request.
    /// </summary>
    internal class AuthenticationRequest
    {
        /// <summary>
        /// AD FS Relying Party ID . Typed as string (and not Uri) to prevent
        /// canonicaliuation, which might break the STS token exchange.
        /// </summary>
        public string RelyingPartyId { get; }

        /// <summary>
        /// ACS URL. This can be a dummy URL such as 'https://app.example/', but
        /// it must be configured in ADFS.
        /// </summary>
        public string AssertionConsumerServiceUrl { get; }

        /// <summary>
        /// Destination URL, typically the URL of the IdP.
        /// </summary>
        public string Destination { get; }

        public X509Certificate2 SigningCertificate { get; set; }

        public AuthenticationRequest(
            string destination,
            string relyingPartyId,
            string acsUrl)
        {
            if (!Uri.IsWellFormedUriString(relyingPartyId, UriKind.Absolute))
            {
                throw new ArgumentException("Relying party ID must be a URI");
            }

            if (!Uri.IsWellFormedUriString(acsUrl, UriKind.Absolute))
            {
                throw new ArgumentException("ACS must be a URI");
            }

            if (!Uri.IsWellFormedUriString(destination, UriKind.Absolute))
            {
                throw new ArgumentException("Destination must be a URI");
            }

            this.RelyingPartyId = relyingPartyId.ThrowIfNull(nameof(relyingPartyId));
            this.AssertionConsumerServiceUrl = acsUrl.ThrowIfNull(nameof(acsUrl));
            this.Destination = destination.ThrowIfNull(nameof(destination));
        }

        private XmlDocument ToDocument()
        {
            var doc = new XmlDocument();

            using (var writer = doc.CreateNavigator().AppendChild())
            {
                var request = new Saml2Schema.AuthnRequest()
                {
                    ID = "_" + Guid.NewGuid().ToString(),
                    AssertionConsumerServiceURL = this.AssertionConsumerServiceUrl,
                    Destination = this.Destination,
                    IsPassive = true,
                    IssueInstant = DateTime.UtcNow,
                    Issuer = this.RelyingPartyId,
                    ProtocolBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST",
                    Version = "2.0",
                    NameIDPolicy = new Saml2Schema.AuthnRequestNameIDPolicy()
                    {
                        AllowCreate = true,
                        Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"
                    }
                };

                new XmlSerializer(typeof(Saml2Schema.AuthnRequest)).Serialize(writer, request);
            }

            return doc;
        }

        /// <summary>
        /// Return the deflated, base64 encoded representation of
        /// the request.
        /// </summary>
        public override string ToString()
        {
            using (var output = new MemoryStream())
            {
                using (var zip = new DeflateStream(output, CompressionMode.Compress))
                using (var streamWriter = new StreamWriter(zip, new UTF8Encoding(false)))
                using (var writer = XmlWriter.Create(streamWriter))
                {
                    var xml = ToDocument();

                    if (this.SigningCertificate != null)
                    {
                        var signedXml = new SignedXml(xml.DocumentElement)
                        {
                            SigningKey = this.SigningCertificate.GetRSAPrivateKey()
                        };

                        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                        signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";

                        //
                        // Sign entire document using "SAML style" transforms.
                        //
                        var reference = new Reference()
                        {
                            Uri = string.Empty
                        };
                        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                        reference.AddTransform(new XmlDsigExcC14NTransform());
                        signedXml.AddReference(reference);

                        //
                        // Embed certificate.
                        //
                        var keyInfo = new KeyInfo();
                        keyInfo.AddClause(new KeyInfoX509Data(this.SigningCertificate));
                        signedXml.KeyInfo = keyInfo;

                        //
                        // Add signature after the Issuer element.
                        //
                        signedXml.ComputeSignature();

                        var issuer = xml.DocumentElement
                            .ChildNodes
                            .Cast<XmlNode>()
                            .OfType<XmlElement>()
                            .First(e => e.Name == "Issuer");

                        xml.DocumentElement.InsertAfter(
                            signedXml.GetXml(),
                            issuer);
                    }

                    xml.WriteTo(writer);
                }

                return Convert.ToBase64String(output.ToArray());
            }
        }
    }
}