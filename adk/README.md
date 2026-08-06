## ADK Authentication Providers

This folder contains authentication providers for ADK agents.

## Gemini Enterprise delegated authentication provider

[`GeminiEnterpriseDelegatedAuthProvider`](geminienterprise_auth.py) is an
ADK authentication provider that lets agents use credentials forwarded by
[Gemini Enterprise](https://docs.cloud.google.com/gemini/enterprise/docs) 
and act on the end user's behalf.

When you register an ADK agent in Gemini Enterprise, you can 
[configure authorization](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-adk-agent#authorize-your-agent). 
Gemini Enterprise then prompts users to perform an OAuth 2.0 authorization flow before 
letting them interact with your agent, and forwards the resulting access token to the agent.
The ADK lets agents read the forwarded access token from `context.session.state`, 
but doesn't have built-in support for using the forwarded access token for 
purposes such as making MCP tool calls.

`GeminiEnterpriseDelegatedAuthProvider` addresses this gap and exposes the 
forwarded access token as an [`AuthCredential`](AuthCredential), making it compatible 
with the ADK built-in authentication facilities.

You can use `GeminiEnterpriseDelegatedAuthProvider` as follows:

1.  Add the following code to your agent's initialization logic to register the provider:

    ```
    from .geminienterprise_auth import *

    CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
    ge_auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme()
    ```

    If you've configured more than one authorization for the agent in Gemini Enterprise,
    use the `name` parameter to specify the name of the authorization to use:

    ```
    from .geminienterprise_auth import *

    CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
    ge_auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme(
        name="my-authorization"
    )
    ```

2.  Pass the `GeminiEnterpriseDelegatedAuthProviderScheme` to the 
    constructor of relevant MCP tool set. For example:

    ```
    # Tool set for Compute Engine
    gce_mcp = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
        auth_scheme=ge_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        ge_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    **Important**: If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).


`GeminiEnterpriseDelegatedAuthProvider` is designed to work with any OAuth 2.0-compliant 
identity provider, including Google, Microsoft Entra, and GitHub.


## Azure workload identity authentication provider

[`AzureServiceAuthProvider`](azure_auth.py) is an ADK authentication provider 
that lets agents use [workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation)
to authenticate to Microsoft Azure. 

Using workload identity federation lets you avoid the need to store client
secrets and is an alternative to using 
[2-legged OAuth with auth manager](https://docs.cloud.google.com/iam/docs/auth-with-2lo).


You can use `AzureServiceAuthProvider` as follows:

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

    +   `TENANT_ID`: the tenant ID of your Entra tenant
    +   `CLIENT_ID`: TODO
    +   `AUDIENCE`: TODO
    +   `SCOPE`: TODO

2.  Pass the `AzureServiceAuthProviderScheme` to the 
    constructor of relevant MCP tool set. For example:

    ```
    # Tool set for Compute Engine
    gce_mcp = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://example.azure-api.net/mcp"),
        auth_scheme=az_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        az_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    **Important**: If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

