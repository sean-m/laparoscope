param ($Name)

Import-Module ADSync;
$param = @{}
if ($Name) { $param.Add('Name', $Name) }
Get-ADSyncConnector @param | select Name, Identifier, Description, CreationTime, LastModificationTime, ObjectInclusionList, AttributeInclusionList, AnchorConstructionSettings, CompanyName, Type, Subtype