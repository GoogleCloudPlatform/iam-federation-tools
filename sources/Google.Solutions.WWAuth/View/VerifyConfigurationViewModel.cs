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

using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Logging;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Properties;
using Google.Solutions.WWAuth.Util;
using System;
using System.Drawing;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.View
{
    internal class VerifyConfigurationViewModel : ViewModelBase
    {
        private string windowTitle = string.Empty;
        private Image acquireTokenStatusImage = null;
        private Image exchangeTokenStatusImage = null;
        private Image impersonateStatusImage = null;
        private Image resultImage = null;
        private string resultText = string.Empty;

        private bool isResultPanelVisible = false;
        private bool isOkButtonEnabled = false;
        private bool isCancelButtonEnabled = false;
        private bool isLogsButtonEnabled = false;

        private bool isShowExternalTokenDetailsLinkEnabled = false;
        private bool isShowServiceAccountTokenDetailsLinkEnabled = false;

        public ISubjectToken ExternalToken { get; private set; }
        public ISubjectToken ServiceAccountToken { get; private set; }

        public string Logs => string.Join("\r\n", this.logger.LogEntries);

        private readonly MemoryLogger logger;
        private readonly ITokenAdapter tokenAdapter;
        private readonly IStsAdapter stsAdapter;
        private readonly IServiceAccountAdapter serviceAccountAdapter;

        public VerifyConfigurationViewModel(
            ITokenAdapter tokenAdapter,
            IStsAdapter stsAdapter,
            IServiceAccountAdapter serviceAccountAdapter,
            MemoryLogger logger)
        {
            this.tokenAdapter = tokenAdapter.ThrowIfNull(nameof(tokenAdapter));
            this.stsAdapter = stsAdapter.ThrowIfNull(nameof(stsAdapter));
            this.serviceAccountAdapter = serviceAccountAdapter.ThrowIfNull(nameof(serviceAccountAdapter));
            this.logger = logger.ThrowIfNull(nameof(logger));
        }

        //---------------------------------------------------------------------
        // Observable properties.
        //---------------------------------------------------------------------

        public string WindowTitle
        {
            get => this.windowTitle;
            private set
            {
                this.windowTitle = value;
                RaisePropertyChange();
            }
        }

        public Image AcquireTokenStatusImage
        {
            get => this.acquireTokenStatusImage;
            private set
            {
                this.acquireTokenStatusImage = value;
                RaisePropertyChange();
            }
        }

        public Image ExchangeTokenStatusImage
        {
            get => this.exchangeTokenStatusImage;
            private set
            {
                this.exchangeTokenStatusImage = value;
                RaisePropertyChange();
            }
        }

        public Image ImpersonateStatusImage
        {
            get => this.impersonateStatusImage;
            private set
            {
                this.impersonateStatusImage = value;
                RaisePropertyChange();
            }
        }

        public bool IsImpersonationStatusVisible
            => this.serviceAccountAdapter.IsEnabled;

        public Image ResultImage
        {
            get => this.resultImage;
            private set
            {
                this.resultImage = value;
                RaisePropertyChange();
            }
        }

        public string ResultText
        {
            get => this.resultText;
            private set
            {
                this.resultText = value;
                RaisePropertyChange();
            }
        }

        public bool IsResultPanelVisible
        {
            get => this.isResultPanelVisible;
            private set
            {
                this.isResultPanelVisible = value;
                RaisePropertyChange();
            }
        }

        public bool IsOkButtonEnabled
        {
            get => this.isOkButtonEnabled;
            private set
            {
                this.isOkButtonEnabled = value;
                RaisePropertyChange();
            }
        }

        public bool IsCancelButtonEnabled
        {
            get => this.isCancelButtonEnabled;
            private set
            {
                this.isCancelButtonEnabled = value;
                RaisePropertyChange();
            }
        }
        public bool IsLogsButtonEnabled
        {
            get => this.isLogsButtonEnabled;
            private set
            {
                this.isLogsButtonEnabled = value;
                RaisePropertyChange();
            }
        }

        public bool IsShowExternalTokenDetailsLinkEnabled
        {
            get => this.isShowExternalTokenDetailsLinkEnabled;
            private set
            {
                this.isShowExternalTokenDetailsLinkEnabled = value;
                RaisePropertyChange();
            }
        }

        public bool IsShowServiceAccountTokenDetailsLinkEnabled
        {
            get => this.isShowServiceAccountTokenDetailsLinkEnabled;
            private set
            {
                this.isShowServiceAccountTokenDetailsLinkEnabled = value;
                RaisePropertyChange();
            }
        }

        //---------------------------------------------------------------------
        // Action.
        //---------------------------------------------------------------------

        public async Task PerformTestAsync(
            CancellationToken cancellationToken)
        {
            this.AcquireTokenStatusImage = Resources.Wait_16;
            this.ExchangeTokenStatusImage = Resources.Wait_16;
            this.ImpersonateStatusImage = Resources.Wait_16;
            this.IsCancelButtonEnabled = true;
            this.IsOkButtonEnabled = false;

            var user = WindowsIdentity.GetCurrent().Name;
            this.WindowTitle = $"Test configuration as {user}";

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                //
                // (1) Acquire token.
                //
                ISubjectToken externalToken;
                try
                {
                    externalToken = await this.tokenAdapter
                        .AcquireTokenAsync(
                            TokenAcquisitionOptions.ExtensiveValidation,
                            cancellationToken)
                        .ConfigureAwait(true);

                    this.AcquireTokenStatusImage = Resources.Success_16;
                    this.IsShowExternalTokenDetailsLinkEnabled = true;
                    this.ExternalToken = externalToken;
                }
                catch (Exception)
                {
                    this.AcquireTokenStatusImage = Resources.Error_16;
                    this.ExchangeTokenStatusImage = null;
                    this.ImpersonateStatusImage = null;

                    throw;
                }

                //
                // (2) Exchange token.
                //
                TokenResponse stsToken;
                try
                {
                    stsToken = await this.stsAdapter
                        .ExchangeTokenAsync(
                            externalToken,
                            CredentialConfiguration.DefaultScopes,
                            cancellationToken)
                        .ConfigureAwait(true);

                    this.ExchangeTokenStatusImage = Resources.Success_16;
                }
                catch (Exception)
                {
                    this.ExchangeTokenStatusImage = Resources.Error_16;
                    this.ImpersonateStatusImage = null;

                    throw;
                }

                if (this.serviceAccountAdapter.IsEnabled)
                {
                    //
                    // (3) Impersonate.
                    //
                    try
                    {
                        if (!await this.serviceAccountAdapter
                            .ExistsAsync(cancellationToken)
                            .ConfigureAwait(true))
                        {
                            throw new ArgumentException(
                                $"Service account {serviceAccountAdapter.ServiceAccountEmail}' does not exist");
                        }

                        var token = await serviceAccountAdapter
                            .ImpersonateAsync(
                                stsToken.AccessToken,
                                CredentialConfiguration.DefaultScopes,
                                cancellationToken)
                            .ConfigureAwait(true);

                        this.ServiceAccountToken = await serviceAccountAdapter
                            .IntrospectTokenAsync(
                                token.AccessToken,
                                cancellationToken)
                            .ConfigureAwait(true);
                        this.IsShowServiceAccountTokenDetailsLinkEnabled = true;
                        this.ImpersonateStatusImage = Resources.Success_16;
                    }
                    catch (Exception)
                    {
                        this.ImpersonateStatusImage = Resources.Error_16;

                        throw;
                    }
                }

                this.ResultImage = Resources.Success_16;
                this.ResultText = "Test completed successfully.";
                this.IsResultPanelVisible = true;
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                this.logger.Error(e, "{0}", e.Message);

                this.ResultImage = Resources.Error_16;
                this.ResultText = e.FullMessage();
                this.IsResultPanelVisible = true;
                this.IsLogsButtonEnabled = true;
            }
            finally
            {
                this.IsCancelButtonEnabled = false;
                this.IsOkButtonEnabled = true;
            }
        }
    }
}
