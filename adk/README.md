## ADK Authentication Providers

This folder contains authentication providers for ADK agents:

*   [`GeminiEnterpriseDelegatedAuthProvider`](geminienterprise_auth.py) is an
    ADK authentication provider that lets agents use credentials forwarded by
    Gemini Enterprise and act on the end user's behalf.

    For more information about this provider and how to use it, see 
    [Gemini Enterprise delegated authorization](https://googlecloudplatform.github.io/iam-federation-tools/adk-geminienterprise/).

*   [`AzureServiceAuthProvider`](azure_auth.py) is an ADK authentication provider 
    that lets agents use workload identity federation to authenticate to Microsoft Azure. 

    For more information about this provider and how to use it, see 
    [Azure workload identity federation](https://googlecloudplatform.github.io/iam-federation-tools/adk-azure/).