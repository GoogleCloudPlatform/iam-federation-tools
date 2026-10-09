---
title: Delegating access from Gemini Enterprise to an A2A agent
---

# Delegating access from Gemini Enterprise to an A2A agent

This article describes how you can configure Gemini Enterprise and
an A2A agent built with the ADK to use delegated authorization.

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Cloud Run and supports 
        [A2A :octicons-link-external-16:](https://google.github.io/adk-docs/a2a/)
*   [ ] The agent is registered in Gemini Enterprise as an _A2A agent_
*   [ ] Users access the agent by using the Gemini Enterprise web app
*   [ ] You want the agent to perform API calls or MCP tool calls under the user's identity

## Delegated authorization

When you register an A2A agent in Gemini Enterprise, you can optionally
[configure OAuth 2.0 for end-user authorization :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-a2a-agent#authorize-your-agent).
Gemini Enterprise then prompts users to perform an OAuth 2.0 authorization flow before
letting them interact with your agent, and
[forwards the resulting access token in the `Authorization` HTTP request header :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-a2a-agent#auth-headers)
(`Authorization: Bearer TOKEN`).

[`HttpHeaderAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/httpheader_auth.py)
is an ADK authentication provider lets ADK agents use the forwarded
access token as an [`AuthCredential`](AuthCredential) to make MCP
tool calls or API requests.

## Set up delegated authorization in Gemini Enterprise

To set up delegated authorization for an A2A agent in Gemini Enterprise, follow the instructions
in [Configure OAuth 2.0 for end-user authorization :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-a2a-agent#authorize-your-agent)
and [Register an A2A agent with Gemini Enterprise :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-a2a-agent#register-agent).

Delegated authorization works with any OAuth 2.0-compliant identity provider,
including [AAAuth](aaauth.md), but the exact configuration parameters depend on the identity provider
you use.

## Use the authentication provider

To let your A2A agent use delegated authorization, do the following:

1.  Add the following code to your agent's initialization logic to register the provider:

    ```
    from .httpheader_auth import *

    CredentialManager.register_auth_provider(HttpHeaderAuthProvider())
    header_auth_scheme=HttpHeaderAuthProviderScheme()
    ```

    By default, `HttpHeaderAuthProviderScheme` extracts a `Bearer` token from the
    `Authorization` HTTP request header. If you need to use a different HTTP header, 
    use the `name` and `scheme` parameters to customize how the token is extracted:

    ```
    from .httpheader_auth import *

    CredentialManager.register_auth_provider(HttpHeaderAuthProvider())
    header_auth_scheme=HttpHeaderAuthProviderScheme(
        name="X-Forwarded-Access-Token",
        scheme="Custom"
    )
    ```

2.  Pass the `HttpHeaderAuthProviderScheme` to the
    constructor of relevant MCP tool set. For example:

    ```
    # Tool set for Compute Engine
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
        auth_scheme=header_auth_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        header_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).
