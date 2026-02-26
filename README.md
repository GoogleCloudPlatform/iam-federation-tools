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

## Token Service

_Token Service_ is an application that lets clients exchange custom credentials against
an ID token that suitable for workload identity federation:

*   Towards a client appliation, the Token Service application acts
    as an Open ID Connect identity provider. Clients can authenticate using
    different authentication flows and can obtain an ID token that
    asserts their identity.
    
*   When you register the _Token Service_ [as a workload identity pool provider](https://cloud.google.com/iam/docs/manage-workload-identity-pools-providers), 
    clients can then use the ID token and exchange it against short-lived Google 
    credentials by using the Google STS.
    
The application is designed to run on Cloud Run and uses 
[workload identity federation](https://cloud.google.com/iam/docs/workload-identity-federation).

[<img src="doc/images/documentation.png">](https://googlecloudplatform.github.io/iam-federation-tools/token-service/)

## Workload Authenticator for Windows

_Workload Authenticator for Windows (WWAuth)_ lets Windows applications authenticate to Google Cloud using their 
Active Directory Kerberos credentials. The tool automates the process of using Kerberos credentials to authenticate
to Active Directory Federation Services (AD FS), and using the resulting AD FS credential to authenticate to Google Cloud.

Using WWAuth is an alternative to using service account keys
and doesn't require you to manage and store any secrets or keys.

[<img src="doc/images/documentation.png">](https://googlecloudplatform.github.io/iam-federation-tools/wwauth/)
[<img src="doc/images/download.png">](https://github.com/GoogleCloudPlatform/iam-windows-authenticator/releases/latest/download/wwauth.exe)

--- 

_IAM Federation tools is an open-source project and not an officially supported Google product._

_All files in this repository are under the
[Apache License, Version 2.0](LICENSE.txt) unless noted otherwise._
