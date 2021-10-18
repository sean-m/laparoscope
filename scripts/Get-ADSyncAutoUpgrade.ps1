$state  = Get-ADSyncAutoUpgrade -Detail
$props  = $state | GM | ? MemberType -like "Property"
$result = New-Object PSObject
$props | foreach {
    $val = $state.($_.Name)
    $result | Add-Member -MemberType NoteProperty -Name ($_.Name) -Value ([string]$val)
}
$result