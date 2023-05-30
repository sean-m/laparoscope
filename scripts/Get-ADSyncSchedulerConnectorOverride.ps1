param (
    [Parameter(Mandatory=$true)]
    [string]
    $ConnectorName
)

Get-ADSyncSchedulerConnectorOverride @PSBoundParameters