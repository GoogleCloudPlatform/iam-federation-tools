# Access credentials in ADK agents

This article describes how an ADK agent can use delegated authorization to call
MCP tools or make API requests.

## Using delegated authorization

By default, the ADK uses the agent's own identity to authenticate MCP tool calls. To
let it use the identity of the end user instead, you must use a custom
authentication provider.

The [`geminienterprise_auth.py` module](https://github.com/GoogleCloudPlatform/iam-federation-tools/tree/master/aaauth/adk/geminienterprise_auth.py)
provided in this repository implements a custom authentication provider that
uses the end user credentials forwarded by Gemini Enterprise.

To initialize the authentication provider, add the following code to your agent:

```py
from .geminienterprise_auth import *

CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
auth_config = AuthConfig(
    auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme()
)
```

To use the authentication provider for an MCP tool set, use the `auth_scheme`
parameter as follows:


``` hl_lines="3"
gce_mcp = McpToolset(
    connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
    auth_scheme=auth_config.auth_scheme,
)
```

Similarly, to use the authentication provider for an MCP tool set from Agent Registry, use the `auth_scheme`
parameter as follows:


``` py hl_lines="4"
registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
registry.get_mcp_toolset(
    f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
    auth_config.auth_scheme,
)
```
