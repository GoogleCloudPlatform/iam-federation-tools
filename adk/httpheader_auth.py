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

from contextvars import ContextVar
import inspect
import logging
import os
from typing import Literal
from typing import Optional

from google.adk.agents.callback_context import CallbackContext
from google.adk.auth.auth_credential import AuthCredential
from google.adk.auth.auth_credential import AuthCredentialTypes
from google.adk.auth.auth_credential import OAuth2Auth
from google.adk.auth.auth_schemes import CustomAuthScheme
from google.adk.auth.auth_tool import AuthConfig
from google.adk.auth.base_auth_provider import BaseAuthProvider
from pydantic import Field
from starlette.applications import Starlette
from starlette.datastructures import Headers
from typing_extensions import override

logger = logging.getLogger("google_solutions." + __name__)

# ------------------------------------------------------------------------------
# Inbound HTTP Header Capture
# ------------------------------------------------------------------------------

# Note: Capturing HTTP headers requiers a Scarlette patch because the ADK doesn't
# expose them directly.

_http_headers: ContextVar[Optional[Headers]] = ContextVar(
    "http_headers", default=None
)

# Use patch for subsequent requests.
_original_starlette_call = Starlette.__call__

async def _patched_starlette_call(self, scope, receive, send):
  if scope.get("type") == "http":
    token = _http_headers.set(Headers(scope=scope))
    try:
      return await _original_starlette_call(self, scope, receive, send)
    finally:
      _http_headers.reset(token)
  return await _original_starlette_call(self, scope, receive, send)

Starlette.__call__ = _patched_starlette_call

# Inspect stack frame for initial request.
_frame = inspect.currentframe()
while _frame:
  _scope = _frame.f_locals.get("scope")
  if isinstance(_scope, dict) and _scope.get("type") == "http":
    _http_headers.set(Headers(scope=_scope))
    break
  _frame = _frame.f_back
del _frame

# ------------------------------------------------------------------------------
# Auth Scheme & Provider
# ------------------------------------------------------------------------------

class HttpHeaderAuthProviderScheme(CustomAuthScheme):
  """Authentication scheme that extracts credentials from an HTTP header.

  This scheme extracts a token from an incoming HTTP request header
  (such as Authorization) and exposes it as an AuthCredential that can
  be used for purposes such as authenticating tool or MCP calls.

  Attributes:
    name: The name of the HTTP header to extract (default: "Authorization").
    scheme: The HTTP authentication scheme (default: "Bearer").
    type_: The type of the security scheme, always "HttpHeaderAuthProviderScheme".
  """

  type_: Literal["HttpHeaderAuthProviderScheme"] = Field(
      default="HttpHeaderAuthProviderScheme", alias="type"
  )
  name: str = "Authorization"
  scheme: str = "Bearer"


class HttpHeaderAuthProvider(BaseAuthProvider):
  """Auth provider that extracts credentials from an incoming HTTP header."""

  @property
  @override
  def supported_auth_schemes(
      self,
  ) -> tuple[type[HttpHeaderAuthProviderScheme], ...]:
    return (HttpHeaderAuthProviderScheme,)

  @override
  async def get_auth_credential(
      self,
      auth_config: AuthConfig,
      context: CallbackContext | None,
  ) -> AuthCredential:
    """Retrieves credentials from the configured HTTP request header.

    Args:
      auth_config: The authentication configuration.
      context: Optional context for the callback.

    Returns:
      An AuthCredential instance.

    Raises:
      ValueError: If auth_scheme is not a HttpHeaderAuthProviderScheme or
        the configured HTTP header is missing or invalid.
    """
    auth_scheme = auth_config.auth_scheme
    if not isinstance(auth_scheme, HttpHeaderAuthProviderScheme):
      raise ValueError(
          f"Expected HttpHeaderAuthProviderScheme, got {type(auth_scheme)}"
      )

    headers = _http_headers.get()
    if headers is None:
      raise ValueError("No HTTP request headers available in current context.")

    header_value = headers.get(auth_scheme.name)
    if not header_value:
      raise ValueError(
          f"No '{auth_scheme.name}' HTTP header found in request."
      )

    if auth_scheme.scheme:
      prefix = f"{auth_scheme.scheme} "
      if not header_value.lower().startswith(prefix.lower()):
        raise ValueError(
            f"Expected '{auth_scheme.scheme}' scheme in '{auth_scheme.name}' header."
        )
      token = header_value[len(prefix) :].strip()
    else:
      token = header_value.strip()

    # Wrap token as an AuthCredential.
    return AuthCredential(
        auth_type=AuthCredentialTypes.OAUTH2,
        oauth2=OAuth2Auth(
            access_token=token
        ),
    )