# IAM Federation tools

This repository contains a collection of open-source tools that let you use 
federated authentication between Google Cloud and external identity providers.

## AI Agent Authenticator

_AI Agent Authenticator (AAAuth)_ enables delegated authentication 
between [Gemini Enterprise and custom ADK or A2A agents](https://docs.cloud.google.com/gemini/enterprise/docs/agents-overview),
including the following:

*   If you use Gemini Enterprise with workforce identity federation, you can use
    AAAuth to let users delegate their access to ADK and A2A agents so that agents 
    can access resources on the users' behalf.
    
*   If you use Gemini Enterprise with Google authentication, you can use AAAuth 
    to let a users delegate their identity to ADK and A2A agents in a way that enables
    agents to make calls to IAP- and Cloud Run-hosted tool servers on users' behalf.

By using AAAuth, you let agents perform actions on behalf of users instead
of having them use their own service identity. This approach helps you reduce 
confused deputy risks and maintain a more detailed audit trail.

The application is designed to run on Cloud Run and uses 
[workforce identity federation](https://cloud.google.com/iam/docs/workforce-identity-federation).

[<img src="doc/images/documentation.png">](https://googlecloudplatform.github.io/iam-federation-tools/aaauth/)

## ADK Authentication Providers

A [collection of example authentication providers](https://googlecloudplatform.github.io/iam-federation-tools/adk/) 
that let ADK agents authenticate in ways the ADK doesn't provide built-in support for.

--- 

_IAM Federation tools is an open-source project and not an officially supported Google product._

_All files in this repository are under the
[Apache License, Version 2.0](LICENSE.txt) unless noted otherwise._
