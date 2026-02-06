# Access credentials in ADK agents

The ADK surfaces delegated user credentials in the agent's session state.
You can access credentials directly by reading from
`tool_context.session.state[AUTHORIZATION_ID]`, where `AUTHORIZATION_ID` is one 
of the following:

*   `aaauth-google-identity` if you use Google identity as your identity provider
*   `aaauth-entra-delegated` if you use workload identity federation and Entra 
     as your identity provider 

Alternatively, you can add a custom 
[function tool :octicons-link-external-16:](https://google.github.io/adk-docs/tools-custom/function-tools/)
that injects the delegated user credentials as a `AuthCredential` parameter to your
tool function:

```
from __future__ import annotations

import inspect

from typing import Any, Callable, Optional, Union
from typing_extensions import override
from google.adk.auth.auth_credential import AuthCredential, AuthCredentialTypes, OAuth2Auth
from google.adk.tools import ToolContext, FunctionTool

class GeminiEnterpriseAuthorizedTool(FunctionTool):
    """A FunctionTool that lets a tool function access the delegated credentials
    of a Gemini Enterprise user (if present).
    """

    def __init__(
        self,
        *,
        func: Callable[..., Any],
        authorization: str
    ):
        """Initializes the GeminiEnterpriseAuthorizedTool.

        Args:
            func: The function to be called.
            authorization: ID of the Gemini Enterprise authorization. 
        """
        super().__init__(func=func)
        self._ignore_params.append("credential")
        self._authorization = authorization

    @override
    async def run_async(
        self, *, args: dict[str, Any], tool_context: ToolContext
    ) -> Any:
        # Obtain token from session state and, if present, inject it
        # into the function call.

        access_token = tool_context.session.state.get(self._authorization)
        if access_token:
            credential = AuthCredential(
                auth_type=AuthCredentialTypes.OPEN_ID_CONNECT,
                oauth2=OAuth2Auth(
                    access_token = access_token
                )
            )

            return await self._run_async_impl(
                args=args, tool_context=tool_context, credential=credential
            )
        else:
            return {
                "error": (
                    "This tool requires authorization."
                )
            }

    async def _run_async_impl(
        self,
        *,
        args: dict[str, Any],
        tool_context: ToolContext,
        credential: AuthCredential,
    ) -> Any:
        args_to_call = args.copy()
        signature = inspect.signature(self.func)
        
        if "credential" in signature.parameters:
            args_to_call["credential"] = credential

        return await super().run_async(args=args_to_call, tool_context=tool_context)

```

You can use the `GeminiEnterpriseAuthorizedTool` as follows:

```

def my_tool(credential: AuthCredential):
    """
    Authenticted tool example
    
    Args:
        credentials: Delegated user credentials, injected by the GeminiEnterpriseAuthorizedTool
    """
        
    # Retrieve the "raw" access token
    access_token = credential.oauth2.access_token
    
    # Example 1: Use the token to initialize the client library.
    service = build(..., credentials=Credentials(access_token))

    # Example 2: Use the token to call a Cloud Run service or an IAP-protected resource
    #
    #  - For IAP, configure the AAAuth client ID for programmatic access, see
    #    https://docs.cloud.google.com/iap/docs/sharing-oauth-clients#programmatic_access
    #  - For Cloud Run, add the AAAuth client ID as custom audience, see 
    #    https://docs.cloud.google.com/run/docs/configuring/custom-audiences
    #
    response = requests.get(
        "https://.../", 
        headers={
            'Authorization': f'Bearer {access_token}',
            'Content-Type': 'application/json'
        })
    
root_agent = Agent(
    ...
    tools=[
        GeminiEnterpriseAuthorizedTool(func=my_tool, authorization="aaauth-google-identity")
    ]
)
```
