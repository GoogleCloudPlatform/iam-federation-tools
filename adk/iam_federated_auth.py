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

from pydantic import Field, computed_field
from typing_extensions import override

logger = logging.getLogger('google_solutions.' + __name__)

class IamFederatedAuthScheme(CustomAuthScheme):
  """Authentication scheme for using IAM federation.

  Attributes:
    source_scheme: Scheme to use for obtaining subject token.
    pool_id: Workforce pool ID.
    provider_id: Workforce pool provider ID.
    scope: Google Cloud OAuth scope to request.
    type_: The type of the security scheme, always "IamFederatedAuthScheme".
  """

  type_: Literal["IamFederatedAuthScheme"] = Field(
      default="IamFederatedAuthScheme", alias="type"
  )
  source_scheme: AuthScheme
  scope: Optional[str] = "https://www.googleapis.com/auth/cloud-platform"

  @computed_field
  @property
  def audience(self) -> str:
    raise NotImplementedError()

class WorkforceIdentityScheme(IamFederatedAuthScheme):
  """Authentication scheme for using workforce identity federation.

  Attributes:
    pool_id: Workforce identity pool ID.
    provider_id: Workforce identity pool provider ID.
    type_: The type of the security scheme, always "WorkforceIdentityScheme".
  """

  type_: Literal["WorkforceIdentityScheme"] = Field(
      default="WorkforceIdentityScheme", alias="type"
  )
  pool_id: str
  provider_id: str

  @computed_field
  @property
  def audience(self) -> str:
      return f"//iam.googleapis.com/locations/global/workforcePools/{self.pool_id}/providers/{self.provider_id}"

class WorkloadIdentityScheme(IamFederatedAuthScheme):
  """Authentication scheme for using workload identity federation.

  Attributes:
    project_number: Project that contains the workload identity pool.
    pool_id: Workload identity pool ID.
    provider_id: Workload identity pool provider ID.
    type_: The type of the security scheme, always "WorkloadIdentityScheme".
  """

  type_: Literal["WorkloadIdentityScheme"] = Field(
      default="WorkloadIdentityScheme", alias="type"
  )
  project_number: str
  pool_id: str
  provider_id: str

  @computed_field
  @property
  def audience(self) -> str:
      return f"//iam.googleapis.com/projects/{self.project_number}/locations/global/workloadIdentityPools/{self.pool_id}/providers/{self.provider_id}"

class IamFederatedAuthProvider(BaseAuthProvider):
  """Auth provider for workforce identity federation.
  
    The provider uses 'source_scheme' from the scheme to obtain
    a user credential and exchanges it for an STS access token.
  """

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[WorkforceIdentityScheme], type[WorkloadIdentityScheme]]:
    return (WorkforceIdentityScheme, WorkloadIdentityScheme)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext,
  ) -> AuthCredential:
    """Exchange user credentials for an STS access token.

    Args:
      auth_config: The authentication configuration.
      context: Optional context for the callback.

    Returns:
      An AuthCredential instance that contains an STS access token.

    Raises:
      ValueError: If auth_scheme is not a IamFederatedAuthScheme or 
        obtaining a user credential failed.
    """
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, IamFederatedAuthScheme):
      raise ValueError(
          f"Expected IamFederatedAuthScheme, got {type(auth_scheme)}"
      )

    # Request source credential
    source_credential_manager = CredentialManager(
      AuthConfig(
        auth_scheme=auth_scheme.source_scheme
      )
    )
    source_credential = await source_credential_manager.get_auth_credential(
      context=context
    )

    if not isinstance(source_credential, AuthCredential) or \
        not source_credential.oauth2 or \
        not source_credential.oauth2.access_token:
      raise ValueError(
        f"Unsupported source credential scheme {type(source_credential)}"
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
        "audience": auth_scheme.audience,
        "scope": auth_scheme.scope,
        "requestedTokenType": "urn:ietf:params:oauth:token-type:access_token",
        "subjectToken": source_credential.oauth2.access_token,
        "subjectTokenType" :"urn:ietf:params:oauth:token-type:jwt",
      })

    if response.status != 200:
      raise ValueError(f"The STS token exchange failed: {response.status}: {response.data.decode('utf-8')}")

    # Wrap token as an AuthCredential.
    token_data = json.loads(response.data.decode("utf-8"))
    access_token = token_data.get("access_token")

    if not access_token:
      raise ValueError("The STS token exchange response did not contain an access_token")
    
    return AuthCredential(
        auth_type=AuthCredentialTypes.OAUTH2,
        oauth2=OAuth2Auth(
            access_token=access_token
        ),
    )