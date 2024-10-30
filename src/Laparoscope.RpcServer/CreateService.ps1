
## Takes and ACL and SID, returns an ACL with the correct entry for read-only permissions added.
function Add-ReadAce {
    [OutputType([System.Security.AccessControl.FileSystemSecurity])]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$false)]
        [System.Security.Principal.IdentityReference]$SID,
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, Position=0)]
        [System.Security.AccessControl.FileSystemSecurity]$ACL
        )

    # Rule applies to parent container, does not propagate
    $aclRights = [System.Security.AccessControl.FileSystemRights]"Traverse, ExecuteFile, ListDirectory, ReadData, ReadAttributes, ReadExtendedAttributes, ReadPermissions"
    $objectInherit = [System.Security.AccessControl.InheritanceFlags]"ContainerInherit, ObjectInherit"
    $PropagationFlag = [System.Security.AccessControl.PropagationFlags]::None
    $objType =[System.Security.AccessControl.AccessControlType]::Allow

    $readACE = New-Object System.Security.AccessControl.FileSystemAccessRule `
        ($SID, $aclRights, $objectInherit, $PropagationFlag, $objType)

    $ACL.AddAccessRule($readACE)

    return $ACL
}



$serviceName = 'LaparoscopeRpc'
$binaryPath = (Get-Item .\Laparoscope.RpcServer.exe).FullName

$localServiceAcct = [System.Security.Principal.SecurityIdentifier]::new([System.Security.Principal.WellKnownSidType]::LocalServiceSid, $null)

if (-not $binaryPath) {
    $binaryPath = Read-Host "Enter full path to Laparoscope.RpcServer.exe"
    if (-not (Resolve-Path $binaryPath)) {
        throw "Can't find Laparoscope.RpcServer.exe at path $binaryPath"
    }
}

$binaryParentPath = (Split-Path $binaryPath -Parent)

$acl = Get-Acl $binaryParentPath
$acl = Add-ReadAce -SID $localServiceAcct -ACL $acl
$acl | Set-Acl $binaryParentPath

if (-not (Get-Service $serviceName)) {
    New-Service -Name $serviceName -BinaryPathName "$binaryPath" `
    -Description "PowerShell App Host for Laparoscope service." -DisplayName "Laparoscope RPC Server" -DependsOn "adsync" -StartupType Automatic
    Start-Sleep -Seconds 2
}
sc.exe config "$serviceName" obj="NT AUTHORITY\LocalService" type=own depend=ADSync
Add-LocalGroupMember -Member LocalService -Group ADSyncAdmins