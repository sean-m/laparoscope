#Requires -Version 5


$LaparoscopeSession = [ordered]@{
    TenantId    = $null
    ClientId    = $null
    Secret      = $null
    Resource    = $null
    Endpoint    = $null
    AccessToken = $null
    Scope       = $null
}
New-Variable -Scope Script -Name LaparoscopeSession -Value $LaparoscopeSession -Force

function Connect-LaparoscopeTenant {
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
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $TenantId,
        [Parameter(Mandatory=$true)]
        $ClientId,
        [Parameter(Mandatory=$true)]
        $ServiceEndpoint,
        [Parameter(Mandatory=$true)]
        $Resource,
        [Parameter(Mandatory=$true)]
        $ClientSecret,
        $Scope,
        [switch]
        $NoBrowserLaunch
    )
    begin {
        $LaparoscopeSession.TenantId = $TenantId
        $LaparoscopeSession.ClientId = $ClientId
        $LaparoscopeSession.AppId    = $AppIdUri
        $LaparoscopeSession.Resource = $Resource
        $LaparoscopeSession.Endpoint = $ServiceEndpoint
        $LaparoscopeSession.Secret   = $ClientSecret
        $LaparoscopeSession.Scope    = $Scope
    }
    process {
        
        $tokenParams = @{
            Uri = "https://login.microsoftonline.com/$($LaparoscopeSession.TenantId)/oauth2/v2.0/token"            
            Method = 'POST'
            Body= @{
                client_id = $LaparoscopeSession.ClientId
                tenant = $LaparoscopeSession.TenantId
                scope = "$($LaparoscopeSession.Scope)"
                client_secret = $LaparoscopeSession.Secret
                grant_type = 'client_credentials'
            }
        }

        $tokenResponse = Invoke-RestMethod @tokenParams
        
        $LaparoscopeSession.AccessToken = $tokenResponse.access_token
    }
}


function Disconnect-LaparoscopeTenant {
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


function Print-LapAccessToken {
<#
.Synopsis
   Prints the decoded JWT access token Laparoscope is currently using.
.DESCRIPTION
   JWT tokens are standardized in RFC7519 (https://datatracker.ietf.org/doc/html/rfc7519)
   and consist of a header, claims, and signature. This function takes the base64 encoded
   access token, splits on '.', base64 decodes the header and body, then prints
   to the console. Note: Write-Host is used so it is intended for interactive use only.
#>
    begin {
        filter From-Base64 {
            param ($b64)
            [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($b64))
        }

        function ViewJwt {
            param (
                [string]
                $encodedToken
            )

            $header, $body, $sig = $encodedToken.Split('.')
    
            Write-Host -ForegroundColor Yellow 'Header:'
            From-Base64 -b64 $header | ConvertFrom-Json | ConvertTo-Json

            Write-Host ""
            Write-Host -ForegroundColor Yellow 'Body:'
            From-Base64 -b64 $body | ConvertFrom-Json | ConvertTo-Json

    
            Write-Host ""
            Write-Host -ForegroundColor Yellow 'Signature:'
            $sig
        }
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


        Invoke-RestMethod @requestParams
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
    Invoke-LapApi -Path '/api/MeApi'
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
   or stability in these circumstances.
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
   Analogue to Get-ADSyncCSObject. 
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
   Short description
.DESCRIPTION
   Long description
.EXAMPLE
   Example of how to use this cmdlet
.EXAMPLE
   Another example of how to use this cmdlet
#>
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/DomainReachabilityStatus'
}


function Get-LapExportDeletionThreshold {
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
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/ExportDeletionThreshold'
}


function Get-LapGlobalSettings {
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
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/GlobalSettings'
}


function Get-LapMVObject {
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
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/MVObject'
}


function Get-LapPartitionPasswordSyncState {
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
	[CmdletBinding()]
	param()
	Invoke-LapApi -Path '/api/PartitionPasswordSyncState'
}


function Start-LapSync {
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
"Print-LapAccessToken",
#"Invoke-LapApi",

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
"Get-LapSyncScheduler"  #
)
Export-ModuleMember -Function $exportedFunctions