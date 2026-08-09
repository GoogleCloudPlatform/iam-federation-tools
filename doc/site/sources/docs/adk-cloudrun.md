# Authenticating to Cloud Run

This article describes how you can let an ADK agent authenticate to an 
API or MCP server that runs on Cloud Run and 
[requires service-to-service authentication :octicons-link-external-16:](https://docs.cloud.google.com/run/docs/authenticating/service-to-service#use_workload_identity_federation_from_outside).

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run
*   [ ] Your agent is configured to use agent identity or has an attached service account
*   [ ] You want the agent to access an API or MCP tool running on Cloud Run that 
        requires service-to-service authentication

## Cloud Run service-to-service authentication

To authenticate to Cloud Run, an ADK agent must provide an ID token that meets
the following criteria:

+   The `aud` claim must match the Cloud Run app's URL or its
    [custom audience :octicons-link-external-16:](https://docs.cloud.google.com/run/docs/configuring/custom-audiences).
+   The `sub` claim must identify a service account with `roles/run.invoker`
    access to the Cloud Run app.
+   The `iss` claim must equal `https://accounts.google.com`, indicating that the
    ID token is issued by Google.

The following example shows a decoded ID token that lets an ADK agent 
authenticate to a Cloud Run app `https://example.asia-southeast1.run.app/mcp/`:

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

The right way to obtain a suitable ID token depends on whether the ADK agent
uses agent identity or an attached service account:

+   Agents deployed with an attached service account can obtain a suitable
    ID token from their metadata server. 
+   Agents that use agent identity can't use the ID token provided by their
    metadata server because that ID token is a 
    [JWT-SVID :octicons-link-external-16:](https://docs.cloud.google.com/docs/authentication/token-types#agent-identity-id-tokens)
    and does not meet the criteria above.

    Instead, they must impersonate a service account by using
    [`generateIdToken` :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/reference/credentials/rest/v1/projects.serviceAccounts/generateIdToken).

[`CloudRunServiceAuthProvider`](azure_cloudrun.py) is an ADK authentication provider 
that implements the necessary logic to obtain an ID token.

## Use the authentication provider

To let your ADK agent use workload identity federation, do the following:

1.  Add the following code to your agent's initialization logic to register the provider:

    ```
    from .cloudrun_auth import *
    CredentialManager.register_auth_provider(CloudRunServiceAuthProvider())
    cloudrun_auth_scheme=CloudRunServiceAuthProviderScheme(
        service_url="https://example.asia-southeast1.run.app/",
        service_account="cloudrun-client@example-project.iam.gserviceaccount.com"
    )
    ```

    Replace the following:

    +   `service_url`: the base URL of the Cloud Run app, or its custom audience.
    +   `service_account`: email address of the service account to use for obtaining 
        an ID token.

        This parameter is only required for agents that use agent identity.
        The agent must have _Service Account Token Creator_ access to the service account.


2.  Pass the `CloudRunServiceAuthProviderScheme` to the 
    constructor of relevant MCP tool set. For example:

    ```
    # Tool set for Compute Engine
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="example.asia-southeast1.run.app/mcp"),
        auth_scheme=cloudrun_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        cloudrun_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

