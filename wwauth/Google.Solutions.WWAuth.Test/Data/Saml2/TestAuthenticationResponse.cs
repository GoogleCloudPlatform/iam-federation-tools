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

using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Data.Saml2;
using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Google.Solutions.WWAuth.Test.Data.Saml2
{
    [TestFixture]
    public class TestAuthenticationResponse
    {
        private const string UnsignedResponseContainingUnsignedAssertion = @"<samlp:Response ID='_3b' Version='2.0' 
            IssueInstant='2022-07-04T02:46:50.052Z' 
            Destination='https://sts.googleapis.com/v1/token' 
            Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified' 
            InResponseTo='_54430425-0c95-45dc-9c87-2452431f014a' 
            xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
          <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://example.org/adfs/services/trust</Issuer>
          <samlp:Status>
            <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Success' />
          </samlp:Status>
          <Assertion 
            ID='_04dad8c2-a00a-497b-9596-49cbbaf35821' 
            IssueInstant='2022-07-04T02:46:50.052Z' 
            Version='2.0' 
            xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>
            <Issuer>http://example.org/adfs/services/trust</Issuer>
            <Subject>
              <NameID Format='urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified'>bob@example.org</NameID>
              <SubjectConfirmation Method='urn:oasis:names:tc:SAML:2.0:cm:bearer'>
                <SubjectConfirmationData
                    InResponseTo='_54430425-0c95-45dc-9c87-2452431f014a' 
                    NotOnOrAfter='2022-07-04T02:51:50.052Z' 
                    Recipient='https://sts.googleapis.com/v1/token' />
              </SubjectConfirmation>
            </Subject>
            <Conditions NotBefore='2022-07-04T02:46:50.052Z' NotOnOrAfter='2022-07-04T03:46:50.000Z'>
              <AudienceRestriction>
                <Audience>https://rp.example.org/</Audience>
              </AudienceRestriction>
            </Conditions>
            <AuthnStatement AuthnInstant='2022-07-04T02:46:50.037Z' SessionIndex='_04dad8c2-a00a-497b-9596-49cbbaf35821'>
              <AuthnContext>
                <AuthnContextClassRef>urn:federation:authentication:windows</AuthnContextClassRef>
              </AuthnContext>
            </AuthnStatement>
          </Assertion>
        </samlp:Response>";

        private const string UnsignedResponseContainingEncryptedAssertion = @"<samlp:Response ID='_1'
            Version='2.0'
            IssueInstant='2022-07-05T23:18:26.018Z'
            Destination='https://sts.googleapis.com/v1/token'
            Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified' 
            InResponseTo='_09542ad9-4deb-42d1-b565-57fd28ce4d0a'
            xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
          <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://example.org/adfs/services/trust</Issuer>
          <samlp:Status>
            <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Success' />
          </samlp:Status>
          <EncryptedAssertion xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>
            <xenc:EncryptedData 
                    Type='http://www.w3.org/2001/04/xmlenc#Element' 
                    xmlns:xenc='http://www.w3.org/2001/04/xmlenc#'>
              <xenc:EncryptionMethod Algorithm='http://www.w3.org/2001/04/xmlenc#aes256-cbc' />
              <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
                <e:EncryptedKey xmlns:e='http://www.w3.org/2001/04/xmlenc#'>
                  <e:EncryptionMethod Algorithm='http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p'>
                    <DigestMethod Algorithm='http://www.w3.org/2000/09/xmldsig#sha1' />
                  </e:EncryptionMethod>
                  <KeyInfo>
                    <ds:X509Data xmlns:ds='http://www.w3.org/2000/09/xmldsig#'>
                      <ds:X509IssuerSerial>
                        <ds:X509IssuerName>CN=test</ds:X509IssuerName>
                        <ds:X509SerialNumber>0</ds:X509SerialNumber>
                      </ds:X509IssuerSerial>
                    </ds:X509Data>
                  </KeyInfo>
                  <e:CipherData>
                    <e:CipherValue>AAAA</e:CipherValue>
                  </e:CipherData>
                </e:EncryptedKey>
              </KeyInfo>
              <xenc:CipherData>
                <xenc:CipherValue>AAAA</xenc:CipherValue>
              </xenc:CipherData>
            </xenc:EncryptedData>
          </EncryptedAssertion>
        </samlp:Response>
        ";

        private const string SignedResponseContainingEncryptedAssertion = @"<samlp:Response ID='_91bd0'
            Version='2.0' IssueInstant='2022-07-06T00:31:01.824Z'
            Destination='https://sts.googleapis.com/v1/token' 
            Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified'
            InResponseTo='_3b5fbd9b-47ce-47fe-a4af-62066f9d0428'
            xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
          <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://example.org/adfs/services/trust</Issuer>
          <ds:Signature xmlns:ds='http://www.w3.org/2000/09/xmldsig#'>
            <ds:SignedInfo>
              <ds:CanonicalizationMethod Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
              <ds:SignatureMethod Algorithm='http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' />
              <ds:Reference URI='#_91bd0331-f28f-45f2-9d33-7e3cc0bab3f9'>
                <ds:Transforms>
                  <ds:Transform Algorithm='http://www.w3.org/2000/09/xmldsig#enveloped-signature' />
                  <ds:Transform Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
                </ds:Transforms>
                <ds:DigestMethod Algorithm='http://www.w3.org/2001/04/xmlenc#sha256' />
                <ds:DigestValue>gCuKw9LVX0A0EEvXnEdyrPKcWRQa234y29ws5TnBCi4=</ds:DigestValue>
              </ds:Reference>
            </ds:SignedInfo>
            <ds:SignatureValue>AAAA</ds:SignatureValue>
            <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
              <ds:X509Data>
                <ds:X509Certificate>AAAA</ds:X509Certificate>
              </ds:X509Data>
            </KeyInfo>
          </ds:Signature>
          <samlp:Status>
            <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Success' />
          </samlp:Status>
          <EncryptedAssertion xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>
            <xenc:EncryptedData Type='http://www.w3.org/2001/04/xmlenc#Element' 
                xmlns:xenc='http://www.w3.org/2001/04/xmlenc#'>
              <xenc:EncryptionMethod Algorithm='http://www.w3.org/2001/04/xmlenc#aes256-cbc' />
              <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
                <e:EncryptedKey xmlns:e='http://www.w3.org/2001/04/xmlenc#'>
                  <e:EncryptionMethod Algorithm='http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p'>
                    <DigestMethod Algorithm='http://www.w3.org/2000/09/xmldsig#sha1' />
                  </e:EncryptionMethod>
                  <KeyInfo>
                    <ds:X509Data xmlns:ds='http://www.w3.org/2000/09/xmldsig#'>
                      <ds:X509IssuerSerial>
                        <ds:X509IssuerName>CN=test</ds:X509IssuerName>
                        <ds:X509SerialNumber>0</ds:X509SerialNumber>
                      </ds:X509IssuerSerial>
                    </ds:X509Data>
                  </KeyInfo>
                  <e:CipherData>
                    <e:CipherValue>AAAA</e:CipherValue>
                  </e:CipherData>
                </e:EncryptedKey>
              </KeyInfo>
              <xenc:CipherData>
                <xenc:CipherValue>AAAA</xenc:CipherValue>
              </xenc:CipherData>
            </xenc:EncryptedData>
          </EncryptedAssertion>
        </samlp:Response>";

        private const string SignedResponseContainingSignedAssertion = @"<samlp:Response 
                ID='_d1' 
                Version='2.0' 
                IssueInstant='2022-07-14T01:13:31.341Z' 
                Destination='https://sts.googleapis.com/v1/token' 
                Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified'
                InResponseTo='_59f913b3-3b27-4579-ba36-b70d1642950a'
                xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
            <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://adfs-8.g.ntdev.net/adfs/services/trust</Issuer>
            <ds:Signature xmlns:ds='http://www.w3.org/2000/09/xmldsig#'>
              <ds:SignedInfo>
                <ds:CanonicalizationMethod Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
                <ds:SignatureMethod Algorithm='http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' />
                <ds:Reference URI='#_d416e104-c192-4c7a-abe8-4db688b622c6'>
                <ds:Transforms>
                    <ds:Transform Algorithm='http://www.w3.org/2000/09/xmldsig#enveloped-signature' />
                    <ds:Transform Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
                </ds:Transforms>
                <ds:DigestMethod Algorithm='http://www.w3.org/2001/04/xmlenc#sha256' />
                <ds:DigestValue>AAAA</ds:DigestValue>
                </ds:Reference>
              </ds:SignedInfo>
              <ds:SignatureValue>AAAA</ds:SignatureValue>
               <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
                <ds:X509Data>
                 <ds:X509Certificate>AAAA</ds:X509Certificate>
                </ds:X509Data>
               </KeyInfo>
              </ds:Signature>
            <samlp:Status>
              <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Success' />
            </samlp:Status>
            <Assertion
                ID='_78315' 
                IssueInstant='2022-07-14T01:13:31.341Z'
                Version='2.0' 
                xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>
              <Issuer>http://adfs-8.g.ntdev.net/adfs/services/trust</Issuer>
              <ds:Signature xmlns:ds='http://www.w3.org/2000/09/xmldsig#'>
                <ds:SignedInfo>
                <ds:CanonicalizationMethod Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
                <ds:SignatureMethod Algorithm='http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' />
                <ds:Reference URI='#_78317e31-5f7f-4906-ad49-35bad2c0a625'>
                    <ds:Transforms>
                    <ds:Transform Algorithm='http://www.w3.org/2000/09/xmldsig#enveloped-signature' />
                    <ds:Transform Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
                    </ds:Transforms>
                    <ds:DigestMethod Algorithm='http://www.w3.org/2001/04/xmlenc#sha256' />
                    <ds:DigestValue>AAAA</ds:DigestValue>
                </ds:Reference>
                </ds:SignedInfo>
                <ds:SignatureValue>AAAA</ds:SignatureValue>
                <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
                <ds:X509Data>
                    <ds:X509Certificate>AAAA</ds:X509Certificate>
                </ds:X509Data>
                </KeyInfo>
            </ds:Signature>
            <Subject>
                <NameID Format='urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified'>setupadmin@example.org</NameID>
                <SubjectConfirmation Method='urn:oasis:names:tc:SAML:2.0:cm:bearer'>
                <SubjectConfirmationData
                        InResponseTo='_59f913b3-3b27-4579-ba36-b70d1642950a'
                        NotOnOrAfter='2022-07-14T01:18:31.341Z' 
                        Recipient='https://sts.googleapis.com/v1/token' />
                </SubjectConfirmation>
            </Subject>
            <Conditions NotBefore='2022-07-14T01:13:31.341Z' NotOnOrAfter='2022-07-14T02:13:31.341Z'>
                <AudienceRestriction>
                <Audience>https://rp.example.org/</Audience>
                </AudienceRestriction>
            </Conditions>
            <AuthnStatement AuthnInstant='2022-07-14T01:13:31.341Z' SessionIndex='_78317e31-5f7f-4906-ad49-35bad2c0a625'>
                <AuthnContext>
                <AuthnContextClassRef>urn:federation:authentication:windows</AuthnContextClassRef>
                </AuthnContext>
            </AuthnStatement>
            </Assertion>
        </samlp:Response>";

        private const string UnsignedResponseContainingRequesterError = @"<samlp:Response 
            ID='_3b' Version='2.0' 
            IssueInstant='2022-07-04T02:46:50.052Z' 
            Destination='https://sts.googleapis.com/v1/token'
            Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified' 
            InResponseTo='_54430425-0c95-45dc-9c87-2452431f014a' 
            xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
          <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://example.org/adfs/services/trust</Issuer>
          <samlp:Status>
            <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Requester' />
          </samlp:Status>
        </samlp:Response>";

        private const string UnsignedResponseContainingResponderError = @"<samlp:Response 
            ID='_8e65'
            Version='2.0'
            IssueInstant='2022-07-14T01:46:11.656Z'
            Destination='https://sts.googleapis.com/v1/token'
            Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified' 
            InResponseTo='_51936' 
            xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
          <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://adfs-8.g.ntdev.net/adfs/services/trust</Issuer>
          
          <samlp:Status>
          <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Responder' />
          </samlp:Status>
        </samlp:Response>";

        private const string UnsignedResponseContainingResponderErrorAndSubStatus = @"<samlp:Response 
            ID='_8e65'
            Version='2.0'
            IssueInstant='2022-07-14T01:46:11.656Z'
            Destination='https://sts.googleapis.com/v1/token'
            Consent='urn:oasis:names:tc:SAML:2.0:consent:unspecified' 
            InResponseTo='_51936' 
            xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
          <Issuer xmlns='urn:oasis:names:tc:SAML:2.0:assertion'>http://adfs-8.g.ntdev.net/adfs/services/trust</Issuer>
          
          <samlp:Status>
            <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Responder'>
              <samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:RequestDenied' />
            </samlp:StatusCode>
          </samlp:Status>
        </samlp:Response>";

        private static string EncodeXml(string xml)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
        }

        [Test]
        public void WhenResponseIsUnsignedAndContainsRequesterError_ThenParseThrowsException()
        {
            Assert.That(
                () => AuthenticationResponse.Parse(EncodeXml(UnsignedResponseContainingRequesterError)),
                Throws.InstanceOf<InvalidSamlResponseException>());
        }

        [Test]
        public void WhenResponseIsUnsignedAndContainsResponderError_ThenParseThrowsException()
        {
            var encodedResponse = EncodeXml(UnsignedResponseContainingResponderError);

            Assert.That(
                () => AuthenticationResponse.Parse(encodedResponse),
                Throws.InstanceOf<InvalidSamlResponseException>());
        }

        [Test]
        public void WhenResponseIsUnsignedAndContainsResponderErrorAndSubStatus_ThenParseThrowsException()
        {
            var encodedResponse = EncodeXml(UnsignedResponseContainingResponderErrorAndSubStatus);

            Assert.That(
                () => AuthenticationResponse.Parse(encodedResponse),
                Throws.InstanceOf<InvalidSamlResponseException>());
        }

        [Test]
        public void WhenResponseIsUnsignedAndContainsUnsignedAssertion_ThenParsSucceeds()
        {
            var encodedResponse = EncodeXml(UnsignedResponseContainingUnsignedAssertion);
            var response = AuthenticationResponse.Parse(encodedResponse);
            Assert.That(response.IsEncrypted, Is.False);
            Assert.That(response.Type, Is.EqualTo(SubjectTokenType.Saml2));
            Assert.That(response.Value, Is.EqualTo(encodedResponse));
            Assert.That(response.Issuer, Is.EqualTo("http://example.org/adfs/services/trust"));
            Assert.That(response.Audience, Is.EqualTo("https://rp.example.org/"));
            Assert.That(response.Expiry, Is.Not.Null);
        }

        [Test]
        public void WhenResponseIsUnsignedAndContainsEncryptedAssertion_ThenParseSucceeds()
        {
            var encodedResponse = EncodeXml(UnsignedResponseContainingEncryptedAssertion);
            var response = AuthenticationResponse.Parse(encodedResponse);
            Assert.That(response.IsEncrypted, Is.True);
            Assert.That(response.Type, Is.EqualTo(SubjectTokenType.Saml2));
            Assert.That(response.Value, Is.EqualTo(encodedResponse));
            Assert.That(response.Issuer, Is.EqualTo("http://example.org/adfs/services/trust"));
            Assert.That(response.Audience, Is.Null);
            Assert.That(response.Expiry, Is.Null);
        }

        [Test]
        public void WhenResponseIsSignedAndContainsEncryptedAssertion_ThenParseSucceeds()
        {
            var encodedResponse = EncodeXml(SignedResponseContainingEncryptedAssertion);
            var response = AuthenticationResponse.Parse(encodedResponse);
            Assert.That(response.IsEncrypted, Is.True);
            Assert.That(response.Type, Is.EqualTo(SubjectTokenType.Saml2));
            Assert.That(response.Value, Is.EqualTo(encodedResponse));
            Assert.That(response.Issuer, Is.EqualTo("http://example.org/adfs/services/trust"));
            Assert.That(response.Audience, Is.Null);
            Assert.That(response.Expiry, Is.Null);
        }

        [Test]
        public void WhenResponseIsSignedAndContainsSignedAssertionButCertMissing_ThenParseThrowsException()
        {
            var encodedResponse = EncodeXml(SignedResponseContainingSignedAssertion);

            Assert.That(
                () => AuthenticationResponse.Parse(encodedResponse),
                Throws.InstanceOf<CryptographicException>());
        }
    }
}
