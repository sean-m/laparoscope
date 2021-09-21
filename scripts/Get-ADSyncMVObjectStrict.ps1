param (
    [Parameter(Mandatory=$true)]
    $Identifier
    )

Import-Module ADSync

$attributes = @(
"ObjectId",
"Lineage",
"Attributes"
)

Get-ADSyncMVObject -Identifier $Identifier | select $attributes