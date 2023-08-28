//
// Copyright 2023 Google LLC
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

package com.google.solutions.tokenservice.oauth.mtls;

import java.time.OffsetDateTime;

/**
 * Client attributes as conveyed in a client certificate.
 *
 * @param clientId Client ID, corresponds to the spiffeID, sanDns or sanUri.
 *
 * @param spiffeId SPIFFE ID of the certificate.
 * @param sanDns dNSName SAN entry in the certificate.
 * @param sanUri uniformResourceIdentifier SAN entry in the certificate.
 * @param sha256fingerprint SHA-256 sha256fingerprint of the client certificate.
 * @param serialNumber  serial number of the client certificate.
 * @param notBefore timestamp before which the client certificate is not valid.
 * @param notAfter timestamp after which the client certificate is not valid.
 */
public record MtlsClientAttributes(
  String clientId,
  String spiffeId,
  String sanDns,
  String sanUri,
  String sha256fingerprint,
  String serialNumber,
  OffsetDateTime notBefore,
  OffsetDateTime notAfter
){
}
