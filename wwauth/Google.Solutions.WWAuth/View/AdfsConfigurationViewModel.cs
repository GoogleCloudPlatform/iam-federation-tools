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

using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    internal class AdfsConfigurationViewModel
        : ConfigurationViewModelBase, IPropertiesSheetViewModel
    {
        private readonly ICertificateStoreAdapter certificateStoreAdapter;
        private readonly UnattendedCommandLineOptions commandLineOptions;

        private bool isClientIdTextBoxVisible;
        private bool isRelyingPartyIdTextBoxReadonly;
        private bool isAcsUrlTextBoxVisible;

        private bool isSignRequestControlVisible;
        private bool isSignRequestControlEnabled;
        private X509Certificate2 requestSigningCertificate;

        public AdfsConfigurationViewModel(
            CredentialConfigurationFile file,
            IShellAdapter shellAdapter,
            ICertificateStoreAdapter certificateStoreAdapter)
            : base(file, shellAdapter)
        {
            this.commandLineOptions = file.Configuration.Options;
            this.certificateStoreAdapter = certificateStoreAdapter;

            if (!string.IsNullOrEmpty(this.commandLineOptions.SamlRequestSigningCertificate))
            {
                this.RequestSigningCertificate =
                    certificateStoreAdapter.TryGetSigningCertificate(
                        this.commandLineOptions.SamlRequestSigningCertificate);
            }

            ReapplyProtocolDefaults();
        }

        public string Title => "AD FS";

        /// <summary>
        /// By default, STS expects the provider URL as audience, which
        /// defines the default relying party ID.
        /// </summary>
        internal string DefaultRelyingPartyId
            => $"https:{this.File.Configuration.PoolConfiguration.Audience}";


        //---------------------------------------------------------------------
        // Observable properties.
        //---------------------------------------------------------------------

        public string IssuerUrl
        {
            get => this.commandLineOptions.IssuerUrl;
            set
            {
                this.commandLineOptions.IssuerUrl = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
            }
        }

        public string[] AvailableProtocols
        {
            get => Enum.GetValues(typeof(UnattendedCommandLineOptions.AuthenticationProtocol))
                .Cast<UnattendedCommandLineOptions.AuthenticationProtocol>()
                .Select(v => v.ToString())
                .ToArray();
        }

        public int ProtocolIndex
        {
            get => Array.IndexOf(this.AvailableProtocols, this.commandLineOptions.Protocol.ToString());
            set
            {
                this.commandLineOptions.Protocol =
                    (UnattendedCommandLineOptions.AuthenticationProtocol)Enum.Parse(
                        typeof(UnattendedCommandLineOptions.AuthenticationProtocol),
                        this.AvailableProtocols[value]);

                ReapplyProtocolDefaults();

                this.IsDirty = true;
                RaisePropertyChange();
            }
        }

        public string RelyingPartyId
        {
            get => this.commandLineOptions.RelyingPartyId;
            set
            {
                this.commandLineOptions.RelyingPartyId = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
            }
        }

        public string ClientId
        {
            get => this.commandLineOptions.OidcClientId;
            set
            {
                this.commandLineOptions.OidcClientId = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
            }
        }

        public string AcsUrl
        {
            get => this.commandLineOptions.SamlAcsUrl;
            set
            {
                this.commandLineOptions.SamlAcsUrl = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
            }
        }

        public bool IsClientIdTextBoxVisible
        {
            get => this.isClientIdTextBoxVisible;
            set
            {
                this.isClientIdTextBoxVisible = value;
                RaisePropertyChange();
            }
        }

        public bool IsRelyingPartyIdTextBoxReadonly
        {
            get => this.isRelyingPartyIdTextBoxReadonly;
            set
            {
                this.isRelyingPartyIdTextBoxReadonly = value;
                RaisePropertyChange();
            }
        }

        public bool IsAcsUrlTextBoxVisible
        {
            get => this.isAcsUrlTextBoxVisible;
            set
            {
                this.isAcsUrlTextBoxVisible = value;
                RaisePropertyChange();
            }
        }

        public bool IsSignRequestControlVisible
        {
            get => this.isSignRequestControlVisible;
            set
            {
                this.isSignRequestControlVisible = value;
                RaisePropertyChange();
            }
        }

        public bool IsSignRequestControlEnabled
        {
            get => this.isSignRequestControlEnabled;
            set
            {
                this.isSignRequestControlEnabled = value;

                if (!value)
                {
                    this.RequestSigningCertificate = null;
                }

                RaisePropertyChange();
            }
        }

        public X509Certificate2 RequestSigningCertificate
        {
            get => this.requestSigningCertificate;
            set
            {
                this.commandLineOptions.SamlRequestSigningCertificate = value?.Thumbprint;
                this.requestSigningCertificate = value;

                if (value != null)
                {
                    this.IsSignRequestControlEnabled = true;
                }

                this.IsDirty = true;

                RaisePropertyChange();
                RaisePropertyChange((AdfsConfigurationViewModel m) => m.SigningCertificateSubject);
                RaisePropertyChange((AdfsConfigurationViewModel m) => m.IsViewCertificateMenuItemEnabled);
            }
        }

        public string SigningCertificateSubject
            => this.RequestSigningCertificate?.Subject;

        public bool IsViewCertificateMenuItemEnabled
            => this.RequestSigningCertificate != null;

        public string Executable => this.File.Configuration.Options.Executable;

        //---------------------------------------------------------------------
        // Actions.
        //---------------------------------------------------------------------

        public void ResetExecutable()
        {
            this.File.Configuration.ResetExecutable();
            this.IsDirty = true;
        }

        public void ReapplyProtocolDefaults()
        {
            //
            // N.B. Because some defaults depend on settings controlled by
            // other property sheet pages, we need to reapply the default
            // when switching to this sheet and before saving.
            //

            switch (this.commandLineOptions.Protocol)
            {
                case UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc:
                    this.IsRelyingPartyIdTextBoxReadonly = false;
                    if (string.IsNullOrEmpty(this.RelyingPartyId) &&
                        this.File.Configuration.PoolConfiguration.IsValid)
                    {
                        this.RelyingPartyId = $"https:{this.File.Configuration.PoolConfiguration.Audience}";
                    }

                    this.IsClientIdTextBoxVisible = true;

                    this.IsAcsUrlTextBoxVisible = false;
                    if (!string.IsNullOrEmpty(this.AcsUrl))
                    {
                        this.AcsUrl = string.Empty;
                    }

                    this.IsSignRequestControlVisible = false;

                    break;

                case UnattendedCommandLineOptions.AuthenticationProtocol.AdfsWsTrust:
                    this.IsRelyingPartyIdTextBoxReadonly = true;
                    this.RelyingPartyId = this.DefaultRelyingPartyId;

                    this.IsClientIdTextBoxVisible = false;
                    if (!string.IsNullOrEmpty(this.ClientId))
                    {
                        this.ClientId = string.Empty;
                    }

                    this.IsAcsUrlTextBoxVisible = false;
                    if (!string.IsNullOrEmpty(this.AcsUrl))
                    {
                        this.AcsUrl = string.Empty;
                    }

                    this.IsSignRequestControlVisible = false;

                    break;

                case UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost:
                    this.IsRelyingPartyIdTextBoxReadonly = true;
                    this.RelyingPartyId = this.DefaultRelyingPartyId;

                    this.IsClientIdTextBoxVisible = false;
                    if (!string.IsNullOrEmpty(this.ClientId))
                    {
                        this.ClientId = string.Empty;
                    }

                    this.IsAcsUrlTextBoxVisible = true;
                    if (string.IsNullOrEmpty(this.AcsUrl))
                    {
                        this.AcsUrl = StsAdapter.DefaultTokenUrl;
                    }

                    this.IsSignRequestControlVisible = true;
                    this.IsSignRequestControlEnabled = this.RequestSigningCertificate != null;

                    break;
            }
        }

        public void BrowseForRequestSigningCertificate(
            IWin32Window parent)
        {
            var candidates = new X509Certificate2Collection();
            candidates.AddRange(this.certificateStoreAdapter
                .ListSigningCertitficates()
                .ToArray());

            var certificate = X509Certificate2UI.SelectFromCollection(
                    candidates,
                    "Signing certificate",
                    "Select a cerfificate",
                    X509SelectionFlag.SingleSelection,
                    parent.Handle)
                .Cast<X509Certificate2>()
                .FirstOrDefault();

            if (certificate != null)
            {
                this.RequestSigningCertificate = certificate;
            }
        }

        public void ViewRequestSigningCertificate(
            IWin32Window parent)
        {
            X509Certificate2UI.DisplayCertificate(
                this.RequestSigningCertificate,
                parent.Handle);
        }

        public override DialogResult ApplyChanges(IWin32Window owner)
        {
            ReapplyProtocolDefaults();
            return base.ApplyChanges(owner);
        }
    }
}
