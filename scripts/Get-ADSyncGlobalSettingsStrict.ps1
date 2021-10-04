$global_config = Get-ADSyncGlobalSettings
$parameters = @{}
$global_config.Parameters | foreach {
    $parameters.Add($_.Name, $_.Value)
}

New-Object PSObject -Property @{
    Version = $global_config.Version
    InstanceId = $global_config.InstanceId
    SqlSchemaVersion = $global_config.SqlSchemaVersion
    Parameters = $parameters
}