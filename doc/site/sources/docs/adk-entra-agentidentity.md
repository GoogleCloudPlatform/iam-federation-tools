---
title: Using Microsoft Entra Agent ID for ADK agents
---

# Using Microsoft Entra Agent ID for ADK agents

This article describes how you can use Microsoft Entra Agent ID to let 
ADK agents access resources across Google Cloud and Microsoft Azure.

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run
*   [ ] You want to use Entra to govern the lifecycle of the agent

## Set up an Entra Agent ID

This section describes how to set up an Entra Agent ID blueprint and 
agent identity for an ADK agent.

### Create a blueprint

Create an Agent ID blueprint by doing the following:

1.  In the Entra admin portal, go to **Agents > Agent blueprints** 
1.  Click **New agent blueprint** and complete the wizard steps.
1.  Open the blueprint and click **Credentials > Federated credentials**.
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
1.  Select **Linked agent identities**.
1.  Open the agent identity that Entra created automatically.
1.  On the overview page, find the **Blueprint App ID** and **Object ID**, 
    you need these identifiers in a later step.

## Set up workload identity

Follow the instructions in [Configure Workload Identity Federation with AWS or Azure VMs](https://docs.cloud.google.com/iam/docs/workload-identity-federation-with-other-clouds#azure)
to create a workload identity pool and federate it with Entra.

## Use Entra Agent ID in the ADK

To let your ADK agent use Entra Agent ID, do the following:

1.  Verify that you are using ADK 2.9.0 or a newer version.
1.  Add the following code to your agent to register 
    the `IamFederatedAuthProvider` and initialize a `AgentIdentityScheme`:

    ```
    CredentialManager.register_auth_provider(IamFederatedAuthProvider())
    agent_identity_scheme = AgentIdentityScheme(
        tenant_id="TENANT_ID",
        blueprint_id="BLUEPRINT_APP_ID",
        agent_identity_id="AGENT_IDENTITY_ID",
        scope="api://WLIF_APP_ID/.default"
    ```

    Replace the following:

    +   `TENANT_ID`: your Entra tenant ID.
    +   `BLUEPRINT_APP_ID`: the Entra blueprint App ID.
    +   `AGENT_IDENTITY_ID`: the object ID of the Entra agent identity.
    +   `WLIF_APP_ID`: the ID of the app registration used for workload identity federation.

1.  Initialize a `WorkloadIdentityScheme` that uses the crdential 
    from the `AgentIdentityScheme` to obtain short-lived Google Cloud credentials:

    ```
    wlif_scheme = WorkloadIdentityScheme(
        project_number="PROJECT_NUMBER",
        pool_id="entra",
        provider_id="entra",
        source_scheme=agent_identity_scheme,
    )
    ```

    Replace the following:

    +   `PROJECT_NUMBER`: the project number of the project containing the workload identity pool.
    +   `POOL_ID`: the workload identity pool ID.
    +   `PROVIDER_ID`: the workload identity provider ID.

1.  Pass the `WorkloadIdentityScheme` to the 
    constructor of relevant MCP tool sets or use it to initialize an `AuthConfig`.

