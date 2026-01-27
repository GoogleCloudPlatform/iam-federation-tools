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
using System.IdentityModel.Tokens;
using System.Linq;

namespace Google.Solutions.WWAuth.Test.Data.Saml2
{
    [TestFixture]
    public class TestSaml2Token
    {
        private static Assertion CreateToken(Saml2Assertion assertion)
        {
            return new Assertion(
                new Saml2SecurityToken(assertion),
                "encoded");
        }

        //---------------------------------------------------------------------
        // Issuer.
        //---------------------------------------------------------------------

        [Test]
        public void WhenAssertionHasIssuer_ThenIssuerIsNotNull()
        {
            var token = CreateToken(
                new Saml2Assertion(new Saml2NameIdentifier("issuer")));

            Assert.That(token.Issuer, Is.EqualTo("issuer"));
        }

        //---------------------------------------------------------------------
        // Audience.
        //---------------------------------------------------------------------

        [Test]
        public void WhenAssertionLacksAudience_ThenAudienceIsNull()
        {
            var token = CreateToken(
                new Saml2Assertion(new Saml2NameIdentifier("isuer")));

            Assert.That(token.Audience, Is.Null);
        }

        [Test]
        public void WhenAssertionHasAudiences_ThenAudienceReturnsFirstAudience()
        {
            var assertion = new Saml2Assertion(new Saml2NameIdentifier("isuer"))
            {
                Conditions = new Saml2Conditions()
            };

            assertion.Conditions.AudienceRestrictions.Add(
                new Saml2AudienceRestriction(new Uri("https://example.com/")));
            assertion.Conditions.AudienceRestrictions.Add(
                new Saml2AudienceRestriction(new Uri("https://example.org/")));
            var token = CreateToken(assertion);

            Assert.That(token.Audience, Is.EqualTo("https://example.com/"));
        }

        //---------------------------------------------------------------------
        // Attributes.
        //---------------------------------------------------------------------

        [Test]
        public void WhenAssertionHasNoNameIdAndNoAttributes_ThenAttributesIsEmpty()
        {
            var token = CreateToken(
                new Saml2Assertion(new Saml2NameIdentifier("isuer")));

            Assert.That(token.Attributes.Any(), Is.False);
        }

        [Test]
        public void WhenAssertionHasNameId_ThenAttributesReturnsFlattedList()
        {
            var token = CreateToken(new Saml2Assertion(new Saml2NameIdentifier("isuer"))
            {
                Subject = new Saml2Subject(new Saml2NameIdentifier("subject"))
            });

            var attributes = token.Attributes;
            Assert.That(attributes.Count, Is.EqualTo(1));
            Assert.That(attributes["assertion.subject"], Is.EqualTo("subject"));
        }

        [Test]
        public void WhenAssertionHasAttributes_ThenAttributesReturnsFlattedList()
        {
            var attributeStatement1 = new Saml2AttributeStatement();
            attributeStatement1.Attributes.Add(
                new Saml2Attribute("att-1", "value-1"));

            var attributeStatement2 = new Saml2AttributeStatement();
            attributeStatement2.Attributes.Add(
                new Saml2Attribute("att-2", new[] { "value-1", "value-2" }));

            var assertion = new Saml2Assertion(new Saml2NameIdentifier("isuer"));
            assertion.Statements.Add(attributeStatement1);
            assertion.Statements.Add(attributeStatement2);

            var token = CreateToken(assertion);

            var attributes = token.Attributes;
            Assert.That(attributes.Count, Is.EqualTo(3));
            Assert.That(attributes["assertion.attributes['att-1']"], Is.EqualTo("value-1"));
            Assert.That(
                attributes["assertion.attributes['att-2'][0]"],
                Is.EqualTo("value-1"));
            Assert.That(
                attributes["assertion.attributes['att-2'][1]"],
                Is.EqualTo("value-2"));
        }
    }
}
