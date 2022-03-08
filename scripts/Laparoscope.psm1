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


function Invoke-LapApi {
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
    param (
        [ValidateSet('GET','POST')]
        $Method='GET',
        $Path='/api',
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

        Invoke-RestMethod @requestParams
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
   Short description
.DESCRIPTION
   Long description
.EXAMPLE
   Example of how to use this cmdlet
.EXAMPLE
   Another example of how to use this cmdlet
#>
    [CmdLetBinding()]
    param ($Token)
    $LaparoscopeSession.AccessToken = $Token
}


function Get-LapAccessToken {
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
    $LaparoscopeSession.AccessToken
}


function Get-LapIdentity {
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
    [Alias("DONTYOUKNOWWHOIAM")]
    param()
    Invoke-LapApi -Path '/api/MeApi'
}


function Get-LapSyncScheduler {
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
    param ()
    Invoke-LapApi -Path '/api/Scheduler'
}


function Get-LapAADCompanyFeature {
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
	Invoke-LapApi -Path '/api/AADCompanyFeature'
}


function Get-LapAADPasswordResetConfiguration {
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
	Invoke-LapApi -Path '/api/AADPasswordResetConfiguration'
}


function Get-LapAutoUpgrade {
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
	Invoke-LapApi -Path '/api/AutoUpgrade'
}


function Get-LapConnector {
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
	Invoke-LapApi -Path '/api/Connector'
}


function Get-LapConnectorStatistics {
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
	Invoke-LapApi -Path '/api/ConnectorStatistics'
}


function Get-LapCSObject {
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
	Invoke-LapApi -Path '/api/CSObject'
}


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