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

import com.fasterxml.jackson.annotation.JsonProperty;
import com.google.common.base.Strings;
import com.google.solutions.tokenservice.Exceptions;
import com.google.solutions.tokenservice.oauth.Authentication;
import com.google.solutions.tokenservice.oauth.AuthenticationFlow;
import com.google.solutions.tokenservice.oauth.AuthenticationRequest;
import com.google.solutions.tokenservice.oauth.IdTokenIssuer;
import com.google.solutions.tokenservice.platform.LogAdapter;

import javax.enterprise.context.RequestScoped;
import javax.enterprise.inject.Instance;
import javax.inject.Inject;
import javax.ws.rs.*;
import javax.ws.rs.core.*;
import java.net.MalformedURLException;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Collection;
import java.util.List;
import java.util.stream.Collectors;

/**
 * REST API controller.
 */
@RequestScoped
@Path("/")
public class OAuthResource {
  @Inject
  RuntimeEnvironment runtimeEnvironment;

  @Inject
  RuntimeConfiguration configuration;

  @Inject
  LogAdapter logAdapter;

  @Inject
  Instance<AuthenticationFlow> flows;

  @Inject
  IdTokenIssuer tokenIssuer;



  /**
   * OAuth token endpoint.
   */
  private Authentication handleTokenRequest(
    String grantType,
    MultivaluedMap<String, String> parameters
  ) throws Exception {
    if (Strings.isNullOrEmpty(grantType))
    {
      throw new IllegalArgumentException("A grant type is required");
    }

    //
    // Find a flow that:
    // - is enabled (in the configuration)
    // - supports the requested grant type
    // - supports the presented set of request parameters
    //
    var request = new AuthenticationRequest(grantType, parameters);
    var flow = this.flows
      .stream()
      .filter(f -> this.configuration.authenticationFlows().contains(f.name()))
      .filter(f -> f.grantType().equals(grantType) && f.canAuthenticate(request))
      .findFirst();

    if (!flow.isPresent()) {
      this.logAdapter
        .newWarningEntry(
          LogEvents.API_TOKEN,
          String.format(
            "No suitable flow found for grant type '%s' (enabled flows: %s)",
            grantType,
            String.join(", ", this.configuration.authenticationFlows())))
        .write();

      throw new IllegalArgumentException(
        String.format("No suitable flow found for grant type '%s'", grantType)
      );
    }

    //
    // Run flow to authenticate the user or client.
    //
    try {
      return flow.get().authenticate(request);
    }
    catch (Exception e)
    {
      this.logAdapter
        .newErrorEntry(
          LogEvents.API_TOKEN,
          String.format("Authentication failed: %s", Exceptions.getFullMessage(e)))
        .write();

      throw (Exception) e.fillInStackTrace();
    }
  }

  // -------------------------------------------------------------------------
  // REST resources.
  // -------------------------------------------------------------------------

  /**
   * Root endpoint, redirect to OIDC metadata.
   */
  @GET
  @Produces(MediaType.APPLICATION_JSON)
  public Response getRoot() throws MalformedURLException, URISyntaxException {
    var metadataUrl = new URL(this.tokenIssuer.id() + "/.well-known/openid-configuration");

    return Response
      .temporaryRedirect(metadataUrl.toURI())
      .build();
  }

  /**
   * OIDC provider metadata.
   */
  @GET
  @Path(".well-known/openid-configuration")
  @Produces(MediaType.APPLICATION_JSON)
  public ProviderMetadata getMetadata(
    @Context UriInfo uriInfo
  ) throws MalformedURLException {
    var tokenUrl = new URL(this.tokenIssuer.id() + "/token");

    return new ProviderMetadata(
      this.tokenIssuer.id(),
      tokenUrl, // We don't have a real authorization endpoint
      tokenUrl,
      this.tokenIssuer.jwksUrl(),
      List.of("none"),
      this.flows.stream()
        .map(f -> f.grantType())
        .distinct()
        .collect(Collectors.toList()),
      List.of("none"),
      List.of("RS256"),
      this.flows.stream()
        .map(f -> f.authenticationMethod())
        .distinct()
        .collect(Collectors.toList()));
  }

  /**
   * OAuth token endpoint.
   */
  @POST
  @Path("token")
  @Consumes(MediaType.APPLICATION_FORM_URLENCODED)
  @Produces(MediaType.APPLICATION_JSON)
  public Response post(
    @FormParam("grant_type") String grantType,
    @FormParam("format") String format,
    MultivaluedMap<String, String> parameters
  ) {
    if ("external_credential".equals(format))
    {
      //
      // Return results in a format that's consumable by client libraries,
      // see https://google.aip.dev/auth/4117.
      //
      try {
        var authentication = handleTokenRequest(grantType, parameters);

        return Response
          .ok()
          .entity(new ExternalCredentialResponse(
            authentication.idToken().value(),
            authentication.idToken().expiryTime().getEpochSecond()
              - authentication.idToken().issueTime().getEpochSecond()))
          .build();
      }
      catch (IllegalArgumentException e) {
        return Response.status(Response.Status.BAD_REQUEST)
          .entity(new ExternalCredentialErrorResponse(TokenErrorResponse.INVALID_REQUEST, e))
          .build();
      }
      catch (Authentication.InvalidClientException e) {
        return Response.status(Response.Status.FORBIDDEN)
          .entity(new ExternalCredentialErrorResponse(TokenErrorResponse.UNAUTHORIZED_CLIENT, e))
          .build();
      }
      catch (Authentication.TokenIssuanceException e) {
        return Response.status(Response.Status.FORBIDDEN)
          .entity(new ExternalCredentialErrorResponse(TokenErrorResponse.ACCESS_DENIED, e))
          .build();
      }
      catch (Exception e) {
        return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
          .entity(new ExternalCredentialErrorResponse(TokenErrorResponse.SERVER_ERROR, e))
          .build();
      }
    }
    else {
      //
      // Return results in standard OAuth format.
      //
      try {
        var authentication = handleTokenRequest(grantType, parameters);
        var tokenResponse = authentication.accessToken() != null
          ? new TokenResponse(
          authentication.idToken().value(),
          authentication.accessToken().value(),
          TokenResponse.BEARER,
          authentication.accessToken().expiryTime().getEpochSecond()
            - authentication.accessToken().issueTime().getEpochSecond(),
          authentication.accessToken().scope())
          : new TokenResponse(authentication.idToken().value());

        return Response
          .ok()
          .entity(tokenResponse)
          .build();
      }
      catch (IllegalArgumentException e) {
        return Response.status(Response.Status.BAD_REQUEST)
          .entity(new TokenErrorResponse(TokenErrorResponse.INVALID_REQUEST, e))
          .build();
      }
      catch (Authentication.InvalidClientException e) {
        return Response.status(Response.Status.FORBIDDEN)
          .entity(new TokenErrorResponse(TokenErrorResponse.UNAUTHORIZED_CLIENT, e))
          .build();
      }
      catch (Authentication.TokenIssuanceException e) {
        return Response.status(Response.Status.FORBIDDEN)
          .entity(new TokenErrorResponse(TokenErrorResponse.ACCESS_DENIED, e))
          .build();
      }
      catch (Exception e) {
        return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
          .entity(new TokenErrorResponse(TokenErrorResponse.SERVER_ERROR, e))
          .build();
      }
    }
  }

  //---------------------------------------------------------------------------
  // Response entities.
  //---------------------------------------------------------------------------

  /*
   * OIDC provider metadata as defined in [OIDC.Discovery], section 3.
   */
  public record ProviderMetadata(
    @JsonProperty("issuer")
    String issuerId,

    @JsonProperty("authorization_endpoint")
    URL authorizationEndpoint,

    @JsonProperty("token_endpoint")
    URL tokenEndpoint,

    @JsonProperty("jwks_uri")
    URL jwksEndpoint,

    @JsonProperty("response_types_supported")
    Collection<String> supportedResponseTypes,

    @JsonProperty("grant_types_supported")
    Collection<String> supportedGrantTypes,

    @JsonProperty("subject_types_supported")
    Collection<String> supportedSubjectTypes,

    @JsonProperty("id_token_signing_alg_values_supported")
    Collection<String> supportedIdTokenSigningAlgorithms,

    @JsonProperty("token_endpoint_auth_methods_supported")
    Collection<String> supportedTokenEndpointAuthenticationMethods
  ) {
  }

  /**
   * Token response as defined in RFC6749.
   */
  public record TokenResponse(
    @JsonProperty("id_token")
    String idToken,

    @JsonProperty("access_token")
    String accessToken,

    @JsonProperty("token_type")
    String tokenType,

    @JsonProperty("expires_in")
    Long expiresInSeconds,

    @JsonProperty("scope")
    String scope)
  {
    public TokenResponse(String idToken) {
      this(idToken, null, null, null, null);
    }

    public static final String BEARER = "Bearer";

  }

  /**
   * Token error response entity as defined in RFC6749.
   *
   * @param error Error code
   * @param description Description
   */
  public record TokenErrorResponse(
    @JsonProperty("error")
    String error,

    @JsonProperty("error_description")
    String description
  ) {

    public TokenErrorResponse(String errorCode, Exception exception) {
      this(errorCode, exception.getMessage());
    }

    public static final String UNAUTHORIZED_CLIENT = "unauthorized_client";
    public static final String ACCESS_DENIED = "access_denied";
    public static final String INVALID_REQUEST = "invalid_request";
    public static final String SERVER_ERROR = "server_error";
    public static final String TEMPORARILY_UNAVAILABLE = "temporarily_unavailable";
  }

  /**
   * External credential response entity as defined in https://google.aip.dev/auth/4117.
   */
  public record ExternalCredentialResponse(
    @JsonProperty("success")
    boolean success,

    @JsonProperty("version")
    int version,

    @JsonProperty("id_token")
    String idToken,

    @JsonProperty("token_type")
    String tokenType,

    @JsonProperty("expiration_time")
    long expirationTime
  ) {
    public ExternalCredentialResponse(String idToken, long expirationTime) {
      this(
        true,
        1,
        idToken,
        "urn:ietf:params:oauth:token-type:id_token",
        expirationTime);
    }
  }

  /**
   * External credential error entity as defined in https://google.aip.dev/auth/4117.
   */
  public record ExternalCredentialErrorResponse(
    @JsonProperty("success")
    boolean success,

    @JsonProperty("version")
    int version,

    @JsonProperty("code")
    String code,

    @JsonProperty("message")
    String message
  ) {
    public ExternalCredentialErrorResponse(String errorCode, Exception exception) {
      this(false, 1, errorCode, exception.getMessage());
    }
  }
}
