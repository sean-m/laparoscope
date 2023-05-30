param (
    [Parameter(Mandatory=$true)]
    [string]
    $ConnectorName,
    [Parameter(Mandatory=$true)]
    [bool]
    $FullImportRequired=$false,
    [Parameter(Mandatory=$true)]
    [bool]
    $FullSyncRequired=$false
)

Set-ADSyncSchedulerConnectorOverride @PSBoundParameters