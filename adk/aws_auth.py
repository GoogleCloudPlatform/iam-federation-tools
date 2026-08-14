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
import httpx

from typing import Any, Dict, List, Literal, Optional
from typing_extensions import override
from pydantic import Field, ConfigDict

import boto3
from botocore.auth import SigV4Auth
from botocore.awsrequest import AWSRequest
from botocore.credentials import DeferredRefreshableCredentials
from botocore.session import get_session

import contextvars
from google.adk.auth.auth_credential import AuthCredential, AuthCredentialTypes
from google.adk.auth.auth_schemes import CustomAuthScheme
from google.adk.auth.auth_tool import AuthConfig
from google.adk.auth.base_auth_provider import BaseAuthProvider
from google.adk.agents.callback_context import CallbackContext
from google.adk.agents.readonly_context import ReadonlyContext
from google.adk.tools.mcp_tool import McpToolset, StreamableHTTPConnectionParams

from google.auth.transport.requests import Request
from google.oauth2 import id_token

logger = logging.getLogger('google_solutions.' + __name__)

class AwsCredential(AuthCredential):
  """ADK AuthCredential that wraps a boto3 session.
  
  Attributes:
    session: The underlying boto3 session
  """
  model_config = ConfigDict(arbitrary_types_allowed=True)

  # The underlying boto3 session
  session: boto3.Session
  
  @classmethod
  def from_session(
    cls, 
    session: boto3.Session
  ) -> AwsCredential:
    return cls(
      auth_type=AuthCredentialTypes.HTTP,
      session=session,
    )

class AwsSigV4Scheme(CustomAuthScheme):
  """Authentication scheme for federated authentication to AWS.
  
  Attributes:
    type_: The type of the security scheme, always "AwsSigV4Scheme".
    role_arn: ARN of the role to assume
    role_session_name: Name of the role session
    duration (Optional): Duration of session, in seconds
    audience (Optional): Expected audience (accounts.google.com:oaud)
      in trust policy
  """

  type_: Literal["AwsSigV4Scheme"] = Field(
      default="AwsSigV4Scheme", alias="type"
  )
  role_arn: str
  role_session_name: str
  duration: int = Field(default=1200, ge=900)
  audience: Optional[str] = "https://sts.amazonaws.com"

class AwsFederatedAuthProvider(BaseAuthProvider):
  """Auth provider for federated authentication to AWS.

  The provider obtains an ID token/JWT-SVID from the agent's
  metadata server and uses it to call AssumeRoleWithWebIdentity
  and obtain temporary AWS credentials.

  Credentials are obtained lazily on first access.
  """

  def __init__(self, region_name: str):
    self.region_name = region_name
    self.sts_client = boto3.client("sts", region_name=region_name)

    # When GOOGLE_API_USE_CLIENT_CERTIFICATE is true, MCPSessionManager
    # ignores custom authentication and forces connection to use ADC
    # instead.
    
    if (os.environ.get('GOOGLE_API_USE_CLIENT_CERTIFICATE', 'true').lower() == 'true'):
      logger.warning('To use AWS authentication, ' \
      'the environment variable GOOGLE_API_USE_CLIENT_CERTIFICATE must be set to false')

  @property
  @override
  def supported_auth_schemes(self) -> tuple[type[AwsSigV4Scheme], ...]:
    return (AwsSigV4Scheme,)

  def _refresh(self, auth_scheme: AwsSigV4Scheme) -> Dict[str, Any]:
    """Fetches a fresh OIDC token and calls STS AssumeRoleWithWebIdentity."""
    try:
      token = str(id_token.fetch_id_token(Request(), auth_scheme.audience))

      response = self.sts_client.assume_role_with_web_identity(
          RoleArn=auth_scheme.role_arn,
          RoleSessionName=auth_scheme.role_session_name,
          WebIdentityToken=token,
          DurationSeconds=auth_scheme.duration,
      )

      logger.info("Assumed AWS role %s", auth_scheme.role_arn)

      creds = response["Credentials"]

      return {
          "access_key": creds["AccessKeyId"],
          "secret_key": creds["SecretAccessKey"],
          "token": creds["SessionToken"],
          "expiry_time": creds["Expiration"].isoformat(),
      }
    except Exception as e:
      logger.exception(
          "Assuming AWS role %s failed: %s",
          auth_scheme.role_arn,
          e,
      )
      raise

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext | None,
  ) -> AwsCredential:
    """Performs a token exchange with AWS STS to obtain temporary AWS credentials"""

    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, AwsSigV4Scheme):
      raise ValueError(
          f"Expected AwsSigV4Scheme, got {type(auth_scheme)}"
      )

    refreshable_credentials = DeferredRefreshableCredentials(
        method="sts-assume-role-web-identity",
        refresh_using=lambda: self._refresh(auth_scheme)
    )

    botocore_session = get_session()
    botocore_session._credentials = refreshable_credentials
    return AwsCredential.from_session(
        boto3.Session(
            botocore_session=botocore_session,
            region_name=self.region_name,
        )
    )


class AwsMcpToolset(McpToolset):
  """MCP toolset that uses AWS SigV4 authentication"""

  class AuthHandler(httpx.Auth):
    """ Auth handler that injects SigV4 headers """
    def __init__(self, toolset: AwsMcpToolset):
      self.toolset = toolset

    async def async_auth_flow(self, request: httpx.Request):
      """ Adds AWS SigV4 headers to a HTTP request """

      auth_config = self.toolset.get_auth_config()
      if not auth_config or not auth_config.credential_key:
        raise ValueError("Expected credential_key in auth config")
      
      # Get the context captured in _get_auth_headers and obtain
      # the right credential.

      ctx = self.toolset.active_callback_context.get()
      if not ctx:
        raise ValueError("Expected context")

      credential = ctx.get_credential(auth_config.credential_key)
      if not isinstance(credential, AwsCredential):
        raise ValueError(
            f"Expected AwsCredential, got {type(credential)}"
        )

      # Create signature over request body
      credentials = await asyncio.to_thread(credential.session.get_credentials)
      if not credentials:
        raise ValueError("No AWS credentials found in the session")
      
      signer = SigV4Auth(
        credentials, 
        self.toolset.service_name, 
        self.toolset.region_name)

      await request.aread()
      aws_request = AWSRequest(
        method=request.method,
        url=str(request.url),
        data=request.content,
        headers=dict(request.headers),
      )
      signer.add_auth(aws_request)

      for key, value in aws_request.headers.items():
        request.headers[key] = value

      yield request

  def __init__(
      self, 
      region_name: str,
      service_name: str,
      url: str,
      auth_scheme: AwsSigV4Scheme,
      tool_filter: Optional[List[str]] = None
    ):
    self.region_name = region_name
    self.service_name = service_name
    self.active_callback_context: contextvars.ContextVar[Optional[ReadonlyContext]] = (
        contextvars.ContextVar("active_callback_context", default=None)
    )
    
    connection_params = StreamableHTTPConnectionParams(
      url=url,
      httpx_client_factory=lambda headers=None, timeout=None, auth=None: httpx.AsyncClient(
        headers=headers,
        timeout=timeout,
        auth=AwsMcpToolset.AuthHandler(self),
        follow_redirects=True,
      )
    )
    super().__init__(
      connection_params=connection_params,
      tool_filter=tool_filter,
      auth_scheme=auth_scheme)

  def _get_auth_headers(
      self, readonly_context: Optional[ReadonlyContext] = None
    ) -> Optional[Dict[str, str]]:

    # Usually, the ADK expects us to provide authentication headers
    # here, but in case of SignV4, that's not possble because the
    # headers depend on the request content.
    #
    # Capture the context (which contains the credential) so
    # that we can use it to inject the right headers in 
    # async_auth_flow.

    self.active_callback_context.set(readonly_context)
    return super()._get_auth_headers(readonly_context)
