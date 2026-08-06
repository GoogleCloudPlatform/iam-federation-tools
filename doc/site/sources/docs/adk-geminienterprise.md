# Gemini Enterprise delegated authorization

This article describes how you can configure Gemini Enterprise and
an ADK agent to use delegated authorization.

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform
*   [ ] Users access the agent by using the Gemini Enterprise web app
*   [ ] You want the agent to perform API calls or MCP tool calls under the user's identity

## Delegated authorization

When you deploy an agent on Agent Platform and register it in Gemini Enterprise, you can optionally
[configure authorization :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-adk-agent#authorize-your-agent). 
Gemini Enterprise then prompts users to perform an OAuth 2.0 authorization flow before 
letting them interact with your agent, and forwards the resulting access token to the agent.

Although ADK 1.x agents can read the forwarded access token from `context.session.state`,
the ADK provides limited support for using the token for 
purposes such as making MCP tool calls.

[`GeminiEnterpriseDelegatedAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/geminienterprise_auth.py) 
is an ADK authentication provider that addresses this gap and exposes the 
forwarded access token as an [`AuthCredential`](AuthCredential), making it compatible 
with the ADK built-in authentication facilities.

## Set up delegated authorization in Gemini Enterprise

To set up delegated authorization in Gemini Enterprise, follow the instructions
in [Configure authorization details :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-adk-agent#register-an-adk-agent).

Delegated authorization works with any OAuth 2.0-compliant identity provider,
including [AAAuth](aaauth.md), but the exact configuration parameters depend on identity provider
you use.

## Use the `GeminiEnterpriseDelegatedAuthProvider`

To let your ADK agent use delegated authorization, do the following:

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
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
        auth_scheme=ge_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        ge_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

