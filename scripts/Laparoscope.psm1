#Requires -Version 5


################################################################################
##                              Helper Functions                              ##
################################################################################

function Jwt-CreateToken {
<#
PowerShell to create JWT using RS256.
http://blog.d-apps.com/2013/08/powershell-and-json-web-token-handler.html

Workaround when using System.IdentityModel.Tokens.Jwt 1.0 with certificates smaller than 2048 bits.
Exception calling "WriteToken" with "1" argument(s): "Jwt10530: The 'System.IdentityModel.Tokens.X509AsymmetricSecurityKey' for signing cannot be smaller than '2048' bits. Parameter name: key.KeySize Actual value was 1024."

In 2013, Google's API console would generate P12 keys that was 1024-bits. In 2020, the key size is now 2048-bits.

Instead of using the proof of concept code below, check out libraries listed at https://jwt.io

Tested using:
    Windows 10.0.18362.628
    PowerShell 7.0.0
    Windows PowerShell 5.1.18362.628
#>
    Param(
        [String] $Issuer,
        [String] $Audience,
        [string] $Certificate,
        [string] $CertificateThumbprint,
        [string] $CertificatePassword,
        [System.Security.Claims.Claim[]] $Claims = $null,
        [SecureString] $SigningCertificatePassword,
        [DateTime] $NotBefore,
        [DateTime] $Expires
    )

    function ConvertTo-Base64Url {
        Param(
            $InputObject
        )

        $inputObjectBytes = [System.Convert]::ToBase64String($InputObject)
        return $inputObjectBytes.Replace('=','').Replace('+', '-').Replace('/', '_')
    }

    if ($null -eq $NotBefore) {
        $NotBefore = Get-Date
    }

    if ($null -eq $Expires) {
        $Expires = $NotBefore.AddHours(1)
    }

    $internalHeader = @{
        alg = 'RS256'
        typ = 'JWT'
    }

    $internalClaims = @{
        iss   = $Issuer
        aud   = $Audience
        exp   = ([System.DateTimeOffset]$Expires).ToUnixTimeSeconds()
        iat   = ([System.DateTimeOffset]$NotBefore).ToUnixTimeSeconds()
        nbf   = ([System.DateTimeOffset]$NotBefore).ToUnixTimeSeconds()
    }

    # Merge user provided payload claims with internal payload claims.
    if ($null -ne $Claims) {
        $claims | ForEach-Object -Process {
            $internalClaims.Add($_.Type, $_.Value)
        }
    }

    if ($CertificateThumbprint) {
        $signingX509Certificate = Get-ChildItem -Path "cert:\" -Recurse `
            | Where-Object { $_.Thumbprint -like $CertificateThumbprint } `
            | Where-Object { $_.HasPrivateKey -eq $true } `
            | Select-Object -First 1
    } elseif ((Test-Path -Path $Certificate)) {
        # Use P12 or PFX certificate if it exists.
        if ($PSBoundParameters.ContainsKey('SigningCertificatePassword')) {
            $signingCertificateCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList 'notinuse', $SigningCertificatePassword
            $plaintextSigningCertificatePassword = $signingCertificateCredential.GetNetworkCredential().Password
        } else {
            $plaintextSigningCertificatePassword = $CertificatePassword
        }

        $signingX509Certificate = New-Object -TypeName System.Security.Cryptography.X509Certificates.X509Certificate2($Certificate, $plaintextSigningCertificatePassword, 'Export')
    } else {
        # Find certificate by thumbprint ID.
        $signingX509Certificate = Get-ChildItem -Path "cert:\$Certificate" -Recurse | Where-Object { $_.HasPrivateKey -eq $true } | Select-Object -First 1
    }

    ## Set signing cert hash header
    $x5t = ConvertTo-Base64Url ($signingX509Certificate.GetCertHash())
    $internalHeader.Add('x5t', $x5t)
    
    ## Encode JWT
    $b64Header = ConvertTo-Base64Url -InputObject ([System.Text.Encoding]::UTF8.GetBytes(($internalHeader | ConvertTo-Json -Compress)))
    $b64Claims = ConvertTo-Base64Url -InputObject ([System.Text.Encoding]::UTF8.GetBytes(($internalClaims | ConvertTo-Json -Compress)))

    $baseSignature = $b64Header + '.' + $b64Claims
    $bytesSignature = [System.Text.Encoding]::UTF8.GetBytes($baseSignature)

    ## Sign token
    $bytesSignedSignature = $signingX509Certificate.PrivateKey.SignData($bytesSignature, 
            [System.Security.Cryptography.HashAlgorithmName]::SHA256,
            [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $signedB64Signature = ConvertTo-Base64Url -InputObject $bytesSignedSignature

    
    $payload = $baseSignature + '.' + $signedB64Signature
    do { $payload += '=' } while ((($payload.Length * 6) % 8) -ne 0)

    return ($payload)
}


function Epoch { ([DateTimeOffset]([DateTime]::UtcNow)).ToUnixTimeSeconds() }

function ViewJwt {
    param (
        [string]
        $encodedToken
    )

    $token = DecodeJwt -encodedToken $encodedToken
    
    Write-Host -ForegroundColor Yellow 'Header:'
    $token.Header | ConvertTo-Json

    Write-Host ""
    Write-Host -ForegroundColor Yellow 'Body:'
    $token.Body | ConvertTo-Json

    
    Write-Host ""
    Write-Host -ForegroundColor Yellow 'Signature:'
    $token.Signature
}


function DecodeJwt {
    param ($encodedToken)
    begin {
    filter From-Base64 {
            param ($b64)
            [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($b64 + ("=" * ($b64.Length % 4))))
        }
    }
    process {
        $header, $body, $sig = $encodedToken.Split('.')
        [pscustomobject]@{
            Header    = From-Base64 -b64 $header | ConvertFrom-Json
            Body      = From-Base64 -b64 $body| ConvertFrom-Json
            Signature = $sig
        }
    }
}

################################################################################
##                                Private State                               ##
################################################################################

$LaparoscopeSession = [ordered]@{
    TenantId    = $null
    ClientId    = $null
    Secret      = $null
    Resource    = $null
    Endpoint    = $null
    AccessToken = $null
    Scope       = $null
    Jwt         = $null
    Jti         = $null
    CertThumb   = $null
}
New-Variable -Scope Script -Name LaparoscopeSession -Value $LaparoscopeSession -Force


################################################################################
##                              Module Functions                              ##
################################################################################

function Connect-LaparoscopeTenant {
<#
.Synopsis
   Authenticate to Azure AD with the client credentials grant and store the
   access token.
.DESCRIPTION
   After "connecting", all communication with the Laparoscope service is
   authenticated with bearer token auth (https://datatracker.ietf.org/doc/html/rfc6750).
   The access token encodes some lifetime data that is NOT taken into account
   when making API calls. Parsing expiration time and calling
   Connect-LaparoscopeTenant is an exercise for the user (right now).

   Alternative authentication methods are possible without using this command.
   Server endpoint and access token are the only two peices of info validated
   before making API calls so utilizing certificate auth or a different
   grant type can be handled outside this module then passed in through
   Set-LapAccessToken. Setting the endpoint is similarly handled with
   Set-LapServiceEndpoint.
.EXAMPLE
   # Client secret auth
   $tenantId        = '63157f30-8e13-4d09-bee7-a847b2dbb0a5'
   $clientId        = 'd814639a-66c4-4fc6-9141-1784115ee15b'
   $clientSecret    = 'my super secret secret'
   $serviceEndpoint = 'https://localhost:44359/'
   $scope           = 'api://285eb67f-706c-45f4-9063-e150c012d12e/.default'
   
   Connect-LaparoscopeTenant `
       -TenantId $tenantId `
       -ClientId $clientId `
       -ClientSecret $clientSecret `
       -Scope $scope `
       -ServiceEndpoint $serviceEndpoint

.EXAMPLE
   # Certificate based auth
   $tenantId        = '63157f30-8e13-4d09-bee7-a847b2dbb0a5'
   $clientId        = 'd814639a-66c4-4fc6-9141-1784115ee15b'
   $certThumb       = 'C40A668B2EBEE8E2C387DC823919345F7882E7CE'
   $serviceEndpoint = 'https://localhost:44359/'
   $scope           = 'api://285eb67f-706c-45f4-9063-e150c012d12e/.default'
   
   Connect-LaparoscopeTenant `
       -TenantId $tenantId `
       -ClientId $clientId `
       -CertificateThumbprint $certThumb `
       -Scope $scope `
       -ServiceEndpoint $serviceEndpoint
#>
    [CmdLetBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $TenantId,
        [Parameter(Mandatory=$true)]
        $ClientId,
        [Parameter(Mandatory=$true)]
        $ServiceEndpoint,
        [Parameter(Mandatory=$false)]
        $ClientSecret,
        [Parameter(Mandatory=$false)]
        $CertificateThumbprint,
        $Scope,
        [switch]
        $NoBrowserLaunch
    )
    begin {
        # Clear potentially stale values from a previous connection attempt
        $LaparoscopeSession.AccessToken = $null
        $LaparoscopeSession.CertThumb   = $null
        $LaparoscopeSession.Secret      = $null
        $LaparoscopeSession.Jwt         = $null
        
        # Set private state for use during the session
        $LaparoscopeSession.CertThumb = $CertificateThumbprint
        $LaparoscopeSession.TenantId  = $TenantId
        $LaparoscopeSession.ClientId  = $ClientId
        $LaparoscopeSession.Endpoint  = $ServiceEndpoint
        $LaparoscopeSession.Secret    = $ClientSecret
        $LaparoscopeSession.Scope     = $Scope

        if ($CertificateThumbprint -and $ClientSecret) {
            throw "Certificate based and client secret based auth are mutually exclusive. Pass CertificateThumbprint or ClientSecret, not both."
        }
        if (-not $CertificateThumbprint -and -not $ClientSecret) {
            throw "Must pass CertificateThumbprint or ClientSecret to retrieve an Azure AD access key."
        }
        
    }
    process {
        
        if ($CertificateThumbprint) {
            
            $aud = "https://login.microsoftonline.com/$($LaparoscopeSession.TenantId)/oauth2/v2.0/token"
            $expire = (Get-Date).AddMinutes(60)
            $notBefore = (Get-Date)

            $jti = [Guid]::NewGuid().ToString()
            $LaparoscopeSession.Jti = $jti

            $claims = @(
                [System.Security.Claims.Claim]::new("jti",$jti)
                [System.Security.Claims.Claim]::new("sub",$LaparoscopeSession.ClientId)
            )

            $jwt = Jwt-CreateToken -Issuer $clientId -Audience $aud `
                    -CertificateThumbprint $certThumb `
                    -NotBefore $notBefore `
                    -Expires $expire `
                    -Claims $claims
            
            if (-not $jwt) {
                throw "Failed to create JWT. Cannot get access token from Azure AD."
            }
            
            $LaparoscopeSession.Jwt = $jwt

            $tokenParams = @{
                Uri = "https://login.microsoftonline.com/$($LaparoscopeSession.TenantId)/oauth2/v2.0/token"            
                Method = 'POST'
                Body= @{
                    client_assertion_type = 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'                    
                    client_assertion      = $LaparoscopeSession.Jwt
                    grant_type            = 'client_credentials'                 
                    client_id             = $LaparoscopeSession.ClientId
                    tenant                = $LaparoscopeSession.TenantId
                    scope                 = "$($LaparoscopeSession.Scope)"
                }
            }
            

            $tokenResponse = Invoke-RestMethod @tokenParams
        
            $LaparoscopeSession.AccessToken = $tokenResponse.access_token
        }
        elseif ($ClientSecret) {
            
            $tokenParams = @{
                Uri = "https://login.microsoftonline.com/$($LaparoscopeSession.TenantId)/oauth2/v2.0/token"            
                Method = 'POST'
                Body= @{
                    client_secret = $LaparoscopeSession.Secret
                    grant_type    = 'client_credentials'
                    client_id     = $LaparoscopeSession.ClientId
                    tenant        = $LaparoscopeSession.TenantId
                    scope         = "$($LaparoscopeSession.Scope)"
                }
            }

            $tokenResponse = Invoke-RestMethod @tokenParams
        
            $LaparoscopeSession.AccessToken = $tokenResponse.access_token
        }
        else {
            throw "This shouldn't happen! Pass ClientSecret or CertificateThumbprint."
        }
    }
}


function Disconnect-LaparoscopeTenant {
<#
.Synopsis
   Clears private information stored during the connect process.
#>
    [CmdLetBinding()]
    param ()

    begin {}
    process {
        throw [System.NotImplementedException]::new("Disconnect not implemented yet.")
        ## Log out of AAD
    }
    end {
        Clear-Variable -Name 'LaparoscopeSession'
    }
}


function Test-Variable {
    [CmdLetBinding()]
    param ()
    $LaparoscopeSession | FT -AutoSize
}


function Show-LapAccessToken {
<#
.Synopsis
   Prints the decoded JWT access token Laparoscope is currently using.
.DESCRIPTION
   JWT tokens are standardized in RFC7519 (https://datatracker.ietf.org/doc/html/rfc7519)
   and consist of a header, claims, and signature. This function takes the base64 encoded
   access token, splits on '.', base64 decodes the header and body, then writes
   to the console. Note: Write-Host is used so it is intended for interactive use only.
#>
    begin {
        
    }
    process {
        $token = Get-LapAccessToken
        if ($token) {
            ViewJwt -encodedToken $token
        }
        else {
            Write-Warning "Access token not set! Run Connect-LaparoscopeTenant or Set-AccessToken first."
        }
    }
}

function Show-LapJwt {
<#
.Synopsis
   Prints the decoded JWT client assertion used to authenticate against Azure AD.
.DESCRIPTION
   JWT tokens are standardized in RFC7519 (https://datatracker.ietf.org/doc/html/rfc7519)
   and consist of a header, claims, and signature. This function takes the base64 encoded
   access token, splits on '.', base64 decodes the header and body, then writes
   to the console. Note: Write-Host is used so it is intended for interactive use only.
#>
    begin {
        
    }
    process {
        $token = $LaparoscopeSession.Jwt
        if ($token) {
            ViewJwt -encodedToken $token
        }
        else {
            Write-Warning "Access token not set! Run Connect-LaparoscopeTenant or Set-AccessToken first."
        }
    }
}

function Set-LapServiceEndpoint {
<#
.Synopsis
   Short description
.DESCRIPTION
   Long description
.EXAMPLE
   Example of how to use this cmdlet
.EXAMPLE
   Another example of how to use this cmdlet
#>
    [CmdLetBinding()]
    param ($Endpoint)
    $LaparoscopeSession.Endpoint = $Endpoint
}


function Set-LapAccessToken {
<#
.Synopsis
   Sets base64 encoded JWT access token used during API calls.
   Note: does not validate your input (it's your foot).
#>
    [CmdLetBinding()]
    param ([string]$Token)
    $LaparoscopeSession.AccessToken = $Token
}


function Get-LapAccessToken {
<#
.Synopsis
   Returns base64 encoded JWT access token used during API calls.
#>
    [CmdletBinding()]
    param()
    $LaparoscopeSession.AccessToken
}


function TestAccessTokenExpiration {
    $epoch = Epoch
    $token = DecodeJwt (Get-LapAccessToken)
    $exp = [int]($token.Body.exp)
    if ($exp -lt $epoch) { throw "Token expired! Call Connect-LaparoscopeTenant to refresh." }
}

function Invoke-LapApi {
<#
.Synopsis
   Wrapper around Invoke-RestMethod that sets Accept and Authorization headers for use
   against the REST API.
#>
    param (
        [ValidateSet('GET','POST')]
        $Method='GET',
        $Path='/api',
        [System.Collections.IDictionary]
        $RequestArgs,
        [switch]
        $Anonymous
    )
        begin {
        if (-not $LaparoscopeSession.AccessToken) {
            throw "No access token present! Must call Connect-LaparoscopeTenant and authenticate first."
        }
        if (-not $LaparoscopeSession.Endpoint) {
            throw "No service endpoint set! Must call Connect-LaparoscopeTenant to set the service endpoint first."
        }
        TestAccessTokenExpiration
    }
    process {

        $requestParams = @{
            Method  = $Method
            Uri     = "$($LaparoscopeSession.Endpoint)$Path"
            Headers = @{
                Accept = 'application/json'
            }
        }
        if (-not $Anonymous) { $requestParams.Headers.Add('Authorization', "Bearer $($LaparoscopeSession.AccessToken)") }
        if ($RequestArgs) {
            $requestParams.Add('Body',$RequestArgs)
        }


        Invoke-RestMethod @requestParams -UseBasicParsing
    }
}


function Get-LapIdentity {
<#
.Synopsis
   Invokes GET /api/MeApi.
.DESCRIPTION
   The endpoint returns the ClaimsIdentity token as the Laparoscope server
   recons it. Helpful for debugging claims issues.
#>
    [CmdletBinding()]
    [Alias("DONTYOUKNOWWHOIAM")]
    param()
    Invoke-LapApi -Path '/api/Me'
}


function Get-LapSyncScheduler {
<#
.Synopsis
   Invokes GET /api/Scheduler.
.DESCRIPTION
   Analogue to Get-ADSyncScheduler. Provides information about the current
   ADDC scheduler: staging mode, last sync time, next sync time, current status.
#>
    [CmdletBinding()]
    param ()
    Invoke-LapApi -Path '/api/Scheduler'
}


function Set-LapSyncScheduler {
<#
.Synopsis
   Invokes GET /api/Scheduler.
.DESCRIPTION
   Analogue to Get-ADSyncScheduler. Provides information about the current
   ADDC scheduler: staging mode, last sync time, next sync time, current status.
#>
    [CmdletBinding()]
    param(
        [bool]$SyncCycleEnabled,
        [bool]$SchedulerSuspended,
        [bool]$MaintenanceEnabled,
        [ValidateSet("Unspecified","Delta","Initial")]
        [string]$NextSyncCyclePolicyType
    )
    Invoke-LapApi -Path '/api/Scheduler' -Method POST -RequestArgs $PSBoundParameters
}


function Get-LapAADCompanyFeature {
<#
.Synopsis
   Invokes GET /api/AADCompanyFeature.
.DESCRIPTION
   Analogue to Get-ADSyncAADCompanyFeature. Lists currently configured tenant
   features. Includes: PasswordHashSync, ForcePasswordChangeOnLogOn, UserWriteback,
   DeviceWriteback, UnifiedGroupWriteback, GroupWritebackV2.
#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/AADCompanyFeature'
}


function Get-LapAADPasswordResetConfiguration {
<#
.Synopsis
   Invokes GET /api/AADPasswordResetConfiguration.
.DESCRIPTION
   Analogue to Get-ADSyncAADPasswordResetConfiguration. Lists current configuration
   and service status for AAD Password Reset, the backend for AAD SSPR. 
#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/AADPasswordResetConfiguration'
}


function Get-LapAutoUpgrade {
<#
.Synopsis
   Invokes GET /api/AutoUpgrade.
.DESCRIPTION
   Analogue to Get-ADSyncAutoUpgrade. Returns status of the auto upgrade feature
   of AADC. Note: this will be automatically suspended depending on install
   options or current configuration. Using an external SQL server or custom sync
   rules will disable this feature as automatic upgrade can disrupt configuration
   or stability in these circumstances. The auto upgrade suspension reason can
   be viewed with:
   
   (Get-LapGlobalSettings).Parameters.'Microsoft.OptionalFeature.AutoUpgradeSuspensionReason'

   Note, the quotes are intentional, Microsoft.Optional... is the full property
   name. Removing the quotes yields nothing.
#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/AutoUpgrade'
}


function Get-LapConnector {
<#
.Synopsis
   Invokes GET /api/Connector. Name parameter is optional
.DESCRIPTION
   Analogue to Get-ADSyncConnector. Returns information about a given AADC connector
   if the connector Name is specified. If no name is specified, all connectors
   you have rights to view will be returned.
#>
	[CmdletBinding()]
	param(
        [Parameter(Mandatory=$false,
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [string]
        $Name
    )
    $params = @{
        Path='/api/Connector'
    }
    if ($Name) {
        $params.Add('RequestArgs',@{'Name'=$Name})
    }

	Invoke-LapApi @params
}


function Get-LapConnectorStatistics {
<#
.Synopsis
   Invokes GET /api/ConnectorStatistics. Name parameter is required.
.DESCRIPTION
   Analogue to Get-ADSyncConnectorStatistics. Provides stats about pending
   pending import, export and total connector object counts for a specified
   sync connector.
#>
	[CmdletBinding()]
	param(
        [Parameter(Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            Position=0)]
        [string]
        $Name
    )
    $params = @{
        Path='/api/ConnectorStatistics'
    }
    if ($Name) {
        $params.Add('RequestArgs',@{'Name'=$Name})
    }

	Invoke-LapApi @params
}


function Get-LapCSObject {
<#
.Synopsis
   Invokes GET /api/CSObject.
.DESCRIPTION
   Analogue to Get-ADSyncCSObject. Can be used to check whether a given object 
   is synchronished with AADC and what values are imported for that object.
   Connector name and distinguished name are both required. No search or bulk
   dump capability is exposed.
.EXAMPLE
   $connectorName = 'test.example.com'
   $dn = 'CN=cbackus,OU=Standard,OU=All Users,DC=test,DC=example,DC=com'
   
   Get-LapCSObject -ConnectorName $connectorName `
        -DistinguishedName $dn
#>
	[CmdletBinding()]
	param([Parameter(Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            Position=0)]
          [string]
          $ConnectorName,
          [Parameter(Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            Position=1)]
          [string]
          $DistinguishedName)
process {
    $params = @{
        Path='/api/CSObject'
    }
    
    $params.Add('RequestArgs',@{
        'ConnectorName'=$ConnectorName;
        'DistinguishedName'=$DistinguishedName;
        })

	Invoke-LapApi @params
} }


function Get-LapDomainReachabilityStatus {
<#
.Synopsis
   Invokes GET /api/DomainReachabilityStatus.
.DESCRIPTION
   Analogue to Get-ADSyncDomainReachabilityStatus cmdlet. Tests that the network
   connectivity to the specified domain can be established. Domains are 
   specified by connector name. If the user doesn't have access to the specified
   connector a 403 is returned.
.EXAMPLE
   Get-ADSyncDomainReachabilityStatus -ConnectorName 'test.example.com'

   FullName               IsReachable ExceptionMessage
   --------               ----------- ----------------
   test.example.com              True                 

.EXAMPLE
    Get-ADSyncDomainReachabilityStatus -ConnectorName 'test2.example.com'

    Invoke-RestMethod : The remote server returned an error: (403) Forbidden.
    At C:\scripts\Laparoscope.psm1:232 char:9
    +         Invoke-RestMethod @requestParams
    +         ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        + CategoryInfo          : InvalidOperation: (System.Net.HttpWebRequest:HttpWebRequest) [Invoke-RestMethod], WebException
        + FullyQualifiedErrorId : WebCmdletWebResponseException,Microsoft.PowerShell.Commands.InvokeRestMethodCommand

#>
	[CmdletBinding()]
	param([Parameter(Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            Position=0)]
          [string]
          $ConnectorName)
    $params = @{
        Path='/api/DomainReachabilityStatus'
    }
    $params.Add('RequestArgs',@{
        'ConnectorName'=$ConnectorName;
        })

	Invoke-LapApi @params
}


function Get-LapExportDeletionThreshold {
<#
.Synopsis
   Invokes GET /api/ExportDeletionThreshold.
.DESCRIPTION
   Analogue to Get-ADSyncExportDeletionThreshold cmdlet. If the deletion
   threshold is currently disabled, the DeletionPrevention value will be 0.
#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/ExportDeletionThreshold'
}


function Get-LapGlobalSettings {
<#
.Synopsis
   Invokes GET /api/GlobalSettings.
.DESCRIPTION
   Analogue to Get-ADSyncGlobalSettings cmdlet. Returns settings that are specific
   to the AADC service proper, not the AAD tenant.
.EXAMPLE
   PS C:\scripts> $settings = Get-LapGlobalSettings
   
   
   PS C:\scripts> $settings | FL
   
   
   SqlSchemaVersion : 616
   Parameters       : @{Microsoft.Synchronize....}
   Version          : 22102
   InstanceId       : ecf3f9c3-16da-4c7e-82f2-2d2ce4c105a5
   
   PS C:\scripts> $settings.Parameters
   
   
   Microsoft.Synchronize.TimeInterval                     : 00:30:00
   Microsoft.DeviceWriteBack.Forest                       : 
   Microsoft.SynchronizationOption.JoinCriteria           : AlwaysProvision
   Microsoft.OptionalFeature.GroupWriteBack               : False
   Microsoft.AADFilter.ApplicationList                    : 
   Microsoft.OptionalFeature.AutoUpgradeState             : Suspended
   Microsoft.OptionalFeature.GroupFiltering               : False
   Microsoft.DeviceWriteBack.Container                    : 
   Microsoft.Synchronize.StagingMode                      : False
   Microsoft.SynchronizationOption.CustomAttribute        : 
   Microsoft.SystemInformation.MachineRole                : RoleMemberServer
   Microsoft.OptionalFeature.ExportDeletionThreshold      : False
   Microsoft.ConnectDirectories.WizardDirectoryMode       : ActiveDirectory
   Microsoft.Synchronize.SynchronizationPolicy            : Delta
   Microsoft.Synchronize.NextStartTime                    : Thu, 10 Mar 2022 18:48:01 GMT
   Microsoft.SynchronizationOption.AnchorAttribute        : mS-DS-ConsistencyGuid
   Microsoft.OptionalFeature.DirectoryExtension           : False
   Microsoft.OptionalFeature.DeviceWriteUp                : True
   Microsoft.OptionalFeature.UserWriteBack                : False
   Microsoft.Synchronize.MaintenanceEnabled               : True
   Microsoft.OptionalFeature.ExchangeMailPublicFolder     : False
   Microsoft.UserSignIn.SignOnMethod                      : PasswordHashSync
   Microsoft.AADFilter.AttributeExclusionList             : 
   Microsoft.OptionalFeature.FilterAAD                    : False
   Microsoft.Synchronize.SynchronizationSchedule          : True
   Microsoft.SynchronizationOption.UPNAttribute           : userPrincipalName
   Microsoft.UserWriteBack.Forest                         : 
   Microsoft.Version.SynchronizationRuleImmutableTag      : V1
   Microsoft.DirectoryExtension.SourceTargetAttributesMap : 
   Microsoft.OptionalFeature.DeviceWriteBack              : False
   Microsoft.Synchronize.RunHistoryPurgeInterval          : 7.00:00:00
   Microsoft.Synchronize.SchedulerSuspended               : False
   Microsoft.UserWriteBack.Container                      : 
   Microsoft.OptionalFeature.DirectoryExtensionAttributes : 
   Microsoft.OptionalFeature.ExportDeletionThresholdValue : 500
   Microsoft.OptionalFeature.AutoUpgradeSuspensionReason  : UpgradeNotSupportedNonLocalDbInstall
   Microsoft.Synchronize.ServerConfigurationVersion       : 2.0.25.1
   Microsoft.OptionalFeature.HybridExchange               : False
   Microsoft.UserSignIn.DesktopSsoEnabled                 : False

#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/GlobalSettings'
}


function Get-LapMVObject {
<#
.Synopsis
   Invokes GET /api/MVObject.
.DESCRIPTION
   Returns a given metaverse object (MVObject) by its id. No search function is
   provided by AADC so the best way to find an MVObject is by first querying
   Get-LapCSObject with a domain object's distinguished name. When used in
   tandem it allows following an AD domain object through the sync engine:
   
   AD connector -> Metaverse -> AAD.
   
   Particularly useful when MVbjects may be joined to two source domains; a
   possibility when hosting Exchange in a resource domain. Secondary joins
   are visible by inspecting the Lineage property of a specified MVObject.

   Identity AD Connector -\
                           -> Metaverse -> AAD
   Exchange AD Connector -/

.EXAMPLE

   # Get CSObject from AD DN
   $csObject = Get-LapCSObject -ConnectorName 'test.example.com' `
   -DistinguishedName 'CN=hgroce,OU=Standard,OU=All Users,DC=test,DC=example,DC=com'

   # Get MVObject from metaverse Id on CSObject
   $mvObject = Get-LapMVObject -Id ($csObject.ConnectedMVObjectId)

   # Get AAD connector info from metaverse object
   $aadConnector = $mvObject.Lineage | where ConnectorName -like "*AAD"

   ## AAD connector
   Get-LapCSObject -ConnectorName ($aadConnector.ConnectorName) -DistinguishedName ($aadConnector.ConnectedCsObjectDN)

   ObjectId             : 3184d2b1-2922-ec11-8d0e-f16a15e46b81
   ConnectorId          : b891884f-051e-4a83-95af-2544101c9083
   ConnectorName        : xample.onmicrosoft.com - AAD
   ConnectorType        : Extensible2
   PartitionId          : bf40f90a-841a-48c3-9807-be39a96e0bb3
   DistinguishedName    : CN={49483470516544687930436775392F31734F4A7874513D3D}
   AnchorValue          : VAAAAFUAcwBlAHIAXwBhAGQAOAA5ADQAZgA1ADEALQBjADMAYgAxAC0ANAAwAGYAYwAtADgANwAyADUALQA1AGEANgA3ADIAYgAxADUANgAzADUANgAAAA==
   ObjectType           : user
   IsTransient          : False
   IsPlaceHolder        : False
   IsConnector          : True
   HasSyncError         : False
   HasExportError       : False
   ExportError          : 
   SynchronizationError : 
   ConnectedMVObjectId  : 6f84d2b1-2922-ec11-8d0e-f16a15e46b81
   Lineage              : {@{SyncRuleInternalId=21eb63a4-ccab-4c5a-b5f3-3e5b92c5b1be; SyncRuleName=Out to AAD - User Join; Operation=Provision}, 
                          @{SyncRuleInternalId=629db589-d446-43d3-a147-b32c3f63a83f; SyncRuleName=Out to AAD - User Identity; Operation=Join}, 
                          @{SyncRuleInternalId=4f93fb88-0fda-4d13-a08e-552cbbf449eb; SyncRuleName=Out to AAD - User ExchangeOnline; Operation=Join}, 
                          @{SyncRuleInternalId=eeebc75b-a465-47d3-8a66-48f243a75f07; SyncRuleName=Out to AAD - User DynamicsCRM; Operation=Join}...}
   Attributes           : {@{Name=accountEnabled; Type=2; IsMultiValued=False; SyncRuleName=; ConnectorName=; LineageId=00000000-0000-0000-0000-000000000000; 
                          LastModificationTime=0001-01-01T00:00:00; Values=System.Object[]}, @{Name=cloudAnchor; Type=0; IsMultiValued=False; SyncRuleName=; 
                          ConnectorName=; LineageId=00000000-0000-0000-0000-000000000000; LastModificationTime=0001-01-01T00:00:00; Values=System.Object[]}, 
                          @{Name=cloudMastered; Type=2; IsMultiValued=False; SyncRuleName=; ConnectorName=; LineageId=00000000-0000-0000-0000-000000000000; 
                          LastModificationTime=0001-01-01T00:00:00; Values=System.Object[]}, @{Name=commonName; Type=0; IsMultiValued=False; SyncRuleName=; ConnectorName=; 
                          LineageId=00000000-0000-0000-0000-000000000000; LastModificationTime=0001-01-01T00:00:00; Values=System.Object[]}...}
   
#>
	[CmdletBinding()]
	param([Parameter(Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            Position=0)]
          [string]
          $Id)
process {
    $params = @{
        Path='/api/MVObject'
    }
    
    $params.Add('RequestArgs',@{
        'Id'=$Id;
        })

	Invoke-LapApi @params
} }


function Get-LapPartitionPasswordSyncState {
<#
.Synopsis
   Invokes GET /api/PartitionPasswordSyncState.
.DESCRIPTION
   Analogue to Get-ADSyncPartitionPasswordSyncState cmdlet. Used to check password
   hash sync operation status. Cannot be filtered by specifying a connector
   but is filtered by authorization policy server side. Records not allowed
   by policy are filtered out.
.EXAMPLE
      
   PS C:\scripts> Get-LapPartitionPasswordSyncState
   
   
   ConnectorId                                   : fffb4a69-4ed6-444d-8b89-73bc16f373dd
   DN                                            : DC=test,DC=example,DC=com
   PasswordSyncLastSuccessfulCycleStartTimestamp : 2022-03-10T18:48:23.937
   PasswordSyncLastSuccessfulCycleEndTimestamp   : 2022-03-10T18:48:24.203
   PasswordSyncLastCycleStartTimestamp           : 2022-03-10T18:48:23.937
   PasswordSyncLastCycleEndTimestamp             : 2022-03-10T18:48:24.203
   PasswordSyncLastCycleStatus                   : Successful
   

#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/PartitionPasswordSyncState'
}


function Get-LapRunProfileResult {
<#
.Synopsis
   Invokes GET /api/RunProfileResult.
.DESCRIPTION
   Analogue to Get-ADSyncRunProfileResult cmdlet with a couple exceptions:
   only sync history for the last hour is returned to reduce server side load.
   On systems managing many connected directories, retreiving sync history
   can be a non-trivial operation.
.EXAMPLE

    PS C:\scripts> Get-LapRunProfileResult -NumberRequested 1 -RunStepDetails

    RunHistoryId      : 8a654dc1-cf23-40ed-86f4-b745bd553c22
    ConnectorId       : fffb4a69-4ed6-444d-8b89-73bc16f373dd
    ConnectorName     : garage.mcardletech.com
    RunProfileId      : e73df090-c632-4a61-961a-87ae047570c8
    RunProfileName    : Export
    RunNumber         : 26285
    Username          : NT SERVICE\ADSync
    IsRunComplete     : True
    Result            : no-start-connection
    CurrentStepNumber : 1
    TotalSteps        : 1
    StartDate         : 2022-04-08T20:13:31.163
    EndDate           : 2022-04-08T20:13:52.187
    RunStepResults    : {@{RunHistoryId=8a654dc1-cf23-40ed-86f4-b745bd553c22; StepHistoryId=f0efe315-baf3-4e46-9ebc-92b06213d4ca; 
                        StepNumber=1; StepResult=no-start-connection; StartDate=2022-04-08T20:13:31.17; 
                        EndDate=2022-04-08T20:13:52.183; StageNoChange=0; StageAdd=0; StageUpdate=0; StageRename=0; StageDelete=0; 
                        StageDeleteAdd=0; StageFailure=0; DisconnectorFiltered=0; DisconnectorJoinedNoFlow=0; 
                        DisconnectorJoinedFlow=0; DisconnectorJoinedRemoveMv=0; DisconnectorProjectedNoFlow=0; 
                        DisconnectorProjectedFlow=0; DisconnectorProjectedRemoveMv=0; DisconnectorRemains=0; 
                        ConnectorFilteredRemoveMv=0; ConnectorFilteredLeaveMv=0; ConnectorFlow=0; ConnectorFlowRemoveMv=0; 
                        ConnectorNoFlow=0; ConnectorDeleteRemoveMv=0; ConnectorDeleteLeaveMv=0; ConnectorDeleteAddProcessed=0; 
                        FlowFailure=0; ExportAdd=0; ExportUpdate=0; ExportRename=0; ExportDelete=0; ExportDeleteAdd=0; 
                        ExportFailure=0; CurrentExportBatchNumber=0; LastSuccessfulExportBatchNumber=0; StepFileName=; ConnectorCon
                        nectionInformationXml=<connection-result>failed-connection</connection-result><server>10.2.3.4:389</server>
                        <connection-log><incident><connection-result>failed-connection</connection-result><date>2022-04-08 
                        20:13:52.177</date><server>10.2.3.4:389</server><cd-error><error-code>0x51</error-code>
                        <error-literal>Server Down</error-literal>
                        </cd-error></incident></connection-log>; ConnectorDiscoveryErrors=; ConnectorCountersXml=; SyncErrors=; 
                        StepXml=<step-type type="export">
                        </step-type>
                        <partition>DC=garage,DC=mcardletech,DC=com</partition>
                        <custom-data>
                     
                                          <adma-step-data>
                                             <batch-size>30</batch-size>
                                             <page-size>500</page-size>
                                             <time-limit>120</time-limit>
                                          </adma-step-data>
                                   
                        </custom-data>
                        ; MvRetryErrors=; FlowCountersXml=}}
#>
    [CmdletBinding()]
    param(
        [Guid]
        $RunHistoryId,
        [Guid]
        $ConnectorId,
        [ValidateRange(0, [int]::MaxValue)]
        [int]
        $NumberRequested=0,
        [switch]
        $RunStepDetails
    )

    $requestArgs=@{}
    $params = @{
        Path='/api/RunProfileResult'
    }

    if ($RunHistoryId) { $requestArgs.Add('RunHistoryId', $RunHistoryId) }
    if ($ConnectorId) { $requestArgs.Add('ConnectorId', $ConnectorId) }
    if ($NumberRequested -ne 0) { $requestArgs.Add('NumberRequested', $NumberRequested) }
    if ($RunStepDetails) { $requestArgs.Add('RunStepDetails', $true) }
    if ($requestArgs.Count -gt 0) { $params.Add('RequestArgs', $requestArgs) }

    Invoke-LapApi @params 
}


function Start-LapSync {
<#
.Synopsis
   Invokes POST /api/StartSync.
.DESCRIPTION
   Analogue to Start-ADSyncSyncCycle -PolicyType Delta. The local sync schedule
   is checked and will not initate a sync during or within 10 minutes of the
   previous sync. This is to allow time for downstream systems to reflect
   synced changes.
.EXAMPLE
   PS C:\scripts> Start-LapSync

   Result                                                                              Started
   ------                                                                              -------
   Last sync was less than 10 minutes ago; 03/10/2022 18:48:13 UTC, not starting sync.   False
.EXAMPLE
   Another example of how to use this cmdlet
#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/StartSync' -Method POST
}


$exportedFunctions = @(
"Connect-LaparoscopeTenant",
"Disconnect-LaparoscopeTenant",
#"Test-Variable", # for debug only
"Set-LapServiceEndpoint",
"Set-LapAccessToken",
"Get-LapAccessToken",
"Show-LapAccessToken",
"Show-LapJwt",
"Invoke-LapApi",

"Get-LapIdentity",
"Get-LapAADCompanyFeature",
"Get-LapAADPasswordResetConfiguration",
"Get-LapAutoUpgrade",
"Get-LapConnector",
"Get-LapConnectorStatistics",
"Get-LapCSObject",
"Get-LapDomainReachabilityStatus",
"Get-LapExportDeletionThreshold",
"Get-LapGlobalSettings",
"Get-LapMVObject",
"Get-LapPartitionPasswordSyncState",
"Start-LapSync",
"Get-LapSyncScheduler",
"Set-LapSyncScheduler"
"Get-LapRunProfileResult"
)
Export-ModuleMember -Function $exportedFunctions
