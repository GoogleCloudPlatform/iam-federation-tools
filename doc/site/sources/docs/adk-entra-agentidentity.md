---
title: Using Microsoft Entra Agent ID for ADK agents
---

# Using Microsoft Entra Agent ID for ADK agents

This article describes how you can use Microsoft Entra Agent ID to let 
ADK agents access resources across Google Cloud and Microsoft Azure on
their own behalf.

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run
*   [ ] Your agent is configured to use agent identity or has an attached service account
*   [ ] You want to use Entra to govern the lifecycle of the agent
*   [ ] You want the agent to access resources across Google Cloud or Microsoft Azure
        on its own behalf

## Combining Entra Agent ID and Google Cloud agent identity

Despite their similar names, Entra Agent ID and Google Cloud agent identity
focus on different aspects of managing identity and access for agents:

*   Google Cloud agent identity focuses on workload identity: Agent identity
    assigns a unique identity to an agent and ties this identity to the 
    lifecycle of the resource. Agents with an assigned agent identity can 
    obtain SPIFFE-compliant credentials to access Google Cloud resources on
    their own behalf.
*   Entra Agent ID focuses on governance, delegation, and access management: 
    Administrators can use Agent ID to assign owners and sponsors to agents, 
    track their lifecycle, and control access.

For agents deployed on Google Cloud, you can combine both services so that
Entra Agent ID governs access across both Microsoft Azure and Google Cloud:

1.  To authenticate to Entra Agent ID, the agent uses its Google Cloud agent
    identity (or attached service account) to obtain an ID token and uses that
    token as a federated credential for its Entra agent identity blueprint.
1.  Using the blueprint, the agent obtains an Entra access token for its
    assigned Entra agent identity.
1.  The agent can then use this Entra access token in two ways:
    +   Use the token directly to access Azure APIs or MCP servers that require
        Entra authentication.
    +   Exchange the token using Google Cloud [workload identity federation :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/workload-identity-federation)
        to obtain short-lived Google Cloud credentials and access Google Cloud
        resources under the governance of its Entra agent identity.

## Set up an Entra Agent ID

Create an Agent ID blueprint and linked agent identity by doing the following:

1.  In the Entra admin portal, go to **Agents > Agent blueprints**.
1.  Click **New agent blueprint** and complete the wizard steps.
1.  Open the blueprint and click **Credentials > Federated credentials**.
1.  Click **Add credential** and configure a federated credential. The configuration
    differs based on whether your agent uses agent identity or an attached service account:

    === "Service account"

        +   **Scenario**: **Other issuer**
        +   **Issuer**: `https://accounts.google.com`
        +   **Type**: **Explicit subject identifier**.
        +   **Value**: ID of the service account. The ID looks similar to the following: `102770123456789012345`.
        +   **Name**: `GoogleServiceAccount`.
        +   **Audience**: Keep `api://AzureADTokenExchange`.

    === "Agent identity"

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
        +   **Audience**: Keep `api://AzureADTokenExchange`.

1.  Click **Add**.
1.  Select **Linked agent identities**.
1.  Open the agent identity that Entra created automatically.
1.  On the overview page, find the **Blueprint App ID** and **Object ID**.
    You need these identifiers in a later step.

## Set up workload identity federation

To let the ADK agent access Google Cloud resources using its assigned
Entra agent identity, you also need to set up workload identity federation:

1.  Follow the instructions in [Configure Workload Identity Federation with AWS or Azure VMs :octicons-link-external-16:](https://docs.cloud.google.com/iam/docs/workload-identity-federation-with-other-clouds#azure)
    to create an Entra app registration, create a workload identity pool in Google Cloud,
    and federate the pool with Entra.

    Note the **Application (client) ID** of the Entra app registration that you
    created for workload identity federation, as you need this ID in the next section.

1.  Grant the Entra agent identity access to the required Google Cloud resources by
    granting IAM roles to the workload identity pool principal:

    ```
    principal://iam.googleapis.com/projects/PROJECT_NUMBER/locations/global/workloadIdentityPools/POOL_ID/subject/AGENT_IDENTITY_ID
    ```

    Replace the following:

    +   `PROJECT_NUMBER`: the project number of the project containing the workload identity pool.
    +   `POOL_ID`: the workload identity pool ID.
    +   `AGENT_IDENTITY_ID`: the object ID of the Entra agent identity.

## Use Entra Agent ID in the ADK

To let your ADK agent use Entra Agent ID, you use the following custom
authentication providers:

+   [`AgentIdentityAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/entra_auth.py)
    to authenticate the agent with Entra Agent ID and obtain an Entra access token.
+   [`IamFederatedAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/iam_federated_auth.py)
    to exchange the Entra access token for short-lived Google Cloud credentials
    using workload identity federation.

In your agent's source code, set up the providers as follows:

1.  Verify that you are using ADK 2.9.0 or a newer version.
1.  Add the following code to your agent to register 
    the `AgentIdentityAuthProvider` and initialize an `AgentIdentityScheme`:

    ```
    from entra_auth import AgentIdentityAuthProvider, AgentIdentityScheme

    CredentialManager.register_auth_provider(AgentIdentityAuthProvider())
    agent_identity_scheme = AgentIdentityScheme(
        tenant_id="TENANT_ID",
        blueprint_id="BLUEPRINT_APP_ID",
        agent_identity_id="AGENT_IDENTITY_ID",
        scope="api://WLIF_APP_ID/.default"
    )
    ```

    Replace the following:

    +   `TENANT_ID`: your Entra tenant ID.
    +   `BLUEPRINT_APP_ID`: the Entra blueprint App ID.
    +   `AGENT_IDENTITY_ID`: the object ID of the Entra agent identity.
    +   `WLIF_APP_ID`: the Application (client) ID of the Entra app registration used for workload identity federation.
        If you want to use `agent_identity_scheme` directly to access an Azure API or MCP server instead of Google Cloud,
        replace `api://WLIF_APP_ID/.default` with the required Entra scope.

1.  Register the `IamFederatedAuthProvider` and initialize a `WorkloadIdentityScheme`
    that uses the credential from the `AgentIdentityScheme` to obtain short-lived
    Google Cloud credentials:

    ```
    from iam_federated_auth import IamFederatedAuthProvider, WorkloadIdentityScheme

    CredentialManager.register_auth_provider(IamFederatedAuthProvider())
    wlif_scheme = WorkloadIdentityScheme(
        project_number="PROJECT_NUMBER",
        pool_id="POOL_ID",
        provider_id="PROVIDER_ID",
        source_scheme=agent_identity_scheme,
    )
    ```

    Replace the following:

    +   `PROJECT_NUMBER`: the project number of the project containing the workload identity pool.
    +   `POOL_ID`: the workload identity pool ID.
    +   `PROVIDER_ID`: the workload identity provider ID.

1.  Pass the `WorkloadIdentityScheme` (for Google Cloud resources) or `AgentIdentityScheme`
    (for Azure resources) to the constructor of relevant MCP tool sets or use it to
    initialize an `AuthConfig`. For example:

    ```
    # Tool set for Compute Engine
    toolset = McpToolset(
        connection_params=StreamableHTTPConnectionParams(url="https://compute.googleapis.com/mcp"),
        auth_scheme=wlif_scheme
    )
    
    # Tool set from the Agent registry
    registry = AgentRegistry(project_id=PROJECT_ID, location=LOCATION)
    toolset = registry.get_mcp_toolset(
        f"projects/{PROJECT_ID}/locations/{LOCATION}/mcpServers/agentregistry-00000000-0000-0000-aaaa-aaaaaaaaaaaa",
        wlif_scheme
    )
    ```


