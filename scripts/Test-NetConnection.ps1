param ($ComputerName,
       [int] $Port)

$params = @{}
if ($Port) {
    $params['Port']=$Port
}

$properties = @(
"ComputerName",
@{Name="RemoteAddress";Expression={$_.RemoteAddress | ? { $_} | % { $_.ToString() }}},
"PingSucceeded",
"TcpClientSocket",
"TcpTestSucceeded",
"RemotePort",
"Detailed",
"InterfaceAlias",
"InterfaceIndex",
"InterfaceDescription",
@{Name="SourceAddress";Expression={$_.SourceAddress | ? { $_} | % { $_.ToString() }}},
"NameResolutionSucceeded",
"NetworkIsolationContext"
)

Test-NetConnection -WarningAction SilentlyContinue @params | select $properties | select *