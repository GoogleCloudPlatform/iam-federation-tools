# Copyright 2026 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from __future__ import annotations

import logging
import os

from typing import Literal
from typing import Optional

from google.adk.agents.callback_context import CallbackContext
from google.adk.auth.auth_credential import AuthCredential
from google.adk.auth.auth_credential import AuthCredentialTypes
from google.adk.auth.auth_credential import OAuth2Auth
from google.adk.auth.auth_tool import AuthConfig
from google.adk.auth.base_auth_provider import BaseAuthProvider
from google.adk.auth.auth_schemes import CustomAuthScheme

from pydantic import Field
from typing_extensions import override

logger = logging.getLogger('google_solutions.' + __name__)

class GeminiEnterpriseDelegatedAuthProviderScheme(CustomAuthScheme):
  """Authentication scheme for Gemini Enterprise-managed authentication.

  When the agent is registered in Gemini Enterprise and configured to 
  use authorization, Gemini Enterprise performs an OAuth authorization
  flow on behalf of the agent and passes the resulting access token
  to the agent. 
  
  This scheme exposes this access token as an AuthCredential
  that can be used for purposes such as authenticating MCP calls.

  Attributes:
    name: The name of the authorization configured in Gemini Enterprise.
    type_: The type of the security scheme, always "GeminiEnterpriseDelegatedAuthProviderScheme".
  """

  type_: Literal["GeminiEnterpriseDelegatedAuthProviderScheme"] = Field(
      default="GeminiEnterpriseDelegatedAuthProviderScheme", alias="type"
  )
  name: Optional[str] = None

class GeminiEnterpriseDelegatedAuthProvider(BaseAuthProvider):
  """Auth provider for Gemini Enterprise-managed authentication."""

  def __init__(self):
    # When GOOGLE_API_USE_CLIENT_CERTIFICATE is true, MCPSessionManager
    # ignores custom authentication and forces connection to use ADC
    # instead.
    
    if (os.environ.get('GOOGLE_API_USE_CLIENT_CERTIFICATE', 'true').lower() == 'true'):
      logger.warning('To use Gemini Enterprise authentication for MCP, ' \
      'the environment variable GOOGLE_API_USE_CLIENT_CERTIFICATE must be set to false')

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[GeminiEnterpriseDelegatedAuthProviderScheme], ...]:
    return (GeminiEnterpriseDelegatedAuthProviderScheme,)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext | None,
  ) -> AuthCredential:
    """Retrieves Gemini Enterprise-provided credentials from the user's session.

    Args:
      auth_config: The authentication configuration.
      context: Optional context for the callback.

    Returns:
      An AuthCredential instance.

    Raises:
      ValueError: If auth_scheme is not a GeminiEnterpriseDelegatedAuthProviderScheme or 
        Gemini Enterprise did not provide a token.
    """
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, GeminiEnterpriseDelegatedAuthProviderScheme):
      raise ValueError(
          f"Expected GeminiEnterpriseDelegatedAuthProviderScheme, got {type(auth_scheme)}"
      )

    if context is None or context.session is None:
      raise ValueError(
          "GeminiEnterpriseDelegatedAuthProviderScheme requires a context with a valid session."
      )

    if auth_scheme.name and auth_scheme.name in context.session.state:
      # Use specific authorization.
      token = context.session.state[auth_scheme.name]
    elif len(context.session.state) == 1:
      # Use the only authorization that is available.
      token = list(context.session.state.values())[0]
    else:
      # There are multiple tokens, and we don't know which one to use.
      raise ValueError(
          f"No matching Gemini Enterprise authorization found in session."
      )

    # Wrap token as an AuthCredential.
    return AuthCredential(
        auth_type=AuthCredentialTypes.OAUTH2,
        oauth2=OAuth2Auth(
            access_token=token
        ),
    )
