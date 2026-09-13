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
import time
import json

from typing import Literal
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
from google.cloud import iam_credentials_v1

from pydantic import Field

logger = logging.getLogger('google_solutions.' + __name__)

class IapServiceAuthProviderScheme(CustomAuthScheme):
  """Authentication scheme for authenticating to IAP-protected
  resources.

  This scheme uses a service account JWT for authentication.
  To create the JWT, the scheme uses the `signJwt` method
  of a designated service account.

  For agents that have an attached service account, the service
  account used for signing can (but does not need to) be the 
  same as the attached service account.
  
  Attributes:
    type_: The type of the security scheme, always "IapServiceAuthProviderScheme".
    service_url: URL of the IAP resource  to authenticate to
    service_account: Service account to use for signing the JWT
  """

  type_: Literal["IapServiceAuthProviderScheme"] = Field(
      default="IapServiceAuthProviderScheme", alias="type"
  )
  service_url: str
  service_account: str

class IapServiceAuthProvider(BaseAuthProvider):
  """Authentication provider for service authentication to IAP."""

  def __init__(self):
    # When GOOGLE_API_USE_CLIENT_CERTIFICATE is true, MCPSessionManager
    # ignores custom authentication and forces connection to use ADC
    # instead.
    
    if (os.environ.get('GOOGLE_API_USE_CLIENT_CERTIFICATE', 'true').lower() == 'true'):
      logger.warning('To use IAP service authentication for MCP, ' \
      'the environment variable GOOGLE_API_USE_CLIENT_CERTIFICATE must be set to false')

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[IapServiceAuthProviderScheme], ...]:
    return (IapServiceAuthProviderScheme,)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext | None,
  ) -> AuthCredential:
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, IapServiceAuthProviderScheme):
      raise ValueError(
          f"Expected IapServiceAuthProviderScheme, got {type(auth_scheme)}"
      )

    # Make sure the audience allows sub-paths
    audience = f"{auth_scheme.service_url.rstrip('/*')}/*"

    # Construct JWT payload
    now = int(time.time())
    payload = {
        "iss": auth_scheme.service_account,
        "sub": auth_scheme.service_account,
        "aud": audience,
        "iat": now,
        "exp": now + 3600,
    }

    # Sign the JWT using the designated service account's key
    jwt = await asyncio.to_thread(
        lambda: iam_credentials_v1.IAMCredentialsClient()
        .sign_jwt(
            name=f"projects/-/serviceAccounts/{auth_scheme.service_account}",
            payload=json.dumps(payload),
        )
        .signed_jwt
    )

    # Return token as access token so that it can be used for MCP tool calls
    return AuthCredential(
        auth_type=AuthCredentialTypes.OAUTH2,
        oauth2=OAuth2Auth(
            access_token=jwt
        )
    )
    