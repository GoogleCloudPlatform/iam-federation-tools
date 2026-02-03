# Configure a Gemini Enterprise agent to use AAAuth

This article describes how you register an ADK agent in Gemini Enterprise
and configure it to use AAAuth.

1.  In the Cloud Console, go to **Gemini Enterprise > (Your app) > Agents**
1.  Click **Add agent**
1.  On the **Authorizations** page, click **Add authorization** and configure the following settings:

    *   **Authorization name**: Enter a name.
    *   **Client ID** and **Client Secret**: These values depend on the 
        identity provider used by Gemini Enterprise:


        === "Google Identity"

            Enter the client ID and client secret that you created when
            you [deployed AAAuth](aaauth-deokoyment.md).

        === "Entra"

            Enter the client ID of the App Registration used by your 
            workforce identity provider. 

            For client secret, you can either reuse the existing secret
            or create a new secret.

    *   **Token URI**: `https://SERVICE/entra-delegated/token`
    *   **Authorization URI**: `https://SERVICE/entra-delegated/authorize`
    
    In the **Token URI** and **Authorization URI**, replace `SERVICE` is the domain name of the 
    Cloud Run service -- for example, `service-xxxxxx-as.a.run.app`.

1.  Click **Done**.
1.  Click **Next**.
1.  On the **Configuration** page, configure the following settings:

    *   A name and descrption for the agent.
    *   The resource name of the Agent Engine reasoning engine.
    
1.  Click **Create**.
        
Configure access for the agent:

1.  Open the details for the agent that you just registered.
1.  Select the **User permissions** tab.
1.  Grant the **Agent User** role to a relevant set of users.

