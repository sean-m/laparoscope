param (
    [Parameter(Mandatory=$true)]
    $ConnectorName,
    [Parameter(Mandatory=$true)]
    $DistinguishedName)

Import-Module ADSync

$attributes = @(
"ObjectId",
"ConnectorId",
"ConnectorName",
"ConnectorType",
"PartitionId",
"DistinguishedName",
"AnchorValue",
"ObjectType",
"IsTransient",
"IsPlaceHolder",
"IsConnector",
"HasSyncError",
"HasExportError",
"ExportError",
"SynchronizationError",
"ConnectedMVObjectId",
"Lineage",
"Attributes"
)

Get-ADSyncCSObject -ConnectorName $ConnectorName -DistinguishedName $DistinguishedName | select $attributes