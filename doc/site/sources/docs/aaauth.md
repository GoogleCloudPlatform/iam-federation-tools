# AI Agent Authenticator

The _AI Agent Authenticator (AAAuth)_ is an application that enables delegated authentication 
between [Gemini Enterprise and custom ADK or A2A agents](https://docs.cloud.google.com/gemini/enterprise/docs/agents-overview).

## Letting agents authenticate to MCP servers and APIs

When an agent needs to interact with an MCP server or access Google Cloud resources, 
it needs to authenticate. An agent can authenticate in two ways:

1.  The agent can use its
    [attached service account or agent identity :octicons-link-external-16:](https://docs.cloud.google.com/agent-builder/agent-engine/manage/access)
    to authenticate.

    However, for Gemini Enterprise agents, this approach can be problematic: Gemini Enterprise agents
    operate in a close interaction loop with a user and tend to fit the notion of an
    _assistant_ or _interactive agent_. As such, they act on behalf of a user, not on 
    their own behalf.

    A Gemini Enterprise agent that uses a service account or agent identity
    to access resources is prone to falling victim of 
    [confused deputy attacks :octicons-link-external-16:](https://en.wikipedia.org/wiki/Confused_deputy_problem), in
    which a user tricks the agent into performing actions or accessing resources
    that the user themselves isn't authorized to access.

2.  The agent can use OAuth to request authorization from the Gemini Enterprise user to 
    act on their behalf. This lets the agent access resources _as the user_.

    This approach, referred to as _delegated authorization_, helps prevent confused deputy attacks and
    is a better way for most Gemini Enterprise agents to authenticate.

Gemini Enterprise lets you implement (2) by configuring 
[agent authorization :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/register-and-manage-an-a2a-agent#configure_authorization_details). Depending on 
[your identity provider :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/configure-identity-provider),
agent authorization lets you implement the following scenarios:

| Scenario                                                                | Google identity         | Workforce identity (Entra) | Workforce identity (other) |
| ------------------------------------------------------------------------| ------------------------| ---------------------------| ---------------------------|
| Access Google APIs on behalf of the user <br>(requires an access token) | :material-check:        | :material-close:           | :material-close:           |
| Access other Cloud Run services          <br>(requires an ID token)     | :material-close:        | :material-close:           | :material-close:           |
| Access other IAP-protected services      <br>(requires an ID token)     | :material-close:        | :material-close:           | :material-close:           |
| Access Azure, M365 on behalf of the user <br>(requires an access token) | :material-close:        | :material-check:           | :material-close:           |

AAAuth lets you implement delegated authorization for a number of additional scenarios:

| Scenario                                                                | Google identity         | Workforce identity (Entra) | Workforce identity (other) |
| ------------------------------------------------------------------------| ------------------------| ---------------------------| ---------------------------|
| Access Google APIs on behalf of the user <br>(requires an access token) | :material-check:        | :material-check-circle:    | :material-close:           |
| Access other Cloud Run services          <br>(requires an ID token)     | :material-check-circle: | :material-close:           | :material-close:           |
| Access other IAP-protected services      <br>(requires an ID token)     | :material-check-circle: | :material-close:           | :material-close:           |
| Access Azure, M365 on behalf of the user <br>(requires an access token) | :material-close:        | :material-check:           | :material-close:           |


## Delegated authorization using AAAuth

AAAuth works by acting as an intermediary between Gemini Enterprise and your identity provider:

*   When you set up agent authorization, you configure AAAuth as identity provider.
*   AAAuth redirects the user to your actual identity provider, which might be
    Google or an external identity such as Microsoft Entra.
*   Before returning to Gemini Enterprise, AAAuth performs an additional
    token exchange to ensure that Gemini Enterprise receives the right
    token for the scenario you're implementing.

![Architecture](images/aaauth.png){ width="570" }

That way, AAAuth lets you implement delegated authorization for a number of additional scenarios:

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
using ASP.NET Web API. The application exposes the following endpoints:

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
itself -- it merely mediates between your identity provider and Gemini Enterprise.

## What's next

[Deploy AAAuth](aaauth-deployment.md) to Cloud Run by using Terraform.