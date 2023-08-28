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

using Google.Solutions.WWAuth.Util;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;

namespace Google.Solutions.WWAuth
{
    /// <summary>
    /// Command line options for unattended use.
    /// </summary>
    internal class UnattendedCommandLineOptions : ICommandLineOptions
    {
        public enum AuthenticationProtocol
        {
            /// <summary>
            /// OpenID Connect with client authentication grant.
            /// </summary>
            AdfsOidc,

            /// <summary>
            /// WS-Trust with SAML 2.0 assertion.
            /// </summary>
            AdfsWsTrust,

            /// <summary>
            /// SAML 2.0 with POST-binding.
            /// </summary>
            AdfsSamlPost
        }

        public string Executable { get; set; }

        //---------------------------------------------------------------------
        // General options.
        //---------------------------------------------------------------------

        [Required]
        [CommandLineArgument]
        public string IssuerUrl { get; set; }

        [Required]
        [CommandLineArgument]
        public AuthenticationProtocol Protocol { get; set; }

        //---------------------------------------------------------------------
        // Protocol options.
        //---------------------------------------------------------------------

        [Required]
        [CommandLineArgument]
        public string RelyingPartyId { get; set; }

        [CommandLineArgument]
        public string OidcClientId { get; set; }

        [CommandLineArgument]
        public string SamlAcsUrl { get; set; }

        [CommandLineArgument]
        public string SamlRequestSigningCertificate { get; set; }

        //---------------------------------------------------------------------
        // Parsing.
        //---------------------------------------------------------------------

        public static UnattendedCommandLineOptions Parse(string commandLine)
            => CommandLineParser.Parse<UnattendedCommandLineOptions>(commandLine);

        public override string ToString()
        {
            //
            // NB. Some client libraries don't support quotes in
            // commands (b/237606033), so always create an unquoted
            // command line.
            //
            Debug.Assert(this.IssuerUrl == null || !this.IssuerUrl.Contains(" "));
            Debug.Assert(this.RelyingPartyId == null || !this.RelyingPartyId.Contains(" "));
            Debug.Assert(this.OidcClientId == null || !this.OidcClientId.Contains(" "));
            Debug.Assert(this.SamlAcsUrl == null || !this.SamlAcsUrl.Contains(" "));

            return CommandLineParser.ToString(this, false);
        }

        public UnattendedCommandLineOptions Validate()
        {
            Validator.ValidateObject(
                this,
                new ValidationContext(this));
            return this;
        }
    }

    public class AttendedCommandLineOptions : ICommandLineOptions
    {
        public string Executable { get; set; }

        [CommandLineArgument]
        public string Edit { get; set; }

        [CommandLineArgument]
        public string Verify { get; set; }

        public AttendedCommandLineOptions Validate()
        {
            if (this.Edit != null && !File.Exists(this.Edit))
            {
                throw new ValidationException(
                    $"File '{this.Edit}' does not exist");
            }

            return this;
        }

        public static AttendedCommandLineOptions Parse(string commandLine)
            => CommandLineParser.Parse<AttendedCommandLineOptions>(commandLine);

        public override string ToString()
            => CommandLineParser.ToString(this);
    }
}
