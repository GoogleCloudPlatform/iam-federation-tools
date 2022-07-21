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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Google.Solutions.WWAuth.Adapters
{
    /// <summary>
    /// Adapter for Windows certificate store.
    /// </summary>
    public interface ICertificateStoreAdapter
    {
        IEnumerable<X509Certificate2> ListSigningCertitficates();

        X509Certificate2 TryGetSigningCertificate(string thumbprint);
    }

    public class CertificateStoreAdapter : ICertificateStoreAdapter
    {
        public IEnumerable<X509Certificate2> ListSigningCertitficates()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates
                    .Cast<X509Certificate2>()
                    .Where(cert => cert.HasPrivateKey)
                    .Where(cert => cert.Extensions
                        .OfType<X509KeyUsageExtension>()
                        .Any(ext => ext.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature)))
                    .ToList();
            }
        }

        public X509Certificate2 TryGetSigningCertificate(
            string thumbprint)
        {
            return ListSigningCertitficates()
                .FirstOrDefault(cert => cert.Thumbprint.Equals(
                    thumbprint,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
