# ADK Authentication Providers

This folder contains example authentication providers for ADK agents.

## User authentication

The following authentication providers let ADK agents authenticate users
and act on their behalf:

*   [`GeminiEnterpriseDelegatedAuthProvider`](geminienterprise_auth.py) is an
    ADK authentication provider that lets agents use credentials forwarded by
    Gemini Enterprise and act on the end user's behalf.

    For more information about this provider and how to use it, see 
    [Gemini Enterprise delegated authorization (ADK)](https://googlecloudplatform.github.io/iam-federation-tools/adk-geminienterprise/).

*   [`HttpHeaderAuthProvider`](httpheader_auth.py) is an ADK authentication provider
    that lets agents extract credentials from an incoming HTTP request header
    (such as `Authorization`) and act on the end user's behalf.

    For more information about this provider and how to use it, see
    [Gemini Enterprise delegated authorization (A2A)](https://googlecloudplatform.github.io/iam-federation-tools/adk-geminienterprise-a2a/).

*   [`AgentIdentityAuthProvider`](entra_auth.py) is an ADK authentication provider
    that lets agents use Microsoft Entra Agent ID to authenticate users and
    obtain on-behalf-of access tokens.

    For more information about this provider and how to use it, see
    [Microsoft Entra Agent ID on-behalf-of authorization](https://googlecloudplatform.github.io/iam-federation-tools/adk-entra-agentidentity-obo/).

*   [`IamFederatedAuthProvider`](iam_federated_auth.py) (and
    [`WorkforceIdentityFederatedAuthProvider`](workforceidentity_auth.py)) is an
    ADK authentication provider that lets agents exchange external user
    credentials for short-lived Google Cloud credentials using workforce
    identity federation.

    For more information about this provider and how to use it, see
    [Microsoft Entra Agent ID on-behalf-of authorization](https://googlecloudplatform.github.io/iam-federation-tools/adk-entra-agentidentity-obo/).

## Service authentication

The following authentication providers let ADK agents authenticate to APIs
and MCP servers on their own behalf:

*   [`CloudRunServiceAuthProvider`](cloudrun_auth.py) is an ADK authentication provider 
    that lets agents authenticate to APIs and MCP servers that run on Cloud Run and 
    require service-to-service authentication.

    For more information about this provider and how to use it, see 
    [Cloud Run service authentication](https://googlecloudplatform.github.io/iam-federation-tools/adk-cloudrun/).

*   [`IapServiceAuthProvider`](iap_auth.py) is an ADK authentication provider
    that lets agents authenticate to APIs and MCP servers that are protected by
    Identity-Aware-Proxy (IAP).

    For more information about this provider and how to use it, see
    [IAP service authentication](https://googlecloudplatform.github.io/iam-federation-tools/adk-iap/).
    
*   [`AzureFederatedAuthProvider`](azure_auth.py) is an ADK authentication provider 
    that lets agents use workload identity federation to authenticate to Microsoft Azure. 

    For more information about this provider and how to use it, see 
    [Azure workload identity federation](https://googlecloudplatform.github.io/iam-federation-tools/adk-azure/).
    
*   [`AwsFederatedAuthProvider`](aws_auth.py) is an ADK authentication provider 
    that lets agents assume an AWS role and call AWS APIs and MCP tools that use SigV4 authentication. 

    For more information about this provider and how to use it, see 
    [AWS federation](https://googlecloudplatform.github.io/iam-federation-tools/adk-aws/).

*   [`AgentIdentityAuthProvider`](entra_auth.py) is an ADK authentication provider
    that lets agents use Microsoft Entra Agent ID to obtain Entra access tokens
    on their own behalf.

    For more information about this provider and how to use it, see
    [Microsoft Entra Agent ID](https://googlecloudplatform.github.io/iam-federation-tools/adk-entra-agentidentity/).

*   [`IamFederatedAuthProvider`](iam_federated_auth.py) is an ADK authentication
    provider that lets agents exchange external credentials for short-lived
    Google Cloud credentials using workload identity federation.

    For more information about this provider and how to use it, see
    [Microsoft Entra Agent ID](https://googlecloudplatform.github.io/iam-federation-tools/adk-entra-agentidentity/).
