---
title: Letting ADK agents authenticate to IAP-protected services
---

# Letting ADK agents authenticate to IAP-protected services

This article describes how you can let an ADK agent authenticate to an 
API or MCP server that is protected by Identity-Aware-Proxy (IAP).

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run, or elsewhere on Google Cloud
*   [ ] Your agent is configured to use agent identity or has an attached service account
*   [ ] You want the agent to access an API or MCP tool that is protected by 
        Identity-Aware-Proxy (IAP)
*   [ ] IAP is configured to use Google identities for authentication
        (as opposed to workforce identity federation or Identity Platform)

## IAP programmatic authentication

IAP lets clients [authenticate programtically :octicons-link-external-16:](https://docs.cloud.google.com/iap/docs/authentication-howto#authenticate-service-account)
using two types of tokens:

1.  [Service account JSON Web Token assertions :octicons-link-external-16:](https://docs.cloud.google.com/docs/authentication/token-types#sa-jwt-assertions)

    These are JSON Web Tokens (JWTs) that contain the target URL as
    `aud` and are signed using a service account key.

1.  [Service account ID tokens :octicons-link-external-16:](https://docs.cloud.google.com/docs/authentication/token-types#sa-id-tokens)

    These are JSON Web Tokens (JWTs) that contain a OAuth client ID as 
    `aud` and are signed using the Google JSON Web Key Set. To be 
    accepted by IAP, the OAuth client ID must be [allow-listed](https://docs.cloud.google.com/iap/docs/managing-access).

[`IapServiceAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/iap_auth.py) 
is an ADK authentication provider that implements (1):

*  The provider creates a Service account JSON Web Token assertion
   and signs it using the [`signJwt`](https://docs.cloud.google.com/iam/docs/reference/credentials/rest/v1/projects.serviceAccounts/signJwt)
   API and a service account that you specify during initialization. 
*  The provider issues an `AuthCredential` that the agent can use to 
   initialize an `McpToolset` or to invoke MCP toolsets obtained
   from Agent Registry.

## Use the authentication provider

To use the `IapServiceAuthProvider`, do the following:

1.  Add the following code to your agent's initialization logic to register the provider:

    ```
    from iap_auth import *
    CredentialManager.register_auth_provider(IapServiceAuthProvider())
    iap_auth_scheme=IapServiceAuthProviderScheme(
        service_url="https://example.asia-southeast1.run.app/",
        service_account="iap-client@example-project.iam.gserviceaccount.com"
    )
    ```

    Replace the following:

    +   `service_url`: the URL of the IAP-protected API or MCP server.
    +   `service_account`: email address of the service account to use for 
        signing the Service account JSON Web Token assertion.

        The agent must have _Service Account Token Creator_ access to the service account.

2.  Pass the `IapServiceAuthProviderScheme` to the 
    constructor of relevant MCP tool set. For example:

    ```
    # Custom MCP tool set
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://example.asia-southeast1.run.app/mcp"),
        auth_scheme=iap_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        iap_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

