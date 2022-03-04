#Requires -Version 5


$LaparoscopeSession = [ordered]@{
    TenantId    = $null
    ClientId    = $null
    Secret      = $null
    Resource       = $null
    Endpoint    = $null
    AccessToken = $null    
}
New-Variable -Scope Script -Name LaparoscopeSession -Value $LaparoscopeSession -Force

function Connect-LaparoscopeTenant {
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
        [Parameter(Mandatory=$false)]
        $ClientSecret,
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
    }
    process {
        
        $deviceParams = @{
            Uri = "https://login.microsoftonline.com/$($LaparoscopeSession.TenantId)/oauth2/v2.0/devicecode"            
            Method = 'POST'
            Body= @{
                client_id = $LaparoscopeSession.ClientId
                resource = $LaparoscopeSession.Resource
            }
        }

        $deviceResponse = Invoke-RestMethod @deviceParams
        Write-Host $deviceResponse.Message
        Write-Host ''
        Read-Host "After following the instructions above, press enter to continue"

        $tokenParams = @{
            Uri    = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
            Method = 'POST'
            Body   = @{
                grant_type = "urn:ietf:params:oauth:grant-type:device_code"
                code = $deviceResponse.device_code
                client_id  = $LaparoscopeSession.ClientId
            }
        }
        if ($LaparoscopeSession.Secret) {
            $tokenParams.Body.Add('client_secret', $LaparoscopeSession.Secret) | Out-Null
        }
        if ($LaparoscopeSession.Resource) {
            $tokenParams.Body.Add('resource', $LaparoscopeSession.Resource) | Out-Null
        }

        $tokenResponse = $null
        $retry = 0
        do {
            $tokenResponse = try { Invoke-WebRequest @tokenParams }
                             catch { $_.Exception.Response }
            if ($tokenResponse.StatusCode -ne 200) {
                $tokenResponse
            }

            Start-Sleep -Seconds ([Math]::Min(10, (2 * $retry)))
            $retry++
        }
        while ($tokenResponse.StatusCode -ne 200 -and $retry -lt 50)

        
        $LaparoscopeSession.AccessToken = $tokenResponse.access_token
    }
}

function Test-Variable {
    [CmdLetBinding()]
    param ()
    $LaparoscopeSession | FT -AutoSize
}

function Disconnect-LaparoscopeTenant {
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

function Get-LapADSyncScheduler {
    [CmdletBinding()]
    param ()
    Invoke-LapApi -Path '/api/Scheduler'
}

function Invoke-LapApi {
    param (
        [ValidateSet('GET','POST')]
        $Method='GET',
        $Path='/api/Scheduler'
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
                'Authorization' = "Bearer $($LaparoscopeSession.AccessToken)"
                'Accept' = 'application/json'
            }
        }

        Invoke-RestMethod @requestParams
    }
}


function Set-LapServiceEndpoint {
    [CmdLetBinding()]
    param ($Endpoint)
    $LaparoscopeSession.Endpoint = $Endpoint
}

function Set-LapAccessToken {
    [CmdLetBinding()]
    param ($Token)
    $LaparoscopeSession.AccessToken = $Token
}


$exportedFunctions = @(
"Connect-LaparoscopeTenant",
"Disconnect-LaparoscopeTenant",
"Test-Variable",
"Get-LapADSyncScheduler",
"Invoke-LapApi",
"Set-LapServiceEndpoint",
"Set-LapAccessToken"
)
Export-ModuleMember -Function $exportedFunctions