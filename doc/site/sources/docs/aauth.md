# AI Agent Authenticator

By default, Gemini Enterprise doesn't propagate the user's identity to
ADK or A2A agents:

-   When you deploy an A2A agent on Cloud Run and register it with Gemini Enterprise,
    Gemini Enterprise can use [its service identity](https://docs.cloud.google.com/run/docs/deploy-a2a-agents#deploy-the-agent) 
    to authenticate to Cloud Run, but doesn't propagate the user's identity or their tokens.
-   When you deploy an ADK agent on Agent Engine and register it with Gemini Enterprise,
    Gemini Enterprise uses its service identity to authenticate to Agent Engine,
    but doesn't propagate the user's identity or their tokens to the ADK agent.

Although agents can use [a service acount or agent identity](https://docs.cloud.google.com/agent-builder/agent-engine/manage/access)
to access resources, doing so can be problematic for Gemini Enterprise agents which 
operate in a close interaction loop with a user and therefore typically fit the
notion of an _assistant_ or _interactive agent_: 

-   Users might be able to trick the agent into performing actions or accessing resources
    that the user themselves isn’t authorized to access, creating a 
    [confused deputy risk](https://en.wikipedia.org/wiki/Confused_deputy_problem).
-   When analyzing audit logs, it can be difficult to discern why an agent
    performed certain operations, and which user it might have been acting on 
    behalf of.

Instead of letting agents use a service acount or agent identity to access resources,
it's often better to propagate the user's identity to the agent, and let the
agent operate _as the user_.

## Delegated authorization

The AI Agent Authenticator (AAAuth) lets you implement delegated authentication between 
Gemini Enterprise and your custom ADK or A2A agents by acting as an intermediary between 
Gemini Enterprise and your identity provider:

![Architecture](images/aaauth.png){ width="570" }

*   If you use Gemini Enterprise with workforce identity federation, you can use
    AAAuth to let users delegate their access to ADK and A2A agents so that agents 
    can access resources on the users' behalf.

    In this scenatio, AAAuth authenticates the user using your external identity provider,
    obtains an ID token and exchanges it against a federated access token by using 
    [workforce identity federation](https://cloud.google.com/iam/docs/workforce-identity-federation),
    and lets Gemini Enterprise forward the resulting access token to the agent.
    
*   If you use Gemini Enterprise with Google authentication, you can use AAAuth 
    to let a users delegate their identity to ADK and A2A agents in a way that enables
    agents to make calls to IAP- and Cloud Run-hosted tool servers on users' behalf.

    In this scenatio, AAAuth obtains a Google ID token for the user, and lets
    Gemini Enterprise forward the ID token to the agent.

## Implementation

AAAuth is a stateless application that is designed for Cloud Run and implemented
using ASP.NET Web API. The application acts as an intermediary between
Gemini Enterprise and your identity provider and exposes the following endpoints:

*   **OpenID provider metadata**: Returns 
    [OpenID provider metadata :octicons-link-external-16:](https://openid.net/specs/openid-connect-discovery-1_0.html#ProviderMetadata) that
    reflects your identity provider's metadata, but uses AAAuth's own
    Authorization- and Token-endpoint.
*   **OAuth authorization**: Handles 
    [OAuth authorization requests :octicons-link-external-16:](https://datatracker.ietf.org/doc/html/rfc6749#section-4.1) by
    redirecting the user to your existing identity provider.
*   **OAuth token**: Handles OAuth token requests by obtaining tokens
    from your external identity provider and, depending on the configuration,
    performing additional token exchanges before returning them to 
    Gemini Enterprise.

AAAuth behaves like an OAuth identity provider, but doesn't issue tokens by
itself. 

# What's next

*   See how you can [deploy AAAuth](aaauth-deployment.md)