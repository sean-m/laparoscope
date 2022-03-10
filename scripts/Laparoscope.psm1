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
   
   $tenantId = '63157f30-8e13-4d09-bee7-a847b2dbb0a5'
   $clientId = 'd814639a-66c4-4fc6-9141-1784115ee15b'
   $clientSecret = 'my super secret secret'
   $serviceEndpoint = 'https://localhost:44359/'
   $scope = 'api://285eb67f-706c-45f4-9063-e150c012d12e/.default'
   
   Connect-LaparoscopeTenant `
       -TenantId $tenantId `
       -ClientId $clientId `
       -ClientSecret $clientSecret `
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
        [Parameter(Mandatory=$true)]
        $ClientSecret,
        $Scope,
        [switch]
        $NoBrowserLaunch
    )
    begin {
        $LaparoscopeSession.TenantId = $TenantId
        $LaparoscopeSession.ClientId = $ClientId
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
"Get-LapSyncScheduler"
)
Export-ModuleMember -Function $exportedFunctions