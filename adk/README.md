# ADK Authentication Providers

This folder contains example authentication providers for ADK agents.

## User authentication

The following authentication providers let ADK agents authenticate users
and act on their behalf:

*   [`GeminiEnterpriseDelegatedAuthProvider`](geminienterprise_auth.py) is an
    ADK authentication provider that lets agents use credentials forwarded by
    Gemini Enterprise and act on the end user's behalf.

*   [`HttpHeaderAuthProvider`](httpheader_auth.py) is an ADK authentication provider
    that lets agents extract credentials from an incoming HTTP request header
    (such as `Authorization`) and act on the end user's behalf.

*   [`AgentIdentityAuthProvider`](entra_auth.py) is an ADK authentication provider
    that lets agents use Microsoft Entra Agent ID to authenticate users and
    obtain on-behalf-of access tokens.

*   [`WorkforceIdentityFederatedAuthProvider`](workforceidentity_auth.py)) is an
    ADK authentication provider that lets agents exchange external user
    credentials for short-lived Google Cloud credentials using workforce
    identity federation.

## Service authentication

The following authentication providers let ADK agents authenticate to APIs
and MCP servers on their own behalf:

*   [`CloudRunServiceAuthProvider`](cloudrun_auth.py) is an ADK authentication provider 
    that lets agents authenticate to APIs and MCP servers that run on Cloud Run and 
    require service-to-service authentication.

*   [`IapServiceAuthProvider`](iap_auth.py) is an ADK authentication provider
    that lets agents authenticate to APIs and MCP servers that are protected by
    Identity-Aware-Proxy (IAP).
    
*   [`AzureFederatedAuthProvider`](azure_auth.py) is an ADK authentication provider 
    that lets agents use workload identity federation to authenticate to Microsoft Azure. 
    
*   [`AwsFederatedAuthProvider`](aws_auth.py) is an ADK authentication provider 
    that lets agents assume an AWS role and call AWS APIs and MCP tools that use SigV4 authentication. 

*   [`AgentIdentityAuthProvider`](entra_auth.py) is an ADK authentication provider
    that lets agents use Microsoft Entra Agent ID to obtain Entra access tokens
    on their own behalf.

*   [`IamFederatedAuthProvider`](iam_federated_auth.py) is an ADK authentication
    provider that lets agents exchange external credentials for short-lived
    Google Cloud credentials using workload identity federation.

## More information

For more information about these provider and how to use them, see
[ADK authentication providers](https://googlecloudplatform.github.io/iam-federation-tools/adk/).
