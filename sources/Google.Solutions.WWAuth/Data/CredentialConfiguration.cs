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
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Util;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Google.Solutions.WWAuth.Data
{
    /// <summary>
    /// Application-specific representation of a:
    /// 
    /// - workload identity federation credential configuration
    /// - workforce identity federation credential configuration
    ///   (for completeness' sake, we can't do anything with that yet)
    /// 
    /// </summary>
    internal class CredentialConfiguration
    {
        internal static readonly string[] DefaultScopes =
            new[] { "https://www.googleapis.com/auth/cloud-platform" };

        /// <summary>
        /// Workload or workforce identity pool configuration.
        /// </summary>
        public IdentityPoolConfiguration PoolConfiguration { get; }

        /// <summary>
        /// Service account to impersonate, optional.
        /// Only applies to workload identity federation.
        /// </summary>
        public string ServiceAccountEmail { get; set; }

        /// <summary>
        /// Command line options for for executable command.
        /// </summary>
        public UnattendedCommandLineOptions Options { get; }

        /// <summary>
        /// Timeout for executable command.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        internal CredentialConfiguration(
            IdentityPoolConfiguration poolConfiguration,
            UnattendedCommandLineOptions options)
        {
            this.PoolConfiguration = poolConfiguration.ThrowIfNull(nameof(poolConfiguration));
            this.Options = options.ThrowIfNull(nameof(options));
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Options.IssuerUrl))
            {
                throw new InvalidCredentialConfigurationException("AD FS Issuer URL must be specified.");
            }

            if (string.IsNullOrEmpty(this.Options.RelyingPartyId))
            {
                throw new InvalidCredentialConfigurationException("Relying party ID must be specified.");
            }


            if (!string.IsNullOrEmpty(this.ServiceAccountEmail) &&
                !this.ServiceAccountEmail.EndsWith(".iam.gserviceaccount.com"))
            {
                throw new InvalidCredentialConfigurationException(
                    $"{this.ServiceAccountEmail} is not a valid service account email address");
            }

            this.PoolConfiguration.Validate();
        }

        //---------------------------------------------------------------------
        // Factory methods.
        //---------------------------------------------------------------------

        public static CredentialConfiguration NewWorkloadIdentityConfiguration()
        {
            return new CredentialConfiguration(
                new WorkloadIdentityPoolConfiguration(),
                new UnattendedCommandLineOptions()
                {
                    Executable = Assembly.GetExecutingAssembly().Location
                });
        }

        public static CredentialConfiguration NewWorkforceIdentityConfiguration()
        {
            return new CredentialConfiguration(
                new WorkforceIdentityPoolConfiguration(),
                new UnattendedCommandLineOptions()
                {
                    Executable = Assembly.GetExecutingAssembly().Location
                });
        }

        //---------------------------------------------------------------------
        // JSON serialization to/from the credential configuration file format.
        //---------------------------------------------------------------------

        internal CredentialConfigurationInfo ToJsonStructure()
        {
            Validate();

            var tokenType = this.Options.Protocol
                == UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc
                    ? Data.SubjectTokenType.Jwt
                    : Data.SubjectTokenType.Saml2;

            var userProjectNumber =
                (this.PoolConfiguration as WorkforceIdentityPoolConfiguration)?.UserProjectNumber;

            var serviceAccountImpersonationUrl =
                this.PoolConfiguration is WorkloadIdentityPoolConfiguration &&
                !string.IsNullOrEmpty(this.ServiceAccountEmail)
                    ? "https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/" +
                      $"{this.ServiceAccountEmail}:generateAccessToken"
                    : null;

            return new CredentialConfigurationInfo(
                "external_account",
                StsAdapter.DefaultTokenUrl,
                this.PoolConfiguration.Audience,
                userProjectNumber,
                serviceAccountImpersonationUrl,
                tokenType.GetDescription(),
                new CredentialSourceInfo(new ExecutableInfo(
                    this.Options.ToString(),
                    (ulong)this.Timeout.TotalMilliseconds)));
        }

        internal static CredentialConfiguration FromJsonStructure(CredentialConfigurationInfo info)
        {
            if (info?.Type != CredentialConfigurationInfo.ExternalAccount)
            {
                throw new UnknownCredentialConfigurationException(
                    "Unsupported configuration type: " + info?.Type);
            }

            if (info.CredentialSource?.Executable?.Command == null)
            {
                throw new InvalidCredentialConfigurationException(
                    "Missing credential source or command");
            }

            IdentityPoolConfiguration poolConfig;
            if (string.IsNullOrEmpty(info.Audience))
            {
                throw new InvalidCredentialConfigurationException("Audience missing");
            }
            else if (WorkloadIdentityPoolConfiguration.TryParse(
                info.Audience,
                out var workloadPoolConfig))
            {
                poolConfig = workloadPoolConfig;
            }
            else if (WorkforceIdentityPoolConfiguration.TryParse(
                info.Audience,
                out var workforcePoolConfig))
            {
                if (info.WorkforcePoolUserProject == null || info.WorkforcePoolUserProject == 0)
                {
                    throw new InvalidCredentialConfigurationException(
                    "Missing user project number for workforce identity.");
                }

                workforcePoolConfig.UserProjectNumber = info.WorkforcePoolUserProject;
                poolConfig = workforcePoolConfig;
            }
            else
            {
                throw new InvalidCredentialConfigurationException(
                    "Malformed audience: " + info.Audience);
            }

            var configuration = new CredentialConfiguration(
                poolConfig,
                UnattendedCommandLineOptions.Parse(info.CredentialSource.Executable.Command));

            if (info.CredentialSource.Executable.TimeoutMillis != null)
            {
                configuration.Timeout = TimeSpan.FromMilliseconds(
                    info.CredentialSource.Executable.TimeoutMillis.Value);
            }

            if (!string.IsNullOrEmpty(info.ServiceAccountImpersonationUrl))
            {
                var serviceAccountImpersonationUrlMatch = new Regex(
                    "^https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/(.*):generateAccessToken$")
                    .Match(info.ServiceAccountImpersonationUrl);
                if (!serviceAccountImpersonationUrlMatch.Success)
                {
                    throw new ArgumentException("Malformed service account impersonation URL: " +
                        info.ServiceAccountImpersonationUrl);
                }

                configuration.ServiceAccountEmail = serviceAccountImpersonationUrlMatch.Groups[1].Value;
            }

            return configuration;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(
                ToJsonStructure(),
                Formatting.Indented);
        }

        public static CredentialConfiguration FromJson(string json)
        {
            return FromJsonStructure(
                JsonConvert.DeserializeObject<CredentialConfigurationInfo>(json));
        }

        internal class CredentialConfigurationInfo
        {
            internal const string ExternalAccount = "external_account";

            [JsonProperty("type")]
            public string Type { get; }

            [JsonProperty("token_url")]
            public string TokenUrl { get; }

            [JsonProperty("audience")]
            public string Audience { get; }

            [JsonProperty("workforce_pool_user_project")]
            public ulong? WorkforcePoolUserProject { get; }

            [JsonProperty("service_account_impersonation_url")]
            public string ServiceAccountImpersonationUrl { get; }

            [JsonProperty("subject_token_type")]
            public string SubjectTokenType { get; }

            [JsonProperty("credential_source")]
            public CredentialSourceInfo CredentialSource { get; }

            [JsonConstructor]
            public CredentialConfigurationInfo(
                [JsonProperty("type")] string type,
                [JsonProperty("token_url")] string tokenUrl,
                [JsonProperty("audience")] string audience,
                [JsonProperty("workforce_pool_user_project")] ulong? WorkforcePoolUserProject,
                [JsonProperty("service_account_impersonation_url")] string saImpersonationUrl,
                [JsonProperty("subject_token_type")] string subjectTokenType,
                [JsonProperty("credential_source")] CredentialSourceInfo credentialSource)
            {
                this.Type = type;
                this.TokenUrl = tokenUrl;
                this.Audience = audience;
                this.WorkforcePoolUserProject = WorkforcePoolUserProject;
                this.ServiceAccountImpersonationUrl = saImpersonationUrl;
                this.SubjectTokenType = subjectTokenType;
                this.CredentialSource = credentialSource;
            }
        }

        internal class CredentialSourceInfo
        {
            [JsonProperty("executable")]
            public ExecutableInfo Executable { get; set; }

            [JsonConstructor]
            public CredentialSourceInfo(
                [JsonProperty("executable")] ExecutableInfo executable)
            {
                this.Executable = executable;
            }
        }

        internal class ExecutableInfo
        {
            [JsonProperty("command")]
            public string Command { get; }

            [JsonProperty("timeout_millis")]
            public ulong? TimeoutMillis { get; }

            [JsonConstructor]
            public ExecutableInfo(
                [JsonProperty("command")] string command,
                [JsonProperty("timeout_millis")] ulong? timeoutMillis)
            {
                this.Command = command;
                this.TimeoutMillis = timeoutMillis;
            }
        }
    }

    public class UnknownCredentialConfigurationException : Exception
    {
        public UnknownCredentialConfigurationException(string message)
            : base(message)
        {
        }
    }

    public class InvalidCredentialConfigurationException : Exception
    {
        public InvalidCredentialConfigurationException(string message)
            : base(message)
        {
        }
    }
}
