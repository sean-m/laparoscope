Import-Module ADSync

<#
Don't dispatch a sync if the last run was less than 10 minutes ago.
#>

$lastRun = $null
$lastRun = Get-ADSyncRunProfileResult | select -First 1
$schedule = Get-ADSyncScheduler
$threshold = 10

$mostReccent = if ($lastRun.StartDate -gt $lastRun.EndDate) { $lastRun.StartDate } else { $lastRun.EndDate }

$return = New-Object PSObject -Property @{
    Result=''
    Started=$false
}

if (($schedule.SyncCycleEnabled) -and (-not $schedule.SyncCycleInProgress) -and ((-not ($lastRun)) -or ($mostReccent.AddMinutes($threshold) -lt [DateTime]::UtcNow))) {
    try {
        $result = Start-ADSyncSyncCycle -PolicyType Delta | select -ExpandProperty Result
        
        if ($result -like "Success") {
            $return.Result = "Delta sync started at $(Get-Date)."
            $return.Started = $true
        }
        else {
            $return.Result = $result
        }
    }
    catch {
        $return.Result = $_ | Out-String
    }
    
    return $return
}

if (-not $schedule.SyncCycleEnabled) {
    $return.Result = "Syncing currently disabled for maintenance."
    return $return
}

if ($schedule.SyncCycleInProgress) {
    $return.Result = "Currently syncing, cannot start new cycle."
    return $return
}
    
$return.Result = "Last sync was less than $threshold minutes ago; $mostReccent UTC, not starting sync."
return $return