param (
    [Guid]
    $Id,

    [Guid]
    $ConnectorId, 
    
    [int]
    $NumberRequested,
     
    [bool]
    $RunStepDetails=$false
)


$params = @{
}

if ($Id) { $params.Add('Id', $Id) }
if ($ConnectorId) { $params.Add('ConnectorId', $ConnectorId) }
if ($NumberRequested) { $params.Add('NumberRequested', $NumberRequested) }

$lastHour = [DateTime]::UtcNow.AddHours(-1)

Get-ADSyncRunProfileResult -RunStepDetails:$RunStepDetails @params | where { 
    $_.StartDate -gt $lastHour -or $_.EndDate -gt $lastHour
}