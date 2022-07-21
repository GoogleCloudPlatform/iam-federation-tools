#
# Copyright 2022 Google LLC
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
# 
#   http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#

#
# This script generates a set of credential configuration
# files and creates the necessary configuration in AD FS
# and IAM.
#
# Only intended for testing purposes.
#

param ([Parameter(Mandatory=$True)][string]$PoolName)

$ErrorActionPreference = "Stop"

try {
    $Project = gcloud config get core/project
    Write-Host "Ensure that the current user (ADC) can manage pools in project '$Project'" -ForegroundColor Yellow
}
catch {
    Write-Error "No project selected. Run 'gcloud config set project <..>'"
}


function Get-Pool {
    param(
        [Parameter(Mandatory=$True)][string]$PoolName
    )

    try {
        gcloud iam workload-identity-pools create $PoolName `
            --location=global `
            --format=json `
            --quiet | Out-Null
    }
    catch {
    }

    gcloud iam workload-identity-pools describe $PoolName `
        --location=global `
        --format=json `
        --quiet | ConvertFrom-Json
}

function Get-OidcProvider {
    param(
        [Parameter(Mandatory=$True)][string]$PoolName,
        [Parameter(Mandatory=$True)][string]$ProviderName,
        [Parameter(Mandatory=$True)][string]$IssuerUri,
        [Parameter(Mandatory=$True)][string]$AttributeMapping
    )

    try {
        gcloud iam workload-identity-pools providers create-oidc $ProviderName `
            --workload-identity-pool $PoolName `
            --location=global `
            --issuer-uri $IssuerUri `
            --attribute-mapping $AttributeMapping `
            --format=json `
            --quiet | Out-Null
    }
    catch {
    }
    
    gcloud iam workload-identity-pools providers describe $ProviderName `
            --workload-identity-pool $PoolName `
            --location=global `
            --format=json `
            --quiet | ConvertFrom-Json
}

function Get-SamlProvider {
    param(
        [Parameter(Mandatory=$True)][string]$PoolName,
        [Parameter(Mandatory=$True)][string]$ProviderName,
        [Parameter(Mandatory=$True)][string]$IssuerUrl,
        [Parameter(Mandatory=$True)][string]$AttributeMapping
    )

    $Domain = (New-Object -TypeName Uri -ArgumentList $IssuerUrl).Host
    $TempFile = New-TemporaryFile

    Invoke-WebRequest `
        -Uri "https://$Domain/federationmetadata/2007-06/federationmetadata.xml" `
        -OutFile $TempFile.FullName

    try {
        gcloud iam workload-identity-pools providers create-saml $ProviderName `
            --workload-identity-pool $PoolName `
            --location=global `
            --idp-metadata-path $TempFile.FullName `
            --attribute-mapping $AttributeMapping `
            --format=json `
            --quiet | Out-Null
    }
    catch {
    }
    
    gcloud iam workload-identity-pools providers describe $ProviderName `
            --workload-identity-pool $PoolName `
            --location=global `
            --format=json `
            --quiet | ConvertFrom-Json   
}

function Get-SamlEncryptionCertificate {
    param(
        [Parameter(Mandatory=$True)][string]$PoolName,
        [Parameter(Mandatory=$True)][string]$ProviderName
    )

    $BillingProject = (gcloud config get core/project --quiet)
    try {
        gcloud iam workload-identity-pools providers keys create rsa2048 `
            --workload-identity-pool $PoolName `
            --provider $ProviderName `
            --location=global `
            --use ENCRYPTION `
            --spec RSA_2048 `
            --billing-project $BillingProject
    }
    catch {
    }

    gcloud iam workload-identity-pools providers keys describe rsa2048 `
            --workload-identity-pool $PoolName `
            --provider $ProviderName `
            --location global `
            --billing-project $BillingProject `
            --format "value(keyData.key)"
}

function Get-RequestSigningCertificate {
    $Subject = "CN=SAML Request Signing (SW)"

    $Cert = Get-ChildItem -path cert:\LocalMachine\My | Where-Object {$_.Subject -eq $Subject}

    if ($Cert -eq $null) {
        $Cert = New-SelfSignedCertificate `
           -Subject "SAML Request Signing (SW)" `
           -KeyAlgorithm RSA `
           -KeyLength 2048 `
           -KeyExportPolicy NonExportable `
           -KeyUsage DigitalSignature, KeyEncipherment `
           -Provider 'Microsoft Software Key Storage Provider' `
           -NotAfter (Get-Date).AddDays(365) `
           -CertStoreLocation 'Cert:\LocalMachine\My'
    }

    $Cert
}

function New-SamlConfiguration {
    param(
        [Parameter(Mandatory=$True)][string]$RelyingPartyName,
        [Parameter(Mandatory=$True)][string]$PoolName,
        [Parameter(Mandatory=$True)][string]$ResponseSignature,
        [Parameter(Mandatory=$True)][bool]$EncryptAssertion,
        [Parameter(Mandatory=$True)][bool]$SignRequest
    )

    try {
        Remove-AdfsRelyingPartyTrust -TargetName $RelyingPartyName
    }
    catch {
    }

    #
    # Create a workload identity provider.
    #
    $IssuerUrl = (Get-AdfsProperties).IdTokenIssuer.AbsoluteUri
    $Provider = Get-SamlProvider $PoolName $RelyingPartyName $IssuerUrl "google.subject=assertion.subject"
    
    #
    # Create relying party trust
    #
    $RelyingPartyId = "https://iam.googleapis.com/$($Provider.Name)"
    $AcsUrl = "https://sts.googleapis.com/v1/token"

    $SamlEndpoint = New-AdfsSamlEndpoint `
        -Binding "POST" `
        -Protocol "SAMLAssertionConsumer" `
        -Uri $AcsUrl

    Add-AdfsRelyingPartyTrust `
        -ProtocolProfile "SAML" `
        -Name $RelyingPartyName `
        -Identifier $RelyingPartyId `        -AccessControlPolicyName "Permit everyone"  `        -EncryptClaims  $False `
        -EncryptedNameIdRequired $False `
        -Enabled $True `
        -SamlEndpoint $SamlEndpoint `
        -SamlResponseSignature $ResponseSignature `
        -IssuanceTransformRules "
            c:[] 
                => issue(claim = c); 
            c:[Type == ""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn""] 
                => issue(Type = ""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"", 
                         Issuer = c.Issuer, 
                         OriginalIssuer = c.OriginalIssuer, 
                         Value = c.Value, 
                         ValueType = c.ValueType, 
                         Properties[""http://schemas.xmlsoap.org/ws/2005/05/identity/claimproperties/format""] = ""urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"");" 
    
    if ($SignRequest) {
        $SigningCertificate = Get-RequestSigningCertificate | Select-Object -First 1
        
        Set-AdfsRelyingPartyTrust `
            -TargetName $RelyingPartyName `
            -RequestSigningCertificate $SigningCertificate

        $SigningParam = " /SamlRequestSigningCertificate $($SigningCertificate.Thumbprint) "
    }
    else {
        $SigningParam = ""
    }

    if ($EncryptAssertion) {
        $CertFile = New-TemporaryFile

        Get-SamlEncryptionCertificate $PoolName $RelyingPartyName > $CertFile.FullName
        $EncryptionCertificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertFile.FullName) 

        Set-AdfsRelyingPartyTrust `
            -TargetName $RelyingPartyName `
            -EncryptionCertificate $EncryptionCertificate `
            -EncryptClaims $True
    }

    #
    # Create credential configuration file.
    #
    
    @{
        "type" = "external_account"
        "token_url" = "https://sts.googleapis.com/v1/token"
        "subject_token_type" = "urn:ietf:params:oauth:token-type:saml2"
        "audience" = "//iam.googleapis.com/$($Provider.Name)"
        "credential_source" = @{
            "executable" = @{
                "command" = "wwauth.exe /IssuerUrl $IssuerUrl /Protocol AdfsSamlPost /RelyingPartyId $RelyingPartyId /SamlAcsUrl $AcsUrl $SigningParam"
                "timeout_millis" = 5000
            }
        }
    } | ConvertTo-Json > "$($RelyingPartyName).wwconfig"
}


function New-WsTrustConfiguration {
    param(
        [Parameter(Mandatory=$True)][string]$RelyingPartyName,
        [Parameter(Mandatory=$True)][string]$PoolName
    )

    try {
        Remove-AdfsRelyingPartyTrust -TargetName $RelyingPartyName
    }
    catch {
    }

    #
    # Create a workload identity provider.
    #
    $IssuerUrl = (Get-AdfsProperties).IdTokenIssuer.AbsoluteUri
    $Provider = Get-SamlProvider $PoolName $RelyingPartyName $IssuerUrl "google.subject=assertion.subject"
    
    #
    # Create relying party trust
    #
    $RelyingPartyId = "https://iam.googleapis.com/$($Provider.Name)"

    Add-AdfsRelyingPartyTrust `
        -ProtocolProfile "SAML" `
        -Name $RelyingPartyName `
        -Identifier $RelyingPartyId `        -AccessControlPolicyName "Permit everyone"  `        -EncryptClaims  $False `
        -EncryptedNameIdRequired $False `
        -Enabled $True `
        -IssuanceTransformRules "
            c:[] 
                => issue(claim = c); 
            c:[Type == ""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn""] 
                => issue(Type = ""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"", 
                         Issuer = c.Issuer, 
                         OriginalIssuer = c.OriginalIssuer, 
                         Value = c.Value, 
                         ValueType = c.ValueType, 
                         Properties[""http://schemas.xmlsoap.org/ws/2005/05/identity/claimproperties/format""] = ""urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"");" 

    #
    # Create credential configuration file.
    #
    
    @{
        "type" = "external_account"
        "token_url" = "https://sts.googleapis.com/v1/token"
        "subject_token_type" = "urn:ietf:params:oauth:token-type:saml2"
        "audience" = "//iam.googleapis.com/$($Provider.Name)"
        "credential_source" = @{
            "executable" = @{
                "command" = "wwauth.exe /IssuerUrl $IssuerUrl /Protocol AdfsWsTrust /RelyingPartyId $RelyingPartyId"
                "timeout_millis" = 5000
            }
        }
    } | ConvertTo-Json > "$($RelyingPartyName).wwconfig"
}

function New-OidcConfiguration {
    param(
        [Parameter(Mandatory=$True)][string]$ClientName,
        [Parameter(Mandatory=$True)][string]$PoolName
    )

    try {
        #Remove-AdfsClient -TargetName $ClientName
        #Remove-AdfsWebApiApplication -TargetName $ClientName
        Remove-AdfsApplicationGroup `
            -TargetApplicationGroupIdentifier $ClientName
    }
    catch {
    }
    
    #
    # Create a workload identity provider.
    #
    $IssuerUrl = (Get-AdfsProperties).IdTokenIssuer.AbsoluteUri
    $Provider = Get-OidcProvider $PoolName $ClientName $IssuerUrl "google.subject=assertion.appid"
    
    New-AdfsApplicationGroup `
        -Name $ClientName `

    #
    # Create resource
    #
    $RelyingPartyId = "https://iam.googleapis.com/$($Provider.Name)"

    Add-AdfsWebApiApplication `
        -ApplicationGroupIdentifier $ClientName `
        -Name "$ClientName-pool" `
        -Identifier $RelyingPartyId `
        -IssuanceTransformRules "c:[] => issue(claim = c);" `
        -AccessControlPolicyName  "Permit everyone"

    #
    # Create client
    #
    Add-AdfsServerApplication `
        -ApplicationGroupIdentifier $ClientName `
        -Name $ClientName `
        -Identifier $ClientName `
        -ADUserPrincipalName (whoami /upn)

    Grant-AdfsApplicationPermission `
        -ClientRoleIdentifier $ClientName `
        -ServerRoleIdentifier $RelyingPartyId `
        -ScopeNames "openid"

    #
    # Create credential configuration file.
    #
    
    @{
        "type" = "external_account"
        "token_url" = "https://sts.googleapis.com/v1/token"
        "subject_token_type" = "urn:ietf:params:oauth:token-type:saml2"
        "audience" = "//iam.googleapis.com/$($Provider.Name)"
        "credential_source" = @{
            "executable" = @{
                "command" = "wwauth.exe /IssuerUrl $IssuerUrl /Protocol AdfsOidc /RelyingPartyId $RelyingPartyId /OidcClientId $ClientName"
                "timeout_millis" = 5000
            }
        }
    } | ConvertTo-Json > "$($ClientName).wwconfig"
}

Get-Pool $PoolName
New-OidcConfiguration    "$PoolName-oidc"     $PoolName
New-WsTrustConfiguration "$PoolName-wstrust"  $PoolName
New-SamlConfiguration    "$PoolName-saml-uus" $PoolName "AssertionOnly"       $False $False
New-SamlConfiguration    "$PoolName-saml-uss" $PoolName "MessageAndAssertion" $False $False
New-SamlConfiguration    "$PoolName-saml-usu" $PoolName "MessageOnly"         $False $False
New-SamlConfiguration    "$PoolName-saml-uue" $PoolName "MessageAndAssertion" $True  $False
New-SamlConfiguration    "$PoolName-saml-sus" $PoolName "AssertionOnly"       $False $True