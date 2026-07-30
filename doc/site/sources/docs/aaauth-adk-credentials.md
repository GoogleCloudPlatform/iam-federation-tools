# Access credentials in ADK agents

This article describes how an ADK agent can use delegated authorization to call
MCP tools or make API requests.

## Using delegated authorization

The ADK lets agents read the forwarded access token from `context.session.state`, 
but doesn't have built-in support for using the forwarded access token for 
purposes such as making MCP tool calls.

The [`geminienterprise_auth.py` module](https://github.com/GoogleCloudPlatform/iam-federation-tools/tree/master/adk/geminienterprise_auth.py)
provided in this repository implements a custom authentication provider that
addresses this gap and exposes the forwarded access token as an 
[`AuthCredential`](AuthCredential), making it compatible with the ADK built-in 
authentication facilities.

To use the `GeminiEnterpriseDelegatedAuthProvider`, add the following code to your agent:

```py
from .geminienterprise_auth import *

CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
auth_config = AuthConfig(
    auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme()
)
```

To use the authentication provider for an MCP tool set, specify `GeminiEnterpriseDelegatedAuthProviderScheme`
as `auth_scheme`:


``` hl_lines="3"
gce_mcp = McpToolset(
    connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
    auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme()
)
```

Similarly, to use the authentication provider for an MCP tool set from Agent Registry, 
pass `GeminiEnterpriseDelegatedAuthProviderScheme` as follows:

``` py hl_lines="4"
registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
registry.get_mcp_toolset(
    f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
    GeminiEnterpriseDelegatedAuthProviderScheme(),
)
```
