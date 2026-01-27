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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Google.Solutions.WWAuth.Util
{
    internal static class RequestExtensions
    {
        public static TRequest WithCredentials<TRequest, TResponse>(
            this TRequest request,
            ClientSecrets credentials)
            where TRequest : ClientServiceRequest<TResponse>
        {
            if (credentials != null)
            {
                request.ModifyRequest = (HttpRequestMessage message) =>
                {
                    message.Headers.Authorization = new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}")));
                };
            }

            return request;
        }
    }
}
