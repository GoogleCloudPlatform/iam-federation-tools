# ADK authentication providers

The following articles describe how you can use custom authentication providers
to let ADK agents authenticate users
and act on their behalf:

+   [Gemini Enterprise delegated authorization](adk-geminienterprise.md) describes
    how you to configure Gemini Enterprise and ADK agents to use delegated authorization.
+   [Microsoft Entra Agent ID on-behalf-of authorization](adk-entra-agentidentity.md)
    describes how you can use Microsoft Entra Agent ID to manage
    access for an ADK agent, and let ADK agents access Google Cloud resources
    and other resources on behalf of Entra users.


The following articles describe how you can use custom authentication providers
to let ADK agents authenticate to APIs and MCP servers on their own behalf:

+   [Cloud Run service authentication](adk-cloudrun.md) describes how to 
    authenticate to APIs and MCP servers that run on Cloud Run and 
    require service-to-service authentication.
+   [Azure workload identity federation](adk-azure.md) describes how to 
    authenticate to Microsoft Azure by using workload identity federation.
+   [AWS federation](adk-aws.md) describes how to authenticate to AWS 
    by assuming a role using `AssumeRoleWithWebIdentity`.