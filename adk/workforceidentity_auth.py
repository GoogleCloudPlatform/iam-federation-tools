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
import json

from pydantic import Field
from typing_extensions import override

logger = logging.getLogger('google_solutions.' + __name__)

class WorkforceIdentityFederatedAuthProviderScheme(CustomAuthScheme):
  """Authentication scheme for Workforce identity-federated authentication.

  Attributes:
    user_auth_scheme: Scheme to authenticate the user with.
    pool_id: Workforce pool ID.
    provider_id: Workforce pool provider ID.
    scope: Google Cloud OAuth scope to request.
    type_: The type of the security scheme, always "WorkforceIdentityFederatedAuthProviderScheme".
  """

  type_: Literal["WorkforceIdentityFederatedAuthProviderScheme"] = Field(
      default="WorkforceIdentityFederatedAuthProviderScheme", alias="type"
  )
  user_auth_scheme: AuthScheme
  pool_id: str
  provider_id: str
  scope: Optional[str] = "https://www.googleapis.com/auth/cloud-platform"

class WorkforceIdentityFederatedAuthProvider(BaseAuthProvider):
  """Auth provider for  Workforce identity-federated authentication.."""

  def __init__(self):
    # When GOOGLE_API_USE_CLIENT_CERTIFICATE is true, MCPSessionManager
    # ignores custom authentication and forces connection to use ADC
    # instead.
    
    if (os.environ.get('GOOGLE_API_USE_CLIENT_CERTIFICATE', 'true').lower() == 'true'):
      logger.warning('To use Gemini Enterprise authentication for MCP, ' \
      'the environment variable GOOGLE_API_USE_CLIENT_CERTIFICATE must be set to false')

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[WorkforceIdentityFederatedAuthProviderScheme], ...]:
    return (WorkforceIdentityFederatedAuthProviderScheme,)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext,
  ) -> AuthCredential:
    """Exchanged user credentials for temporary workforce identity credentials.

    Args:
      auth_config: The authentication configuration.
      context: Optional context for the callback.

    Returns:
      An AuthCredential instance.

    Raises:
      ValueError: If auth_scheme is not a WorkforceIdentityFederatedAuthProviderScheme or 
        Gemini Enterprise did not provide a token.
    """
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, WorkforceIdentityFederatedAuthProviderScheme):
      raise ValueError(
          f"Expected WorkforceIdentityFederatedAuthProviderScheme, got {type(auth_scheme)}"
      )

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

    # Exchange token
    request = Request()
    response = request(
      url="https://sts.googleapis.com/v1/token", 
      method="POST", 
      headers={
        "Content-Type": "application/x-www-form-urlencoded"
      }, 
      body={
        "grantType": "urn:ietf:params:oauth:grant-type:token-exchange",
        "audience": f"//iam.googleapis.com/locations/global/workforcePools/{auth_scheme.pool_id}/providers/{auth_scheme.provider_id}",
        "scope": auth_scheme.scope,
        "requestedTokenType": "urn:ietf:params:oauth:token-type:access_token",
        "subjectToken": user_credential.oauth2.access_token,
        "subjectTokenType" :"urn:ietf:params:oauth:token-type:jwt",
      })

    if response.status != 200:
      raise ValueError(f"STS token exchange failed: {response.status}: {response.data.decode('utf-8')}")

    # Wrap token as an AuthCredential.
    return AuthCredential(
        auth_type=AuthCredentialTypes.OAUTH2,
        oauth2=OAuth2Auth(
            access_token=json.loads(response.data).get("access_token")
        ),
    )
