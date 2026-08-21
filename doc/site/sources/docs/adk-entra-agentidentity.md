# Microsoft Entra Agent ID on-behalf-of authorization

This article describes how you can use Microsoft Entra Agent ID to manage
access for an ADK agent, and let ADK agents access Google Cloud resources
and other resources on behalf of Entra users.

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform
*   [ ] Users access the agent by using the Gemini Enterprise web app.
*   [ ] Users authenticate to Gemini Enterprise and other Google Cloud
        resources by using [workforce identity federation :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/workforce-identity-federation)

## Combining Entra Agent ID and Google Cloud agent identity

Despite their similar names, Entra Agent ID and Google Cloud agent identity
focus on different aspects of managing identity and access for agents:

*   Google Cloud agent identity focuses on workload identity: Agent identity
    assigns a unique identity to an agent and ties this identity to the 
    lifecycle of the resource. Agents with an assigned agent identity can 
    obtain SPIFFE-compliant credentials to access Google Cloud resources on
    their own behalf.
*   Entra Agent ID focuses on user-to-agent access, delegation, and governance: 
    Agent ID lets agents authenticate users and access resources on their behalf.
    Administrators can use Agent ID to assign owners and sponsors to agents, 
    track their lifecycle, and control access.
    
For agents deployed on Google Cloud, the two services can complement each other
in the following way:

*   Agents use Entra Agent ID to authenticate users and obtain tokens that
    let the agents act on behalf of users. This may include accessing
    Google Cloud resources such as BigQuery on behalf of the user.
*   To authenticate to Entra Agent ID, agents use their Google Cloud agent 
    identity, in a similar way to how agents deployed on Azure would use 
    [Azure managed identity :octicons-link-external-16:](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/w).

## Authentication process

Users don't interact with ADK agents directly. Instead, they interact with agents
through a frontend application such as the Gemini Enterprise web app. In the context
of Entra Agent ID, Gemini Enterprise 
[takes the role of the _client_ :octicons-link-external-16:](https://learn.microsoft.com/en-us/entra/agent-id/agent-on-behalf-of-oauth-flow).

To let agents act on behalf of users, the frontend applications must authenticate
the user, obtain an Entra access token for them, and forward that token to the 
agent. 

The agent, before being able to act on behalf of the user, must authenticate to Entra itself.
To do that, it uses its Google Cloud agent identity to obtain an 
[ID token](https://docs.cloud.google.com/docs/authentication/token-types#agent-identity-id-tokens)
and uses that as a federated credential.

To access a resource on behalf of the user, the agent can now initiate 
an _on-behalf-of_ flow. The result of this flow is an access token that
identifies the user, grants access to a specific resource, and conveys the
information that the agent acts as a middleman to facilitate the access.

## Set up an Entra Agent ID

This section describes how to set up an Entra Agent ID blueprint and 
agent identity for an ADK agent.

### Create a blueprint

Create an Agent ID blueprint by doing the following:

1.  In Entra, follow the instructions to
    [create an agent identity blueprint using code](https://learn.microsoft.com/en-us/entra/agent-id/create-blueprint?tabs=powershell#create-programmatically).

    In the output of the PowerShell command, find the attribute `id`, which is the
    blueprint App ID. You need this ID in a later step.

    !!! note

        Use PowerShell or the Graph API to create the blueprint. If you create the blueprint
        using the Entra admin center, you might not be able to add an App ID URI and scope later.

1.  In PowerShell, use the blueprint App ID to initialize a variable:

    ```
    $BlueprintAppId = `BLUEPRINT_ID`
    ```

    Replace `BLUEPRINT_ID` with the blueprint App ID.

1.  Assign an App ID URI and scope to the blueprint:

    ```
    $PermissionScope = @{
        adminConsentDescription = "Allow the application to access the agent on behalf of the signed-in user."
        adminConsentDisplayName = "Access agent"
        id = $($BlueprintAppId)
        isEnabled = $true
        type = "User"
        value = "access_agent"
    }

    Update-MgApplication -ApplicationId $BlueprintAppId `
        -IdentifierUris @("api://$($BlueprintAppId)") `
        -Api @{ oauth2PermissionScopes = @($PermissionScope) }
    ```

1.  In the Entra admin portal, use the search function to search for the blueprint App ID.
1.  On the **Overview** page of the blueprint, click **Create blueprint principal**.
1.  Go to **Credentials > Federated credentials**.
1.  Click **Add credential** and configure a federated credential as follows:

    +   **Scenario**: **Other issuer**
    +   **Issuer**: 
    
        ```
        https://sts.googleapis.com/v1/organizations/ORG_ID/locations/global/workloadIdentityPools/agents.global.org-ORG_ID.system.id.goog
        ```
    
        Replace `ORG_ID` with the organization ID of the Google Cloud organization that contains the agent.
        
    +   **Type**: **Explicit subject identifier**.
    +   **Value**: 
    
        ```
        spiffe://agents.global.org-ORG_ID.system.id.goog/resources/aiplatform/RESOURCE_NAME
        ```
    
        Replace the following:
        
        +   `ORG_ID`: the organization ID of the Google Cloud organization that contains the agent
        +   `RESOURCE_NAME`: the resource name of the Agent Runtime deployment as shown in the 
            Cloud Console under **Agent Platform > Agents > Deployments**. 
        
            The resource name looks similar to `projects/1234567890/locations/asia-southeast1/reasoningEngines/5678901234567890`.        

    +   **Name**: `AgentIdentity`.
    +   **Audience**: Keep `api://AzureADTokenExchange` or enter a custom audience.

1.  Click **Add**.

### Create an agent identity

Use the Agent ID blueprint to create an agent identity by doing the following:

1.  Go to **Agents > Agent identities**.
1.  Click **New agent identity** and use the following settings:

    +   **Agent blueprint**: Select the blueprint that you just created.
    +   **Agent identity name**: Specifiy a name that matches or resembles the name of 
        your agent on Agent Platform.

1.  Click **Create**, and then **Go to agent identity**.
1.  On the overview page, find the **Object ID**, which is the ID of the agent identity. 
    You need this ID in a later step.


## Set up Gemini Enterprise

If you use Gemini Enterprise as frontend application, set up authentication 
by doing the following:

1.  In Entra, create an app registration for Gemini Enterprise.

    `https://vertexaisearch.cloud.google.com/oauth-redirect` (Type: Web)

1.  Go to **Authentication** and add a the following redirect URIs:

    +   `https://vertexaisearch.cloud.google.com/oauth-redirect` (Type: Web)
    +   `https://vertexaisearch.cloud.google.com/static/oauth/oauth.html` (Type: Web)

1.  Go to **Certificates & secrets > Client secrets** and add a client secret.

1.  In Gemini Enterprise, follow the instructions in 
    [Register and manage ADK agents hosted on Agent Runtime :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-adk-agent#register_adk_agent-console) 
    and configure authorization using the following settings:

    +   **Authorization name**: a name of your choice
    +   **Client ID**: Client ID of the application registration for Gemini Enterprise
    +   **Client secret**: Client secret of the application registration for Gemini Enterprise
    +   **Token URI**:

        ```
        https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/token
        ```

        Replace `TENANT_ID` with your Entra tenant ID.

    +   **Authorization URI**:

        ```
        https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/authorize?response_type=code&client_id=CLIENT_ID&scope=offline_access%20api://BLUEPRINT_APP_ID/access_agent
        ```

        Replace the following:

        +   `TENANT_ID`: your Entra tenant ID.
        +   `CLIENT_ID`: the client ID of the application registration for Gemini Enterprise
        +   `BLUEPRINT_APP_ID`: the blueprint App ID.

## Enable on-behalf-of access for workforce identity federation

You can let agents access Google Cloud resources on behalf of an Entra user by
using workforce identity federation, but it might require some modifications
to your exising workforce identity federation setup.

### Configure access tokens claims

When users authenticate to Google Cloud by using workforce identity federation,
they typically do so by using a browser-based authentication flow. 
This authentication flow uses ID tokens, and workforce identity
federation evaluates these ID tokens based on the configured attribute mapping.

Agents can also use workforce identity federation to access Google Cloud
on behalf of a user, but with two key differences:

+   Agents can't use the browser-based authentication flow. Instead, they must
    use the [Google STS :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/reference/sts/rest/v1/TopLevel/token) endpoint to perform a token exchange programatically.
+   Agents can't use an ID token for the token exchange because they don't
    possess the user's ID token and can't request ID tokens on behalf of the user.
    Instead, they must use an on-behalf-of access token for the Google STS 
    token exchange.

For workforce identity federation, the difference between Entra ID tokens and 
access tokens is immaterial as long as they match the configured 
attribute condition and mappings. 

To make access tokens compatible with workforce identity federation,
do the following:

1.  In Entra, open the app registration that you use for workforce 
    identity federation. and go to **Manifest**.
1.  In the mannifest, find the field
    [`requestedAccessTokenVersion`](https://learn.microsoft.com/en-us/entra/identity-platform/reference-microsoft-graph-app-manifest#:~:text=require%20user%20consent.-,requestedAccessTokenVersion,-Int32)
    and change it from `1` to `2`.

    This change causes Entra to issue access tokens using the newer `v2.0`
    format.

1.  Click **Save**.
1.  Go to **Token configuration**.
1.  Check of there are any optional claims configured for ID tokens
    that aren't configured for access tokens. If there are, add the 
    same claims to access tokens so that the configuration for ID tokens
    and access tokens is eqiuivalent.

### Add a scope

To enable agents to request suitable access tokens on behalf of the user,
add an App ID URI and scope:

1.  In Entra, open the app registration that you use for workforce 
    identity federation. and go to **Expose an API**.
1.  Next to **Application ID URI**, click **Add**.
1.  Keep the suggested Application ID URI and click **Save**.
1.  Click **Add a scope** and configure the following settings:

    +   **Scope name**: `GoogleCloud`.
    +   **Who can consent**: **Admins only**.

1.  Provide a description and click **Add scope**.

### Request admin consent for all users

To allow the agent identity to request on-behalf-of access tokens for the scope that
you just created, you must provide admin consent. To do this, open
the following URL:

```
https://login.microsoftonline.com/TENANT_ID/v2.0/adminconsent?client_id=AGENT_IDENTITY_ID&scope=SCOPE
```

Replace the following:

+   `TENANT_ID`: your Entra tenant ID.
+   `AGENT_IDENTITY_ID`: the object ID of the Entra agent identity.
+   `SCOPE`: the scope, which looks similar to `api://aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee/GoogleCloud`.


## Use Entra Agent ID in the ADK

To let your ADK agent use Entra Agent ID on-behalf-of authorization, you use
a combination of 3 custom auth provider:

+   [`GeminiEnterpriseDelegatedAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/geminienterprise_auth.py) 
    to authenticate the user.
+   [`AgentIdentityAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/entra_auth.py) 
    to interact with Entra agent identity.
+   [`WorkforceIdentityFederatedAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/workforceidentity_auth.py)
    to obtain temporary Google Cloud credentials.

In your agent's source code, set up the providers as follows:

1.  Register the `GeminiEnterpriseDelegatedAuthProvider` to let your agent access
    the access token that the Gemini Enterprise web app obtains for the user:

    ```
    CredentialManager.register_auth_provider(GeminiEnterpriseDelegatedAuthProvider())
    ge_auth_scheme=GeminiEnterpriseDelegatedAuthProviderScheme()
    ```

1.  Register the `AgentIdentityAuthProvider` and initialize an instance of
    `AgentIdentityOnBehalfOfUserScheme`:

    ```
    CredentialManager.register_auth_provider(AgentIdentityAuthProvider())
    msal_obo_scheme = AgentIdentityOnBehalfOfUserScheme(
        tenant_id="TENANT_ID",
        blueprint_id="BLUEPRINT_APP_ID",
        agent_identity_id="AGENT_IDENTITY_ID",
        scope="api://WFIF_APP_ID/GoogleCloud",
        user_auth_scheme=ge_auth_scheme
    ```

    Replace the following:

    +   `TENANT_ID`: your Entra tenant ID.
    +   `BLUEPRINT_APP_ID`: the Entra blueprint App ID.
    +   `AGENT_IDENTITY_ID`: the object ID of the Entra agent identity.
    +   `WFIF_APP_ID`: the ID of the app registration used for workforce identity federation.

    The scheme configures the provider to obtain a credential using the
    `GeminiEnterpriseDelegatedAuthProviderScheme` and use it to 
    request an on-behalf-of token for the app registration used for 
    workforce identity federation.

1.  Register the `WorkforceIdentityFederatedAuthProvider` and initialize an instance of
    `WorkforceIdentityFederatedAuthProviderScheme`:

    ```
    CredentialManager.register_auth_provider(WorkforceIdentityFederatedAuthProvider())
    wfif_scheme = WorkforceIdentityFederatedAuthProviderScheme(
        pool_id="POOL_ID",
        provider_id="PROVIDER_ID",
        user_auth_scheme=msal_obo_scheme,
    )
    ```

    Replace the following:

    +   `POOL_ID`: the workforce identity pool ID.
    +   `PROVIDER_ID`: the workforce identity provider ID.

    The scheme configures the provider to obtain a credential using the
    `AgentIdentityOnBehalfOfUserScheme` and perform an STS token exchange
    to obtain temporary Google Cloud credentials.

1.  Pass the `WorkforceIdentityFederatedAuthProviderScheme` to the 
    constructor of relevant MCP tool sets or use it to initialize an `AuthConfig`.

1.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

