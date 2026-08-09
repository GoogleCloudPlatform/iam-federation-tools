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

import asyncio
import logging
import os

from typing import Literal
from typing import Optional
from typing_extensions import override

from google.adk.agents.callback_context import CallbackContext
from google.adk.auth.auth_credential import AuthCredential
from google.adk.auth.auth_credential import AuthCredentialTypes
from google.adk.auth.auth_credential import OAuth2Auth
from google.adk.auth.auth_tool import AuthConfig
from google.adk.auth.base_auth_provider import BaseAuthProvider
from google.adk.auth.auth_schemes import CustomAuthScheme

import google.auth
import google.auth.impersonated_credentials
from google.oauth2 import id_token
from google.auth.transport.requests import Request

from pydantic import Field

logger = logging.getLogger('google_solutions.' + __name__)

class CloudRunServiceAuthProviderScheme(CustomAuthScheme):
  """Authentication scheme for service authentication to Cloud Run.

  This scheme obtains an ID token that is suitable for 
  authenticating to Cloud Run.

  If the agent is deployed with an attached service account,
  the scheme uses the attached service account's identity.

  If the agent is deployed with an agent identity, the
  scheme uses a designated service account to obtain an ID token.

  Attributes:
    type_: The type of the security scheme, always "CloudRunServiceAuthProviderScheme".
    service_url: URL of the Cloud Run service to authenticate to
    service_account: Service account to use for authentication, only required
        if the agent uses Agent Identity
  """

  type_: Literal["CloudRunServiceAuthProviderScheme"] = Field(
      default="CloudRunServiceAuthProviderScheme", alias="type"
  )
  service_url: str
  service_account: Optional[str] = None

class CloudRunServiceAuthProvider(BaseAuthProvider):
  """Authentication provider for service authentication to Cloud Run."""

  def __init__(self):
    # When GOOGLE_API_USE_CLIENT_CERTIFICATE is true, MCPSessionManager
    # ignores custom authentication and forces connection to use ADC
    # instead.
    
    if (os.environ.get('GOOGLE_API_USE_CLIENT_CERTIFICATE', 'true').lower() == 'true'):
      logger.warning('To use Cloud Run service authentication for MCP, ' \
      'the environment variable GOOGLE_API_USE_CLIENT_CERTIFICATE must be set to false')

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[CloudRunServiceAuthProviderScheme], ...]:
    return (CloudRunServiceAuthProviderScheme,)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext | None,
  ) -> AuthCredential:
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, CloudRunServiceAuthProviderScheme):
      raise ValueError(
          f"Expected CloudRunServiceAuthProviderScheme, got {type(auth_scheme)}"
      )

    if os.environ.get("GOOGLE_API_CERTIFICATE_CONFIG"):
      # Agent uses Agent Identity. The ID token (JWT-SVID) is not suitable for 
      # authenticating to Cloud Run, so we need to impersonate a service account
      # to obtain a suitable ID token.
      
      if not auth_scheme.service_account:
        raise ValueError(
            "CloudRunServiceAuthProviderScheme "
            "requires `service_account` when using Agent Identity"
        )

      source_credentials, _ = await asyncio.to_thread(google.auth.default)
      impersonated_target = google.auth.impersonated_credentials.Credentials(
          source_credentials=source_credentials,
          target_principal=auth_scheme.service_account,
          target_scopes=["https://www.googleapis.com/auth/cloud-platform"],
      )
      id_token_credentials = google.auth.impersonated_credentials.IDTokenCredentials(
          target_credentials=impersonated_target,
          target_audience=auth_scheme.service_url,
          include_email=True,
      )
      await asyncio.to_thread(id_token_credentials.refresh, Request())
      id_token_value = id_token_credentials.token
      
    else:
      # Use ID token from metadata server
      id_token_value = await asyncio.to_thread(
        id_token.fetch_id_token, 
        Request(), 
        auth_scheme.service_url
      )

    # Return ID token as access token so that it can be used for 
    # MCP tool calls
    return AuthCredential(
        auth_type=AuthCredentialTypes.OAUTH2,
        oauth2=OAuth2Auth(
            access_token=id_token_value
        )
    )
    