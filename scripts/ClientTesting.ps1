$certThumb = 'B5F9BF5A3B5A436064F60F7550454CB5928B5A3B'
$clientId = 'd814639a-66c4-4fc6-9141-1784115ee15d'

$scope = 'api://285eb67f-706c-45f4-9063-e150c012c12e/.default' 
$endpoint = 'https://localhost:7217'

Import-Module "C:\Users\smcardle_admin\source\repos\sean-m\laparoscope\scripts\Laparoscope.psm1"

Connect-LaparoscopeTenant -ClientId $clientId -ServiceEndpoint $endpoint `
-CertificateThumbprint $certThumb -TenantId '63157f30-8e13-4d09-bee7-a847b2dbb0a5' `
-Scope $scope



$connectors = Get-LapConnector 

$connectors | where Type -like "AD" | foreach {
    $partitions = $_.Partitions
    $connName = $_.Name
    $partitions | foreach {
        $dn = $_.DN
        $domainDns = Get-LapCSObject -ConnectorName $connName -DistinguishedName $dn
        
        $guidB64 = $domainDns.Attributes | where Name -like "objectGUID" | select -ExpandProperty Values | select -First 1
        $guid = ([guid]([Convert]::FromBase64String($guidB64))).ToString()
        [pscustomobject]@{domain=$connName;partition=$dn;objectGuid=$guid}
    }
}