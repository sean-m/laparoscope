
$lastHour = [DateTime]::UtcNow.AddHours(-1)

Get-ADSyncRunProfileResult `
| where { 
    if($_) { $_.StartDate -gt $lastHour -or $_.EndDate -gt $lastHour }
}