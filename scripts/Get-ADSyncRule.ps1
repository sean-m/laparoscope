[CmdletBinding()]
param(
    [Guid]$Identifier
    )

Get-ADSyncRule @PSBoundParameters