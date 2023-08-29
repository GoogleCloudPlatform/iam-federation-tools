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

package com.google.solutions.tokenservice.web;

import com.google.auth.oauth2.ComputeEngineCredentials;
import com.google.auth.oauth2.GoogleCredentials;
import com.google.auth.oauth2.ImpersonatedCredentials;
import com.google.auth.oauth2.ServiceAccountCredentials;
import com.google.common.base.Strings;
import com.google.solutions.tokenservice.ApplicationVersion;
import com.google.solutions.tokenservice.URLHelper;
import com.google.solutions.tokenservice.UserId;
import com.google.solutions.tokenservice.oauth.IdTokenIssuer;
import com.google.solutions.tokenservice.oauth.ServiceAccount;
import com.google.solutions.tokenservice.oauth.WorkloadIdentityPool;
import com.google.solutions.tokenservice.oauth.mtls.XlbMtlsClientCredentialsFlow;
import com.google.solutions.tokenservice.platform.LogAdapter;
import io.vertx.core.http.HttpServerRequest;

import javax.enterprise.context.ApplicationScoped;
import javax.enterprise.context.Dependent;
import javax.enterprise.inject.Produces;
import java.io.IOException;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.Stream;

/**
 * Provides access to runtime configuration (AppEngine, local). To be injected using CDI.
 */
@ApplicationScoped
public class RuntimeEnvironment {
  private static final String CONFIG_IMPERSONATE_SA = "tokenservice.impersonateServiceAccount";
  private static final String CONFIG_DEBUG_MODE = "tokenservice.debug";

  private final ServiceAccount serviceAccount;

  /**
   * Configuration, based on app.yaml environment variables.
   */
  private final RuntimeConfiguration configuration = new RuntimeConfiguration(System::getenv);

  // -------------------------------------------------------------------------
  // Private helpers.
  // -------------------------------------------------------------------------

  public boolean isRunningOnCloudRun() {
    return System.getenv().containsKey("K_SERVICE");
  }

  // -------------------------------------------------------------------------
  // Public methods.
  // -------------------------------------------------------------------------

  public RuntimeEnvironment() {
    //
    // Create a log adapter. We can't rely on injection as the adapter
    // is request-scoped.
    //
    var logAdapter = new LogAdapter();

    //
    // Validate options.
    //
    var validClientIdHeaders = Set.of(
      this.configuration.mtlsClientCertSpiffeIdHeader.getValue(),
      this.configuration.mtlsClientCertDnsSansHeader.getValue(),
      this.configuration.mtlsClientCertUriSansHeader.getValue(),
      this.configuration.mtlsClientCertHashHeader.getValue());

    if (!validClientIdHeaders.contains(this.configuration.mtlsClientIdHeader.getValue())) {
      throw new RuntimeException(
        String.format(
          "The header '%s' cannot be used as client ID header. " +
            "Use one of the following headers instead: %s.",
          this.configuration.mtlsClientIdHeader.getValue(),
          String.join(", ", validClientIdHeaders)));
    }

    if (isRunningOnCloudRun()) {
      //
      // Initialize using service account attached to AppEngine or Cloud Run.
      //
      try {
        var applicationCredentials = GoogleCredentials.getApplicationDefault();

        this.serviceAccount = new ServiceAccount(
          new UserId(((ComputeEngineCredentials) applicationCredentials).getAccount()),
          applicationCredentials);

        logAdapter
          .newInfoEntry(
            LogEvents.RUNTIME_STARTUP,
            String.format("Running as %s, version %s",
              this.serviceAccount,
              ApplicationVersion.VERSION_STRING))
          .write();
      }
      catch (IOException e) {
        logAdapter
          .newErrorEntry(
            LogEvents.RUNTIME_STARTUP,
            "Failed to lookup instance metadata", e)
          .write();
        throw new RuntimeException("The runtime environment failed to initialize ", e);
      }
    }
    else if (isDebugModeEnabled()) {
      //
      // Initialize using development settings and credential.
      //
      try {
        var defaultCredentials = GoogleCredentials.getApplicationDefault();

        var impersonateServiceAccount = System.getProperty(CONFIG_IMPERSONATE_SA);
        if (impersonateServiceAccount != null && !impersonateServiceAccount.isEmpty()) {
          //
          // Use the application default credentials (ADC) to impersonate a
          // service account. This can be used when using user credentials as ADC.
          //
          var impersonatedCredentials = ImpersonatedCredentials.create(
            defaultCredentials,
            impersonateServiceAccount,
            null,
            Stream.of(ServiceAccount.OAUTH_SCOPE)
              .distinct()
              .collect(Collectors.toList()),
            0);

          //
          // If we lack impersonation permissions, ImpersonatedCredentials
          // will keep retrying until the call timeout expires. The effect
          // is that the application seems hung.
          //
          // To prevent this from happening, force a refresh here. If the
          // refresh fails, fail application startup.
          //
          impersonatedCredentials.refresh();

          this.serviceAccount = new ServiceAccount(
            new UserId(impersonateServiceAccount),
            impersonatedCredentials);
        }
        else if (defaultCredentials instanceof ServiceAccountCredentials) {
          //
          // Use ADC as-is.
          //
          this.serviceAccount = new ServiceAccount(
            new UserId(((ServiceAccountCredentials) defaultCredentials).getServiceAccountUser()),
            defaultCredentials);
        }
        else {
          throw new RuntimeException(String.format(
            "You're using user credentials as application default "
              + "credentials (ADC). Use -D%s=<service-account-email> to impersonate "
              + "a service account during development",
            CONFIG_IMPERSONATE_SA));
        }
      }
      catch (IOException e) {
        throw new RuntimeException("Failed to lookup application credentials", e);
      }

      logAdapter
        .newWarningEntry(
          LogEvents.RUNTIME_STARTUP,
          String.format("Running in development mode as %s", this.serviceAccount))
        .write();
    }
    else {
      throw new RuntimeException(
        "Application is not running on Cloud Run and debug mode is disabled. Aborting startup");
    }
  }

  public boolean isDebugModeEnabled() {
    return Boolean.getBoolean(CONFIG_DEBUG_MODE);
  }

  // -------------------------------------------------------------------------
  // CDI Producer methods.
  // -------------------------------------------------------------------------

  @Produces
  @ApplicationScoped
  public ServiceAccount getServiceAccount() {
    return this.serviceAccount;
  }

  @Produces
  @ApplicationScoped
  public RuntimeConfiguration getConfiguration() {
    return this.configuration;
  }

  @Produces
  @Dependent
  public XlbMtlsClientCredentialsFlow.Options getXlbMtlsClientCredentialsFlowOptions() {
    return new XlbMtlsClientCredentialsFlow.Options(
      this.configuration.mtlsClientIdHeader.getValue(),
      this.configuration.mtlsClientCertPresentHeader.getValue(),
      this.configuration.mtlsClientCertChainVerifiedHeader.getValue(),
      this.configuration.mtlsClientCertErrorHeader.getValue(),
      this.configuration.mtlsClientCertSpiffeIdHeader.getValue(),
      this.configuration.mtlsClientCertDnsSansHeader.getValue(),
      this.configuration.mtlsClientCertUriSansHeader.getValue(),
      this.configuration.mtlsClientCertHashHeader.getValue(),
      this.configuration.mtlsClientCertSerialNumberHeader.getValue(),
      this.configuration.mtlsClientCertNotBeforeHeader.getValue(),
      this.configuration.mtlsClientCertNotAfterHeader.getValue());
  }

  @Produces
  @Dependent
  public IdTokenIssuer.Options getTokenIssuerOptions(
    HttpServerRequest request
  ) throws MalformedURLException {
    URL issuerId;
    if (!Strings.isNullOrEmpty(this.configuration.tokenIssuer.getValue()))
    {
      //
      // Use configured issuer URL.
      //
      issuerId = URLHelper.fromString(this.configuration.tokenIssuer.getValue());
    }
    else {
      //
      // Determine issuer URL from request URL.
      //
      // Because the load balancer terminates HTTPS, we have
      // to force the scheme back to https://.
      //
      issuerId = new URL(new URL(
        request.absoluteURI().replace("http:", "https:")), "/");
    }

    return new IdTokenIssuer.Options(
      issuerId,
      getWorkloadIdentityPoolOptions().expectedTokenAudience(),
      this.configuration.tokenValidity.getValue()
    );
  }

  @Produces
  @Dependent
  public WorkloadIdentityPool.Options getWorkloadIdentityPoolOptions() {
    if (!this.configuration.workloadIdenityProjectNumber.isValid()) {
      throw new RuntimeException("The workload identity project number is invalid");
    }

    if (!this.configuration.workloadIdenityPoolId.isValid()) {
      throw new RuntimeException("The workload identity pool ID is invalid");
    }

    if (!this.configuration.workloadIdenityProviderIdId.isValid()) {
      throw new RuntimeException("The workload identity provider number is invalid");
    }

    return new WorkloadIdentityPool.Options(
      this.configuration.workloadIdenityProjectNumber.getValue(),
      this.configuration.workloadIdenityPoolId.getValue(),
      this.configuration.workloadIdenityProviderIdId.getValue());
  }
}
