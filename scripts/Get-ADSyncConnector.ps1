param ($Name)

Import-Module ADSync;
$param = @{}
if ($Name) { $param.Add('Name', $Name) }
$fmt_connectivity_params = @{
    Name="ConnectivityParameters"
    Expression={
        $val = New-Object PSObject
        $_.ConnectivityParameters | foreach {
            $p = $_
            $val | Add-Member -MemberType NoteProperty -Name $p.Name -Value $p.Value
        }
        $val
    }
}
$fmt_partition_params = @{
    Name="Partitions"
    Expression={
        $fmt_params = @{
            Name="Parameters"
            Expression={
                $val = New-Object PSObject
                $_.ConnectivityParameters | foreach {
                    $p = $_
                    $val | Add-Member -MemberType NoteProperty -Name $p.Name -Value $p.Value
                }
                $val
            }
        }
        $partition_params = @(
            "Identifier",
            "DN",
            "Version",
            "CreationTime",
            "LastModificationTime",
            "Selected",
            "ConnectorPartitionScope",
            "Name",
            $fmt_params,
            "PreferredDCs",
            "IsDomain"
        )
        $_.Partitions | select $partition_params
    }
}

$conn = Get-ADSyncConnector @param | select Name, Identifier, Description, CreationTime, LastModificationTime, $fmt_partition_params, $fmt_connectivity_params, ObjectInclusionList, AttributeInclusionList, AnchorConstructionSettings, CompanyName, Type, Subtype
$aad, $other = $conn.Where({$_.Subtype -like 'Windows Azure Active Directory (Microsoft)'},'Split')
@($aad, ($other | Sort Name))
