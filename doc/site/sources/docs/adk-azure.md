# Azure workload identity federation

This article describes how you can let an ADK agent authenticate
to Microsoft Azure by using [workload identity federation :octicons-link-external-16:](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation).

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run
*   [ ] Your agent uses agent identity or has an attached service account
*   [ ] You want the agent to access an API or MCP tool that requires 
        Entra authentication

## Workload identity federation

ADK agents running on Agent Platform or Cloud Run can obtain an ID token that 
asserts their agent identity or the identity of their attached service account.

By setting up workload identity federation, you can let agents use this ID token
and exchange it for an Entra-issued access token. Agents can then use this
token to perform authenticated calls to Azure APIs or other APIs that
require Entra authentication.

[`AzureServiceAuthProvider`](azure_auth.py) is an ADK authentication provider 
that implements the necessary logic to obtain an ID token and perform a token exchange.

Using `AzureServiceAuthProvider` is an alternative to using 
[2-legged OAuth with auth manager :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/auth-with-2lo)
that lets you avoid the need to store client secrets.

## Set up workload identity federation in Entra

To use workload identity federation, create an app registration in Entra
that corresponds to your agent:

1.  In Entra, create a new App registration.
1.  Go to **Certificate & Secrets > Federated Credentials** and click **Add credential**.
1.  Configure the federated credential. The configuration differs based on whether
    your agent uses agent identity or an attached service account:

    === "Service account"

        +   **Scenario**: **Other issuer**
        +   **Issuer**: `https://accounts.google.com`            
        +   **Type**: **Explicit subject identifier**.
        +   **Value**: Email address of the service account.
        +   **Name**: `AgentIdentity`.
        +   **Audience**: Keep `api://AzureADTokenExchange` or enter a custom audience.

    === "Agent identity"

        +   **Scenario**: **Other issuer**
        +   **Issuer**: 
        
            ```
            https://sts.googleapis.com/v1/organizations/ORG_ID /locations/global/workloadIdentityPools/agents.global.org-ORG_ID.system.id.goog
            ```
        
            Replace `ORG_ID` with the organization ID of the Google Cloud organization that contains the agent.
            
        +   **Type**: **Explicit subject identifier**.
        +   **Value**: 
        
            ```
            spiffe://agents.global.org-ORG_ID.system.id.goog/resources/aiplatform/RESOURCE_NAME
            ```
        
            Replace the following:
            
            +   `ORG_ID`: the organization ID of the Google Cloud organization that contains the agent
            +   `RESOURCE_NAME`: the resource name of the Agent Runtime deployment as shown in the 
                Cloud Console under **Agent Platform > Agents > Deployments**. 
            
                The resource name looks similar to `projects/1234567890/locations/asia-southeast1/reasoningEngines/5678901234567890`.        

        +   **Name**: `AgentIdentity`.
        +   **Audience**: Keep `api://AzureADTokenExchange` or enter a custom audience.


## Use the `AzureServiceAuthProvider`

To let your ADK agent use workload identity federation, do the following:

1.  Add the following code to your agent's initialization logic to register the provider:

    ```
    from .geminienterprise_auth import *

    CredentialManager.register_auth_provider(AzureServiceAuthProvider())
    az_auth_scheme=AzureServiceAuthProviderScheme(
        tenant_id="TENANT_ID",
        client_id="CLIENT_ID",
        audience="AUDIENCE",
        scope="SCOPE"
    )
    ```

    Replace the following:

    +   `TENANT_ID`: the tenant ID of your Entra tenant.
    +   `CLIENT_ID`: the client ID of the app registration that you created in the previous section.
    +   `AUDIENCE`: the audience of the app registration that you created in the previous section.
    +   `SCOPE` (optional): The required OAuth scope. If you omit this parameter, the agent requests
        a token for `api://CLIENT_ID/.default`.

2.  Pass the `AzureServiceAuthProviderScheme` to the 
    constructor of relevant MCP tool set. For example:

    ```
    # Tool set for Compute Engine
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://example.azure-api.net/mcp"),
        auth_scheme=az_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        az_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

