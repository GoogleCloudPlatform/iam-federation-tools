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

using Google.Solutions.WWAuth.Data.Saml2;
using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Google.Solutions.WWAuth.Test.Data.Saml2
{
    [TestFixture]
    public class TestAuthenticationRequest
    {
        private const string SampleDestination = "http://idp.example.com/";
        private const string SampleRelyingPartyId = "http://rp.example.com/";
        private const string SampleAcs = "http://acs.example.com/";

        private static X509Certificate2 CreateSelfSignedCertificate(RSA key)
        {
            return new CertificateRequest(
                    "CN=test",
                    key,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1)
                .CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddMinutes(-5),
                    DateTimeOffset.UtcNow.AddMinutes(5));
        }

        //---------------------------------------------------------------------
        // ToString.
        //---------------------------------------------------------------------

        [Test]
        public void WhenNoCertificateProvided_ThenToStringReturnsEncodedRequest()
        {
            var request = new AuthenticationRequest(
                SampleDestination,
                SampleRelyingPartyId,
                SampleAcs);

            var encoded = request.ToString();

            using (var raw = new MemoryStream(Convert.FromBase64String(encoded)))
            using (var inflated = new DeflateStream(raw, CompressionMode.Decompress))
            using (var reader = new StreamReader(inflated, new UTF8Encoding(false)))
            {
                var xml = reader.ReadToEnd();
                StringAssert.Contains("<AuthnRequest", xml);
            }
        }

        [Test]
        public void WhenCertificateProvided_ThenToStringReturnsSignedAndEncodedRequest()
        {
            using (var key = RSA.Create())
            using (var cert = CreateSelfSignedCertificate(key))
            {
                var request = new AuthenticationRequest(
                    SampleDestination,
                    SampleRelyingPartyId,
                    SampleAcs)
                {
                    SigningCertificate = cert
                };

                var encoded = request.ToString();

                using (var raw = new MemoryStream(Convert.FromBase64String(encoded)))
                using (var inflated = new DeflateStream(raw, CompressionMode.Decompress))
                using (var reader = new StreamReader(inflated, new UTF8Encoding(false)))
                {
                    var xml = reader.ReadToEnd();
                    StringAssert.Contains("Destination=", xml);
                    StringAssert.Contains("<AuthnRequest", xml);
                    StringAssert.Contains("<X509Certificate>", xml);
                    StringAssert.Contains("<SignatureValue>", xml);
                }
            }
        }
    }
}
