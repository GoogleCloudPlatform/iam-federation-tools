# IAM federation tools

The [`iam-federation-tools`](https://github.com/GoogleCloudPlatform/iam-federation-tools/) repository 
contains tools for letting workloads that run outside of Google Cloud
use [workload identity federation :octicons-link-external-16:](https://cloud.google.com/iam/docs/workload-identity-federation)
to authenticate to Google Cloud. 

## Token Service

The [Token Service](token-service.md) lets IoT devices and on-premises workloads authenticate 
to Google Cloud APIs without using service account keys. The service integrates with 
workload identity federation and, by acting as a token broker, lets you use credentials
that workload identity federation doesn't support natively.

## Workload Authenticator for Windows

[Workload Authenticator for Windows (WWAuth)](wwauth.md) lets Windows applications authenticate to Google Cloud using their 
Active Directory Kerberos credentials. The tool automates the process of using Kerberos credentials to authenticate
to Active Directory Federation Services (AD FS), and using the resulting AD FS credential to authenticate to Google Cloud.

Using WWAuth is an alternative to using service account keys
and doesn't require you to manage and store any secrets or keys.