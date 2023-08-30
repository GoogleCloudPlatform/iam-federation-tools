# Authenticate clients using mutual TLS

The following sections contain examples for how clients can authenticate to the
token broker using mutual TLS (mTLS) and obtain short-lived Google credentials.

!!! note
    If you're using a proxy server, make sure that the proxy server isn’t intercepting 
    TLS for connections to the Token-Service application. 

## Obtain an ID token

To obtain an ID token, do the following:

1. Perform an HTTP request to the Token-Service and authenticate using an mTLS certificate 
 
    === "Windows"
     
        Use the following PowerShell command to perform an HTTP request and authenticate 
        using a certificate from your personal certificate store: 
     
            $Hash = "CERT_HASH"
            $Certificate = Get-ChildItem Cert:\CurrentUser\My\$Hash

            Invoke-RestMethod `
              -Uri "https://PUBLIC_FQDN/token" `
              -Method POST `
              -Certificate $Certificate `
              -Body @{
                "grant_type"="client_credentials"
              } 
            
        Replace the following:

        *   `CERT_HASH`: the certificate thumbprint of a certificate in your personal certificate store.
        *   `PUBLIC_FQDN`: the public FQDN of the load balancer
        
        
        !!! note
            You must run these steps on a device or computer that has a valid client certificate.


    === "Linux"

        
        Use the following `curl` command to perform an HTTP request and authenticate 
        using a certificate and private key file: 
         
            curl "https://PUBLIC_FQDN/token" \
              --data "grant_type=client_credentials" \
              --cert ./user.cer \
              --key ./user.key \
              --verbose

        Replace the following:

        *   `PUBLIC_FQDN`: the public FQDN of the load balancer

    If the response contains an ID token, then deployment was successful. If the request fails, 
    you can find more detailed error information in the logs of the Token Service application 
 
    [Open Logs](https://console.cloud.google.com/logs/query?){ .md-button }