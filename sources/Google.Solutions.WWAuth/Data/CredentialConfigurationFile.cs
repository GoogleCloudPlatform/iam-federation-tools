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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Google.Solutions.WWAuth.Data
{
    /// <summary>
    /// A credential configuration file as 
    /// defined in https://google.aip.dev/auth/4117.
    /// </summary>
    internal class CredentialConfigurationFile
    {
        internal const string FileExtension = "wwconfig";

        /// <summary>
        /// File path to save to, optional.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Flag indicating whether there are unsaved changes.
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Editable configuration.
        /// </summary>
        public CredentialConfiguration Configuration { get; }

        internal CredentialConfigurationFile(
            string filePath,
            CredentialConfiguration configuration)
        {
            this.FilePath = filePath;
            this.Configuration = configuration.ThrowIfNull(nameof(configuration));
        }

        /// <summary>
        /// Mark file as dirty, i.e. it needs saving.
        /// </summary>
        public void SetDirty()
        {
            this.IsDirty = true;
        }

        /// <summary>
        /// Save to existing file.
        /// </summary>
        public void Save()
        {
            if (this.FilePath == null)
            {
                throw new InvalidOperationException("File path not specified");
            }

            //
            // Update executable path.
            //
            this.Configuration.Options.Executable =
                Assembly.GetExecutingAssembly().Location;

            File.WriteAllText(this.FilePath, this.Configuration.ToString());

            this.IsDirty = false;
        }

        /// <summary>
        /// Save to a new file.
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAs(string filePath)
        {
            this.FilePath = filePath;
            Save();
        }

        public static CredentialConfigurationFile FromFile(string filePath)
        {
            try
            {
                return new CredentialConfigurationFile(
                    filePath,
                    CredentialConfiguration.FromJson(File.ReadAllText(filePath)));
            }
            catch (InvalidCredentialConfigurationException e)
            {
                throw new InvalidCredentialConfigurationFileException(
                    "The credential configuration file contains invalid or incomplete " +
                    "settings", e);
            }
            catch (JsonReaderException e)
            {
                throw new InvalidCredentialConfigurationFileException(
                    $"Credential configuration file {filePath} contains malformed data", e);
            }
            catch (ArgumentException e)
            {
                throw new InvalidCredentialConfigurationFileException(
                    $"Credential configuration file {filePath} contains malformed data", e);
            }
        }

        public static CredentialConfigurationFile NewWorkloadIdentityConfigurationFile()
        {
            return new CredentialConfigurationFile(
                null,
                CredentialConfiguration.NewWorkloadIdentityConfiguration());
        }

        public static CredentialConfigurationFile NewWorkforceIdentityConfigurationFile()
        {
            return new CredentialConfigurationFile(
                null,
                CredentialConfiguration.NewWorkforceIdentityConfiguration());
        }

        public CredentialConfigurationFile Clone()
        {
            return new CredentialConfigurationFile(
                null,
                CredentialConfiguration.FromJsonStructure(
                    this.Configuration.ToJsonStructure()));
        }
    }

    public class InvalidCredentialConfigurationFileException : Exception
    {
        public InvalidCredentialConfigurationFileException(
            string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
