# ADK Authentication Providers

This folder contains example authentication providers for ADK agents.

## User authentication

The following authentication providers let ADK agents authenticate users
and act on their behalf:

*   [`GeminiEnterpriseDelegatedAuthProvider`](geminienterprise_auth.py) is an
    ADK authentication provider that lets agents use credentials forwarded by
    Gemini Enterprise and act on the end user's behalf.

    For more information about this provider and how to use it, see 
    [Gemini Enterprise delegated authorization](https://googlecloudplatform.github.io/iam-federation-tools/adk-geminienterprise/).

## Service authentication

The following authentication providers let ADK agents authenticate to APIs
and MCP servers using a service identity:

*   [`CloudRunServiceAuthProvider`](cloudrun_auth.py) is an ADK authentication provider 
    that lets agents authenticate to APIs and MCP servers that run on Cloud Run and 
    require service-to-service authentication.

    For more information about this provider and how to use it, see 
    [Cloud Run service authentication](https://googlecloudplatform.github.io/iam-federation-tools/adk-cloudrun/).
    
*   [`AzureFederatedAuthProvider`](azure_auth.py) is an ADK authentication provider 
    that lets agents use workload identity federation to authenticate to Microsoft Azure. 

    For more information about this provider and how to use it, see 
    [Azure workload identity federation](https://googlecloudplatform.github.io/iam-federation-tools/adk-azure/).
    
*   [`AwsFederatedAuthProvider`](aws_auth.py) is an ADK authentication provider 
    that lets agents assume an AWS role and call AWS APIs and MCP tools that use SigV4 authentication. 

    For more information about this provider and how to use it, see 
    [AWS federation](https://googlecloudplatform.github.io/iam-federation-tools/adk-aws/).