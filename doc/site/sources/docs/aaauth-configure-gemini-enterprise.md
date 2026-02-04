# Configure Gemini Enterprise

This article describes how to configure a Gemini Enterprise so
that agents can use delegated authorization using AAAuth.


???+ info "Before you begin"

    To follow the instructions in this guide, you need the following:

    *   [ ] A Gemini Enterprise app
    *   [ ] An [ADK or A2A agent registration :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/agents-overview) to configure authorization for

## Create an authorization resource

Before you can configure individual agents to use AAAuth for 
authorization, you must create an  authorization resource:

1.  Open a bash or PowerShell prompt
1.  Authenticate gcloud:

        gcloud auth login

1.  Initialize the following variables:

    === "bash"

            PROJECT_ID=project-id
            SERVICE=service
            CLIENT_ID=client-id
            CLIENT_SECRET=client-secret

    === "PowerShell"

            $ProjectId = "project-id"
            $Service = "service"
            $ClientId = "client-id"
            $ClientSecret = "client-secret"

    Replace the following:

    *   `project-id`: project ID of your Gemini Enterprise project
    *   `service`: the domain name of the Cloud Run service -- for example, `service-xxxxxx-as.a.run.app`

    Replace `client-id` and `client-secret` depending on 
    [your identity provider :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/configure-identity-provider):

    === "Google identity"
    
        *   `client-id`: the client ID that you created when you [deployed AAAuth](aaauth-deokoyment.md).
        *   `client-secret`:  the client secret that you created when you [deployed AAAuth](aaauth-deokoyment.md).

    === "Entra"

        *   `client-id`: the client ID of the Entra App registration used by your 
            workforce identity provider.
        *   `client-secret`: a client secretfor the Entra App registration used by your 
            workforce identity provider.

1.  Select the [authorizer](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/aaauth/sources/Google.Solutions.AAAuth/Authorizers/IAuthorizer.cs) for AAAuth to use.

    *   If you use Google identity as identity provider, run the following command:    

        === "bash"

                AUTHORIZER=google-identity

        === "PowerShell"

                $Authorizer = "google-identity"
                

    *   If you use workforce identity and Entra as identity provider, run the following command instead:

        === "bash"

                AUTHORIZER=entra-delegated

        === "PowerShell"

                $Authorizer = "entra-delegated"

1.  Create the authorization resource:

    === "bash"

            curl -s -X POST "https://discoveryengine.googleapis.com/v1alpha/projects/$PROJECT_ID/locations/global/authorizations?authorizationId=aaauth-$AUTHORIZER" \
                -H "Authorization: Bearer $(gcloud auth print-access-token)" \
                -H "Content-Type: application/json" \
                -d @- <<EOF
            {
                "displayName": "Test",
                "serverSideOauth2": {
                    "clientId": "$CLIENT_ID",
                    "clientSecret": "$CLIENT_SECRET",
                    "tokenUri": "https://$SERVICE/$AUTHORIZER/token",
                    "authorizationUri": "https://$SERVICE/$AUTHORIZER/authorize"
                }
            }
            EOF
    
    === "PowerShell"

            Invoke-RestMethod `
                -Uri "https://discoveryengine.googleapis.com/v1alpha/projects/$ProjectId/" + `
                     "locations/global/authorizations?authorizationId=aaauth-$Authorizer" `
                -Method POST `
                -Headers @{
                    "Authorization"      = "Bearer $(gcloud auth print-access-token)"
                    "Content-Type"       = "application/json"
                } `
                -Body (@{
                    "displayName" = "Test"
                    "serverSideOauth2" = @{
                        "clientId" = "$ClientId"
                        "clientSecret" = "$ClientSecret"
                        "tokenUri" = "https://$Service/$Authorizer/token"
                        "authorizationUri" = "https://$Service/$Authorizer/authorize"
                    }
                } | ConvertTo-Json -Depth 2)

## Enable delegated authorization for an agent

To let an agent use AAAuth for authorization, do the following:

1.  Open a bash or PowerShell prompt
1.  Authenticate gcloud:

        gcloud auth login

1.  Initialize the following variables:

    === "bash"

            PROJECT_ID=project-id
            SERVICE=service
            ENGINE=engine
            AGENT=agent

    === "PowerShell"

            $ProjectId = "project-id"
            $Service = "service"
            $Engine = "engine"
            $Agent = "agent"

    Replace the following:

    *   `project-id`: project ID of your Gemini Enterprise project.
    *   `engine`: the ID of your Gemini Enterprise app. 
    *   `agent`: the Name of the Gemini Enterprise agent.

1.  Select the [authorizer](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/aaauth/sources/Google.Solutions.AAAuth/Authorizers/IAuthorizer.cs) for AAAuth to use.

    *   If you use Google identity as identity provider, run the following command:    

        === "bash"

                AUTHORIZER=google-identity

        === "PowerShell"

                $Authorizer = "google-identity"
                

    *   If you use workforce identity and Entra as identity provider, run the following command instead:

        === "bash"

                AUTHORIZER=entra-delegated

        === "PowerShell"

                $Authorizer = "entra-delegated"
            
1.  Link the agent to the authorization resource:

    !!! note
        The command replaces existing authorization configuration for the agent.

    === "bash"

            curl -s -X POST "https://discoveryengine.googleapis.com/v1alpha/$ENGINE/assistants/default_assistant/agents/$AGENT?updateMask=authorizationConfig" \
                -H "Authorization: Bearer $(gcloud auth print-access-token)" \
                -H "Content-Type: application/json" \
                -d @- <<EOF
            {
              "authorizationConfig": {
                "toolAuthorizations": [
                  "projects/$PROJECT_ID/locations/global/authorizations/aaauth-$AUTHORIZER"
                ]
              }
            }
            EOF
            

    === "PowerShell"

            Invoke-RestMethod `
                -Uri "https://discoveryengine.googleapis.com/v1alpha/$Engine" + `
                        "/assistants/default_assistant/agents/$Agent?updateMask=authorizationConfig" `
                -Method POST `
                -Headers @{
                    "Authorization"      = "Bearer $(gcloud auth print-access-token)"
                    "Content-Type"       = "application/json"
                } `
                -Body (@{
                    "authorizationConfig" = @{
                        toolAuthorizations = @(
                            "https://discoveryengine.googleapis.com/v1alpha/projects/$ProjectId/" + `
                            "locations/global/authorizations/aaauth-$Authorizer")
                    }
                } | ConvertTo-Json -Depth 3)

## What's next

Update your ADK agent [to use delegated credentials](aaauth-adk-credentials.md).