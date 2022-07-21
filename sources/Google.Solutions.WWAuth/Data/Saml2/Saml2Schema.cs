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
using System.Xml;
using System.Xml.Serialization;

namespace Google.Solutions.WWAuth.Data.Saml2
{
    public static class Saml2Schema
    {
        //---------------------------------------------------------------------
        // AuthnResponse.
        //---------------------------------------------------------------------

        [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", IsNullable = false)]
        public class Response
        {
            [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
            public string Issuer { get; set; }

            [XmlElement]
            public ResponseStatus Status { get; set; }

            [XmlAnyElement(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
            public XmlElement[] Assertion { get; set; }

            [XmlAttribute]
            public string InResponseTo { get; set; }

            [XmlAttribute]
            public string Destination { get; set; }

            [XmlAttribute]
            public string ID { get; set; }

            [XmlAttribute]
            public DateTime IssueInstant { get; set; }

            [XmlAttribute]
            public string Version { get; set; }
        }

        [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        public class ResponseStatus
        {
            [XmlElement]
            public ResponseStatusStatusCode StatusCode { get; set; }

            [XmlElement]
            public string StatusMessage { get; set; }
        }

        [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        public class ResponseStatusStatusCode
        {
            [XmlElement]
            public ResponseStatusStatusCodeStatusCode StatusCode { get; set; }

            [XmlAttribute]
            public string Value { get; set; }
        }

        [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        public class ResponseStatusStatusCodeStatusCode
        {
            [XmlAttribute]
            public string Value { get; set; }
        }

        //---------------------------------------------------------------------
        // AuthnRequest.
        //---------------------------------------------------------------------

        [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", IsNullable = false)]
        public partial class AuthnRequest
        {
            [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
            public string Issuer { get; set; }

            [XmlElement]
            public AuthnRequestNameIDPolicy NameIDPolicy { get; set; }

            [XmlAttribute]
            public string ID { get; set; }

            [XmlAttribute]
            public string Version { get; set; }

            [XmlAttribute]
            public System.DateTime IssueInstant { get; set; }

            [XmlAttribute]
            public string ProtocolBinding { get; set; }

            [XmlAttribute]
            public bool IsPassive { get; set; }

            [XmlAttribute]
            public string AssertionConsumerServiceURL { get; set; }

            [XmlAttribute]
            public string Destination { get; set; }
        }

        /// <remarks/>
        [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        public partial class AuthnRequestNameIDPolicy
        {
            [XmlAttribute]
            public bool AllowCreate { get; set; }

            /// <remarks/>
            [XmlAttribute]
            public string Format { get; set; }
        }
    }
}