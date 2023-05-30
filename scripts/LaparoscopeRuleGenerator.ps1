param (
    [Parameter(Mandatory=$true)]
    $RoleName,
    [Parameter(Mandatory=$true)]
    [ValidateScript({$_ -like "*.*"})]
    $DomainName
)

filter ApplyRoleNameAndConnector {
<#
    Format and insert the FQDN for the given AD connector to scope the rule.
#>
    param (
        [Parameter(ValueFromPipeline=$true)]
        $Rule
    )
    $Rule.Id = [Guid]::NewGuid()
    $Rule.Role = $RoleName

    switch ($Rule.Context) {
        DomainRechabilityStatus {
            $Rule.ModelProperty = 'FullName'
            $Rule.ModelValue = $DomainName
        }
        PartitionPasswordSyncState {
            $Rule.ModelProperty = 'DN'
            $Rule.ModelValue = "*=" + $DomainName.Replace('.',',*=')
        }
        Connector {
            $Rule.ModelProperty = 'Name'
            $Rule.ModelValue = $DomainName
        }
        Default {
            $Rule.ModelProperty = 'ConnectorName'
            $Rule.ModelValue = $DomainName
        }
    }

    $Rule
}

## Standard rule set that scopes a role a given connector by domain name.
$rules = @"
{
"Rule.AdminDomainReachability": "{\"Id\":\"7151a67c-ac38-43c9-94bd-23ecdc72d6ef\",\"Role\":\"PLACEHOLDER\",\"Context\":\"DomainReachabilityStatus\",\"ClaimProperty\":null,\"ClaimValue\":null,\"ModelProperty\":\"FullName\",\"ModelValue\":\"PLACEHOLDER.DOMAIN\",\"ModelValues\":null}",
"Rule.CSobject": "{\"Id\":\"54d33fb3-3bab-4959-8b2c-ae3fd8b6829d\",\"Role\":\"PLACEHOLDER\",\"Context\":\"CSObject\",\"ClaimProperty\":null,\"ClaimValue\":null,\"ModelProperty\":\"ConnectorName\",\"ModelValue\":\"PLACEHOLDER.DOMAIN\",\"ModelValues\":null}",
"Rule.RunProfileResult": "{\"Id\":\"2356095b-9462-465b-af8b-1edaeeb47455\",\"Role\":\"PLACEHOLDER\",\"Context\":\"RunProfileResult\",\"ClaimProperty\":null,\"ClaimValue\":null,\"ModelProperty\":\"ConnectorName\",\"ModelValue\":\"PLACEHOLDER.DOMAIN\",\"ModelValues\":null}",
"Rule.PartitionPasswordSyncState": "{\n    \"Id\": \"9b75244c-b5b4-4fe3-93fc-409db9d9e2ab\",\n    \"Role\": \"Garage\",\n    \"Context\": \"PartitionPasswordSyncState\",\n    \"ClaimProperty\": null,\n    \"ClaimValue\": null,\n    \"ModelProperty\": \"DN\",\n    \"ModelValue\": \"*PLACEHOLDER*DOMAIN\",\n    \"ModelValues\": null\n}",
"Rule.Connector": "{\"Id\":\"9b75244c-b5b4-4fe3-93fc-409db9d9e2dc\",\"Role\":\"PLACEHOLDER\",\"Context\":\"Connector\",\"ClaimProperty\":null,\"ClaimValue\":null,\"ModelProperty\":\"Name\",\"ModelValue\":\"PLACEHOLDER.DOMAIN\",\"ModelValues\":null}"
}
"@ | ConvertFrom-Json

$ruleNames = $rules.PSObject.Properties.Name | ? { $_ -like "Rule.*" }
$ruleNames | % { 
    $propertyName = $_
    $rule = $rules.$_
    $rule = $rule | ConvertFrom-Json | ApplyRoleNameAndConnector
    $rule | Add-Member -MemberType NoteProperty -Name RuleName -Value ("{0}_{1}" -f $propertyName, $RoleName)
    $rule
}