# IAM federation tools

The [`iam-federation-tools`](https://github.com/GoogleCloudPlatform/iam-federation-tools/) repository 
contains tools for letting workloads that run outside of Google Cloud
use [workload identity federation :octicons-link-external-16:](https://cloud.google.com/iam/docs/workload-identity-federation)
to authenticate to Google Cloud. 

## Token Service

The [Token Service](token-service.md) is an application that lets clients exchange custom credentials against
an ID token that suitable for workload identity federation:

*   Towards a client appliation, the Token Service application acts
    as an Open ID Connect identity provider. Clients can authenticate using
    different authentication flows and can obtain an ID token that
    asserts their identity.
    
*   When you register the _Token Service_
    [as a workload identity pool provider :octicons-link-external-16:](https://cloud.google.com/iam/docs/manage-workload-identity-pools-providers), 
    clients can then use the ID token and exchange it against short-lived Google 
    credentials by using the Google STS.

## Workload Authenticator for Windows

[Workload Authenticator for Windows (WWAuth)](wwauth.md) lets Windows applications authenticate to Google Cloud using their 
Active Directory Kerberos credentials. The tool automates the process of using Kerberos credentials to authenticate
to Active Directory Federation Services (AD FS), and using the resulting AD FS credential to authenticate to Google Cloud.

Using WWAuth is an alternative to using service account keys
and doesn't require you to manage and store any secrets or keys.