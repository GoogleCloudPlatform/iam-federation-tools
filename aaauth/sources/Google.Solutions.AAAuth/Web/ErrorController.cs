//
// Copyright 2026 Google LLC
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Google.Solutions.AAAuth.Web
{
    /// <summary>
    /// Controller for handling failed requests.
    /// </summary>
    [ApiController]
    [Route(Path)]
    public class ErrorController : Controller
    {
        public const string Path = "/error";

        [HttpGet]
        [AllowAnonymous]
        public IActionResult HandleError()
        {
            return Problem();
        }

        [HttpGet]
        [Route("{statusCode}")]
        [AllowAnonymous]
        public IActionResult HandleStatusErrors(int statusCode)
        {
            if (statusCode == 404)
            {
                return Problem(
                    detail: "The requested resource was not found.",
                    statusCode: 404);
            }

            return Problem(statusCode: statusCode);
        }
    }
}
