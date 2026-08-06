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

from typing import Literal
from typing import Optional
from pydantic import Field, ConfigDict
from typing_extensions import override

import asyncio

from google.adk.agents.callback_context import CallbackContext
from google.adk.auth.auth_credential import AuthCredential
from google.adk.auth.auth_credential import AuthCredentialTypes
from google.adk.auth.auth_credential import OAuth2Auth
from google.adk.auth.auth_tool import AuthConfig
from google.adk.auth.base_auth_provider import BaseAuthProvider
from google.adk.auth.auth_schemes import CustomAuthScheme

from google.auth.transport.requests import Request
from google.oauth2 import id_token
from azure.identity import ClientAssertionCredential

logger = logging.getLogger('google_solutions.' + __name__)

class AzureServiceAuthProviderScheme(CustomAuthScheme):
  """Authentication scheme for federated service authentication to Azure.

  This scheme obtains an ID token/JWT-SVID from the agent's
  metadata server and uses it as a client assertion to
  authenticate to Azure. The scheme exposes the resulting
  Azure access token  as an AuthCredential that can be used 
  for purposes such as authenticating MCP calls.

  Attributes:
    type_: The type of the security scheme, always "AzureServiceAuthProviderScheme".
    tenant_id: ID of the Entra tenant containing the app registration
    client_id: Application/client ID of the app registration
    audience (Optional): Expected audience
    scope (Optional): OAuth2 scope, in format "api://APP-ID/.default
  """

  type_: Literal["AzureServiceAuthProviderScheme"] = Field(
      default="AzureServiceAuthProviderScheme", alias="type"
  )
  tenant_id: str
  client_id: str
  audience: str = "api://AzureADTokenExchange"
  scope: Optional[str] = None

class AzureAuthCredential(AuthCredential):
  """An Azure credential
  
  Attributes:
    oauth2: Contains the Azure access token
    azure_credential: The underlying ClientAssertionCredential
  """

  # Allow non-Pydantic types like ClientAssertionCredential
  model_config = ConfigDict(arbitrary_types_allowed=True)

  # The underlying ClientAssertionCredential
  azure_credential: ClientAssertionCredential

  @classmethod
  def from_client_assertion(
      cls, credential: ClientAssertionCredential, scope: str
  ) -> AzureAuthCredential:
      token_response = credential.get_token(scope)
      return cls(
          auth_type=AuthCredentialTypes.OAUTH2,
          oauth2=OAuth2Auth(access_token=token_response.token),
          azure_credential=credential,
      )

class AzureServiceAuthProvider(BaseAuthProvider):
  """Auth provider for federated authentication to Azure."""

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[AzureServiceAuthProviderScheme], ...]:
    return (AzureServiceAuthProviderScheme,)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext | None,
  ) -> AzureAuthCredential:
    """Performs a token exchange to obtain an Azure access token.

    Args:
      auth_config: The authentication configuration.
      context: Optional context for the callback.

    Returns:
      An AzureAuthCredential instance.

    Raises:
      ValueError: If the token exchange fails.
    """
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, AzureServiceAuthProviderScheme):
      raise ValueError(
          f"Expected AzureServiceAuthProviderScheme, got {type(auth_scheme)}"
      )

    # Initialize Azure credential that uses the agent's ID token as client assertion.
    credential = ClientAssertionCredential(
        tenant_id=auth_scheme.tenant_id,
        client_id=auth_scheme.client_id,
        func=lambda: str(id_token.fetch_id_token(Request(), auth_scheme.audience)))

    # Wrap token as an AuthCredential. Include the underlying credential so that
    # it can be used to initialize Azure client libraries.

    logger.info("Obtaining Azure access token for scope '%s'", auth_scheme.scope)

    try:
        return await asyncio.to_thread(
            AzureAuthCredential.from_client_assertion,
            credential,
            str(auth_scheme.scope or f"api://{auth_scheme.client_id}/.default"),
        )
    except Exception as e:
        raise ValueError(f"Failed to obtain Azure access token: {e}") from e
