---
title: Letting ADK agents authenticate to Cloud Run services
---

# Letting ADK agents authenticate to Cloud Run services

This article describes how you can let an ADK agent authenticate to an 
API or MCP server that runs on Cloud Run and 
[requires service-to-service authentication :octicons-link-external-16:](https://docs.cloud.google.com/run/docs/authenticating/service-to-service#use_workload_identity_federation_from_outside).

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run
*   [ ] Your agent is configured to use agent identity or has an attached service account
*   [ ] You want the agent to access an API or MCP tool running on Cloud Run that 
        requires service-to-service authentication

## Cloud Run service-to-service authentication

To authenticate to Cloud Run, an ADK agent must provide a
[service account ID token :octicons-link-external-16:](https://docs.cloud.google.com/docs/authentication/token-types#sa-id-tokens)
or an [agent identity ID token (JWT-SVID) :octicons-link-external-16:](https://docs.cloud.google.com/docs/authentication/token-types#agent-identity-id-tokens)
that meets the following criteria:

+   The `aud` claim must match the Cloud Run app's URL or its
    [custom audience :octicons-link-external-16:](https://docs.cloud.google.com/run/docs/configuring/custom-audiences).
+   The `sub` claim must identify a service account or agent identity with `roles/run.invoker`
    access to the Cloud Run app.
+   The `iss` claim must identify Google (`https://accounts.google.com`) or the agent identity pool
    (`https://sts.googleapis.com/v1/organizations/ORG_ID/locations/global/workloadIdentityPools/agents.global.org-ORG_ID.system.id.goog`)
    as the issuer.

The following examples show decoded ID tokens that let an ADK agent 
authenticate to a Cloud Run app `https://example.asia-southeast1.run.app/mcp/`:

=== "Service account"

    ``` hl_lines="6-7"
    {
      "alg": "RS256",
      "kid": "f10f87405a979c1df36df26606734f33cd85c271",
      "typ": "JWT"
    }.{
      "aud": "https://example.asia-southeast1.run.app/",
      "sub": "102771234567890",
      "azp": "102771234567890",
      "email": "service-1234567890@gcp-sa-aiplatform-re.iam.gserviceaccount.com",
      "email_verified": true,
      "exp": 1786318370,
      "iat": 1786314770,
      "iss": "https://accounts.google.com"
    }.[Signature]
    ```

=== "Agent identity"

    ``` hl_lines="6-7"
    {
      "alg": "RS256",
      "kid": "77c25e373a13b82bb3c4bf5b7d26ddcd5fb7fb7f",
      "typ": "JWT"
    }.{
      "aud": "https://example.asia-southeast1.run.app/",
      "sub": "spiffe://agents.global.org-1234567890.system.id.goog/resources/aiplatform/projects/1234567890/locations/asia-southeast1/reasoningEngines/5678901234567890",
      "exp": 1786318370,
      "iat": 1786314770,
      "iss": "https://sts.googleapis.com/v1/organizations/1234567890/locations/global/workloadIdentityPools/agents.global.org-1234567890.system.id.goog"
    }.[Signature]
    ```

Agents can obtain a suitable ID token in two ways:

+   By querying their metadata server, which issues an ID token for the agent's
    own identity (its agent identity or attached service account).
+   By impersonating a service account using
    [`generateIdToken` :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/reference/credentials/rest/v1/projects.serviceAccounts/generateIdToken).

[`CloudRunServiceAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/cloudrun_auth.py) 
is an ADK authentication provider that implements the necessary logic to obtain an ID token.

## Use the authentication provider

To use the `CloudRunServiceAuthProvider`, do the following:

1.  Verify that you are using ADK 2.9.0 or a newer version.
2.  Add the following code to your agent's initialization logic to register the provider:

    ```
    from cloudrun_auth import *
    CredentialManager.register_auth_provider(CloudRunServiceAuthProvider())
    cloudrun_auth_scheme=CloudRunServiceAuthProviderScheme(
        service_url="https://example.asia-southeast1.run.app/"
    )
    ```

    Replace `https://example.asia-southeast1.run.app/` with the base URL of the Cloud Run app, or its custom audience.

    By default, `CloudRunServiceAuthProviderScheme` uses the agent's own identity (its agent identity or attached service account) to obtain an ID token. If you want the agent to impersonate a service account instead, specify the `service_account` parameter:

    ```
    from cloudrun_auth import *
    CredentialManager.register_auth_provider(CloudRunServiceAuthProvider())
    cloudrun_auth_scheme=CloudRunServiceAuthProviderScheme(
        service_url="https://example.asia-southeast1.run.app/",
        service_account="cloudrun-client@example-project.iam.gserviceaccount.com"
    )
    ```

    To impersonate a service account, the agent must have _Service Account Token Creator_ (`roles/iam.serviceAccountTokenCreator`) access to the service account.


3.  Pass the `CloudRunServiceAuthProviderScheme` to the 
    constructor of relevant MCP tool set. For example:

    ```
    # Custom MCP tool set
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://example.asia-southeast1.run.app/mcp"),
        auth_scheme=cloudrun_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        cloudrun_auth_scheme
    )
    ```

