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
from google.adk.auth.auth_schemes import AuthScheme
from google.adk.auth.base_auth_provider import BaseAuthProvider
from google.adk.auth.auth_schemes import CustomAuthScheme
from google.adk.auth.credential_manager import CredentialManager

from google.auth.transport.requests import Request
from google.oauth2 import id_token

from pydantic import Field
from typing_extensions import override

from msal import ConfidentialClientApplication

logger = logging.getLogger('google_solutions.' + __name__)

class AgentIdentityOnBehalfOfUserScheme(CustomAuthScheme):
  """Authentication scheme for Agent identity on-behalf-of authentication.

  Attributes:
    tenant_id: Entra tenant ID.
    blueprint_id: App ID of the agent identity blueprint.
    blueprint_secret (Optional): Client secret of the agent identity blueprint.
      Set this to None to use federated authentication using the 
      service account/agent identity ID token from the MDS.
    agent_identity_id: Object ID of the agent identity.
    scope: Scope to obtain a token for.
    user_auth_scheme: Scheme to authenticate the user with.
    type_: The type of the security scheme, always "MsalAgentIdentityOnBehalfOfUserScheme".
  """

  type_: Literal["MsalAgentIdentityOnBehalfOfUserScheme"] = Field(
      default="MsalAgentIdentityOnBehalfOfUserScheme", alias="type"
  )
  tenant_id: str
  blueprint_id: str
  blueprint_secret: Optional[str] = None
  agent_identity_id: str
  scope: str
  user_auth_scheme: AuthScheme



class AgentIdentityAuthProvider(BaseAuthProvider):
  """Provider for Agent identity on-behalf-of authentication. """

  def __init__(self):
   
    # When GOOGLE_API_USE_CLIENT_CERTIFICATE is true, MCPSessionManager
    # ignores custom authentication and forces connection to use ADC
    # instead.
    
    if (os.environ.get('GOOGLE_API_USE_CLIENT_CERTIFICATE', 'true').lower() == 'true'):
      logger.warning('To use MSAL authentication for MCP, ' \
      'the environment variable GOOGLE_API_USE_CLIENT_CERTIFICATE must be set to false')

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[AgentIdentityOnBehalfOfUserScheme], ...]:
    return (AgentIdentityOnBehalfOfUserScheme,)


  def get_agent_identity_exchange_token(
      self,
      auth_scheme: AgentIdentityOnBehalfOfUserScheme) -> str:
    """ Get exchange token for the agent identity """

    if auth_scheme.blueprint_secret:
      # Use client secret to authenticate the blueprint.
      blueprint_credential = auth_scheme.blueprint_secret
      
    else:
      # Use a federated credential to authenticate the blueprint.
      assertion = str(id_token.fetch_id_token(
        Request(), 
        "api://AzureADTokenExchange"))
      blueprint_credential = {
        "client_assertion" : assertion
      }

      logger.debug(
        "Obtained ID token to use as a federated credential for blueprint %s",
        auth_scheme.blueprint_id)

    blueprint_app = ConfidentialClientApplication(
        authority=f"https://login.microsoftonline.com/{auth_scheme.tenant_id}",
        client_id=auth_scheme.blueprint_id,
        client_credential=blueprint_credential
    )

    agent_identity_token_response = blueprint_app.acquire_token_for_client(
      scopes=["api://AzureADTokenExchange/.default"],
      fmi_path=auth_scheme.agent_identity_id
    )
  
    if not agent_identity_token_response:
      raise ValueError(
          f"Agent identity authentication failed:"
      )
    elif "access_token" not in agent_identity_token_response:
      error = agent_identity_token_response["error"]
      error_description = agent_identity_token_response["error_description"]
      raise ValueError(
          f"Agent identity authentication failed: {error_description} ({error})"
      )
    else:
      logger.debug(
        "Obtained token from blueprint %s for agent identity %s",
        auth_scheme.blueprint_id,
        auth_scheme.agent_identity_id)
      
      return agent_identity_token_response["access_token"]
    

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext,
  ) -> AuthCredential:
    """

    Args:
      auth_config: The authentication configuration.
      context: Optional context for the callback.

    Returns:
      An AuthCredential instance.

    Raises:
      ValueError: If auth_scheme is not a FakeAuthProviderScheme or 
        Gemini Enterprise did not provide a token.
    """

    auth_scheme = auth_config.auth_scheme
    if isinstance(auth_scheme, AgentIdentityOnBehalfOfUserScheme):
      # Request a token for the user

      user_credential_manager = CredentialManager(
        AuthConfig(
          auth_scheme=auth_scheme.user_auth_scheme
        )
      )
      user_credential = await user_credential_manager.get_auth_credential(
        context=context
      )

      if not isinstance(user_credential, AuthCredential) or \
          not user_credential.oauth2 or \
          not user_credential.oauth2.access_token:
        raise ValueError(
          f"Unsupported user credential scheme {type(user_credential)}"
        )

      # Request a token for the agent identity
      agent_identity_access_token = self.get_agent_identity_exchange_token(auth_scheme)

      agent_identity_app = ConfidentialClientApplication(
        authority=f"https://login.microsoftonline.com/{auth_scheme.tenant_id}",
        client_id=auth_scheme.agent_identity_id,
        client_credential={
          "client_assertion" : agent_identity_access_token
        }
      )

      # Perform OBO token exchange.
      obo_token_response = agent_identity_app.acquire_token_on_behalf_of(
        user_assertion=user_credential.oauth2.access_token,
        scopes={auth_scheme.scope}
      )

      if not obo_token_response:
        raise ValueError(
            f"On-behalf-of token exchange failed:"
        )
      elif "access_token" not in obo_token_response:
        error = obo_token_response["error"]
        error_description = obo_token_response["error_description"]
        raise ValueError(
            f"On-behalf-of token exchange: {error_description} ({error})"
        )
      else:
        obo_access_token = obo_token_response["access_token"]
        
        logger.debug(
          "Obtained OBO token for scope %s and agent identity %s",
          auth_scheme.scope,
          auth_scheme.agent_identity_id)

        # Wrap OBO token as an AuthCredential.
        return AuthCredential(
            auth_type=AuthCredentialTypes.OAUTH2,
            oauth2=OAuth2Auth(
                access_token=obo_access_token
            ),
        )
  
    else:
      raise ValueError(
          f"Unsupported auth scheme {type(auth_scheme)}"
      )

