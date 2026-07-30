## ADK Authentication Providers

This folder contains a selection of authentication providers for ADK agents.


## Gemini Enterprise authentication provider

The `GeminiEnterpriseDelegatedAuthProvider` lets agents use credentials forwarded 
by [Gemini Enterprise](https://docs.cloud.google.com/gemini/enterprise/docs) and act on 
the end user's behalf.

When you register an ADK agent in Gemini Enterprise, you can 
[configure authorization](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-adk-agent#authorize-your-agent). 
Gemini Enterprise then prompts users to perform an OAuth 2.0 authorization flow before 
letting them interact with your agent, and forwards the resulting access token to the agent.

The ADK lets agents read the forwarded access token from `context.session.state`, 
but doesn't have built-in support for using the forwarded access token for 
purposes such as making MCP tool calls.

The `GeminiEnterpriseDelegatedAuthProvider` addresses this gap and exposes the 
forwarded access as an [`AuthCredential`](AuthCredential), making it compatible 
with the ADK built-in authentication facilities.

To use the `GeminiEnterpriseDelegatedAuthProvider`, add the following code to your agent:


```
from .geminienterprise_auth import *

CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
auth_config = AuthConfig(
    auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme()
)
```

If you've configured more than one authorization for the agent in Gemini Enterprise,
use the `name` parameter to select the authorization you want to use:

```
from .geminienterprise_auth import *

CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
auth_config = AuthConfig(
    auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme(
        name="second-authorization"
    )
)
```

To use the authentication provider for an MCP tool set, use the `auth_scheme` parameter as follows:

```
gce_mcp = McpToolset(
    connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
    auth_scheme=auth_config.auth_scheme,
)
```

Similarly, to use the authentication provider for an MCP tool set from Agent Registry, use the `auth_scheme` parameter as follows:

```
registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
registry.get_mcp_toolset(
    f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
    auth_config.auth_scheme,
)
```
