# Federated federation to AWS

This article describes how you can let an ADK agent use 
[`AssumeRoleWithWebIdentity` :octicons-link-external-16:](https://docs.aws.amazon.com/STS/latest/APIReference/API_AssumeRoleWithWebIdentity.html)
 to assume an AWS role and call AWS APIs and MCP tools that use
 [SigV4 authentication :octicons-link-external-16:](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_sigv.html).

Follow the steps in this article if all of the following applies:

*   [ ] Your agent is deployed on Agent Platform or Cloud Run
*   [ ] Your agent is configured to use agent identity or has an attached service account
*   [ ] You want the agent to access an API or MCP tool that requires 
        AWS SigV4 authentication

## Approach

ADK agents running on Agent Platform or Cloud Run can obtain an ID token that 
asserts their agent identity or the identity of their attached service account.

If you create an AWS role with an appropriate trust policy, you can let an agent
use this ID token to assume the role and obtain temporary AWS credentials. 
The agent can then use these temporary AWS credentials to call AWS APIs,
use the [AWS MCP Server :octicons-link-external-16:](https://docs.aws.amazon.com/agent-toolkit/latest/userguide/mcp-server.html)
or custom MCP servers that require SigV4 authentication.

-   [`AwsFederatedAuthProvider`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/aws_auth.py) 
    is an ADK authentication provider that implements the necessary logic to obtain an ID token 
    and exchange it for temporary AWS credentials.
-   [`AwsMcpToolset`](https://github.com/GoogleCloudPlatform/iam-federation-tools/blob/master/adk/aws_auth.py) 
    extends the ADK-provided `McpToolset` class and lets you use MCP servers that require
    SigV4 authentication, such as the 
    [AWS MCP Server :octicons-link-external-16:](https://docs.aws.amazon.com/agent-toolkit/latest/userguide/mcp-server.html).

Using `AwsFederatedAuthProvider` lets you avoid the need to store AWS secrets on
Google Cloud.

## Create an AWS IAM role

In AWS, [create an IAM role for OIDC :octicons-link-external-16:](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_create_for-idp_oidc.html) and configure a trust policy. The content of the trust policy depends on whether
your agent uses agent identity or an attached service account:

=== "Service account"

    ```
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Effect": "Allow",
          "Principal": {
            "Federated": "accounts.google.com"
          },
          "Action": "sts:AssumeRoleWithWebIdentity",
          "Condition": {
            "StringEquals": {
              "accounts.google.com:oaud": "https://sts.amazonaws.com",
              "accounts.google.com:sub": "ID"
            }
          }
        }
      ]
    }
    ```
    
    Replace `ID` with the ID of the service account. The ID looks similar to the following: `102770123456789012345`.
            

## Use the authentication provider

To let your ADK agent use AWS federation, do the following:

1.  Add the following code to your agent to initialize a `AwsSigV4Scheme` and register the provider:

    ```
    aws_auth_scheme = AwsSigV4Scheme(
        role_arn="arn:aws:iam::ACCOUNT_ID:role/ROLE",
        role_session_name="SESSION_NAME",
    )

    CredentialManager.register_auth_provider(AwsFederatedAuthProvider(
        region_name="STS_REGION"
    ))
    ```

    Replace the following:

    +   `ACCOUNT_ID`: the AWS account ID.
    +   `ROLE`: the name of the AWS role to assume.
    +   `SESSION_NAME`: a name of your choice for the AWS session.
    +   `STS_REGION`: the AWS region to use for STS, for example `eu-central-1`.


2.  Initialize an instance of `AwsMcpToolset` (as a replacement for `McpToolset`)
    and pass the `AwsSigV4Scheme` instance. 
    
    For example, initialize a toolset for the AWS MCP server as follows:

    ```
    aws_toolset = AwsMcpToolset(
        "eu-central-1",
        "bedrock-agentcore",
        "https://aws-mcp.eu-central-1.api.aws/mcp",
        aws_auth_scheme
    )
    ```

3.  Include the following environment variable in your deployment to disable mTLS:

    ```
    GOOGLE_API_USE_CLIENT_CERTIFICATE=False
    ```

    If you use `adk deploy` to deploy the agent, add the environment variable to your `.agent_engine_config.json`.

    !!! important
    
        If you leave mTLS enabled, the [ADK ignores the authentication scheme passed in the constructor and uses application default credentials instead :octicons-link-external-16:](https://github.com/google/adk-python/blob/3bb10115d3ae69cfc42bebcdfa4a935031c8e1a1/src/google/adk/tools/mcp_tool/mcp_session_manager.py#L639).

