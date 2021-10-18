
$connector = @(Get-ADSyncConnector | ? Type -like "Extensible2" | ? Subtype -like "Windows Azure Active Directory*")

if ($connector.Count -gt 1) {
    throw "Unsupported AADC configuration. More than one AAD connector is not supported."
}

Get-ADSyncAADPasswordResetConfiguration -Connector ($connector.Name)