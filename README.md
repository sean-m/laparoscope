# An Introspection Tool for Azure AD Connect
## TODO
- [x] Embed powershell runner
- [x] Return json from powershell commands
- [x] List which OUs are synced per connector. This will necessarily need to be compared with AD proper to derive which OUs are in scope but excluded/included/implicitly-included could be indicated.
	+ [x] API
    + [ ] UI
      - [x] Show sync status
        - [x] Start sync button
      - [x] Show password hash sync for all visible connectors
      - [x] Show connectors you can see
        - [x] Refresh
        - [x] Show last password hash sync time for a given conector
        - [ ] Show last time your connector was syncd
        - [x] Enter DN, get MV info on user
        
- [x] Role based connector interaction.  
> The results of Get-ADSyncConnector shouldn't be filtered by role but the ability to query a CSObject or inspect a connector's configuration should by granted by role. In instances where a single sync service handles many disparite Active Directory domains, the admins of those domains shouldn't have complete visibility into one another's business.
- [x] MVC views for some of the object search functionality, a UI aids in troubleshooting sync errors.
- [x] Admin view for managing roles.
- [x] Admin API for configuration change at runtime.
- [ ] ~~Rate limiting for queries. Globally tunable. Need performance testing to establish baseline.~~ Azure APIM can do this and suits my use case. Leaving this out but may revisit later.
- [ ] ~~Embed scripts with Fody.~~ This turns out to be a terrible idea. Fody will also bundle dll dependencies that IIS can then not resolve during start-up. There may be a way to fix this but not using Fody is the easiest.
- [x] Reference PowerShell module implementation for interfacing with the API.

### Priority Commands
- [x] Get-ADSyncScheduler
- [x] Start-ADSyncSyncCycle 
- [x] Get-ADSyncAADPasswordResetConfiguration *
- [x] Get-ADSyncAADPasswordSyncConfiguration *
- [x] Get-ADSyncCSObject *
- [x] Get-ADSyncMVObject *
- [x] Get-ADSyncConnector *

\* Rate limit, should not invoke if syncing or last sync cycle was < 10 minutes ago.

### Other ADSync Commands (might not do all of them).
- [x] Get-ADSyncAADCompanyFeature
- [ ] Get-ADSyncADConnectorSchemaDsml
- [x] Get-ADSyncAutoUpgrade
- [ ] Get-ADSyncConnectorHierarchyProvisioningDNComponent
- [ ] Get-ADSyncConnectorHierarchyProvisioningMapping
- [ ] Get-ADSyncConnectorHierarchyProvisioningObjectClass
- [ ] Get-ADSyncConnectorParameter
- [ ] Get-ADSyncConnectorPartition
- [ ] Get-ADSyncConnectorPartitionHierarchy
- [ ] Get-ADSyncConnectorRunStatus
- [x] Get-ADSyncConnectorStatistics
- [ ] Get-ADSyncConnectorTypes
- [ ] Get-ADSyncCSGroupStatistics
- [ ] Get-ADSyncCSGroupSummary
- [ ] Get-ADSyncCSObjectLog
- [ ] Get-ADSyncCSObjectReferences
- [ ] Get-ADSyncDatabaseConfiguration
- [x] Get-ADSyncDomainReachabilityStatus
- [x] Get-ADSyncExportDeletionThreshold
- [x] Get-ADSyncGlobalSettings
- ~~Get-ADSyncGlobalSettingsParameter~~ Covered by Get-ADSyncGlobalSettings
- [x] Get-ADSyncPartitionPasswordSyncState
- [ ] Get-ADSyncRule
- [ ] Get-ADSyncRuleAudit
- [x] Get-ADSyncRunProfileResult
- [ ] Get-ADSyncRunStepResult
- [ ] Get-ADSyncSchedulerConnectorOverride
- [ ] Get-ADSyncSchema
- [ ] Get-ADSyncServerConfiguration
- [ ] Start-ADSyncPurgeRunHistory

## TODO v2
- [ ] Refactor PowerShell commands into .net framework Windows service sidecar.
- [ ] Port Owin MVC app to Asp.Net Core LTS.
- [ ] Move authorization processing to Asp.Net Core [resource based authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-6.0).
- [ ] Create worker service to make batch based information available to query:
  + [ ] Per-connector sync errors
  + [ ] Run history summary
  + [ ] Run profile details

# Practical Use Case
## Crosswalking AD -> AADC -> AAD
Cross-walking from connector space to metaverse with objects is working, as well as lineage tracking for a metaverse object. So if we are syncing between many directories where an identity may be provisioned, we can verify that they are in-fact connected to the destination directories. 

:::mermaid
graph LR;
subgraph  Azure AD Connect / MIM
    MV[Identity Store]
    CS2(Connector Space) ==> MV
    CS1(Connector Space) ==> MV
    
    MV --> CS2   
    MV --> CS1
end

subgraph Active Directory
    AD --> CS1
end

subgraph Azure
    CS2 --> Az(Azure)
end
:::

When following a user account from a connector space, it can be queried by the distinguished name of it's origin. In the case of AD it would look like something like this: CN=Super Great Person,CN=Cool Stuff,DC=my,DC=garage. That information can't be used to resolve the identity in the metaverse but the connector space record can be used to get the metaverse guid of the record. From there you get a response like this:

```json
{
    "ObjectId": "a4936da0-dd7a-eb11-8cc5-dad479d24013",
    "Lineage": [
      {
        "LineageId": "00000000-0000-0000-0000-000000000000",
        "ConnectedCsObjectDN": "CN=Sean McArdle - Admin,OU=Administrators,OU=All Users,DC=garage,DC=mcardletech,DC=com",
        "ConnectedCsObjectId": "94936da0-dd7a-eb11-8cc5-dad479d24013",
        "ConnectorName": "garage.mcardletech.com",
        "ConnectorId": "fd54e07c-2c71-4b07-a088-5e246e2d4ef6"
      },
      {
        "LineageId": "00000000-0000-0000-0000-000000000000",
        "ConnectedCsObjectDN": "CN={4170733550384F58476B656F44666B45704A576563673D3D}",
        "ConnectedCsObjectId": "a5936da0-dd7a-eb11-8cc5-dad479d24013",
        "ConnectorName": "mcardletech.onmicrosoft.com - AAD",
        "ConnectorId": "b891884f-051e-4a83-95af-2544101c9083"
      }
    ],
    "Attributes": [
      {
        "Name": "accountEnabled",
        "Type": 2,
        "IsMultiValued": false,
        "SyncRuleName": "In from AD - User AccountEnabled",
        "ConnectorName": "garage.mcardletech.com",
        "LineageId": "5fb2b14b-b0ff-4327-a974-baa19ce3aa56",
        "LastModificationTime": "2021-03-01T22:29:52.627",
        "Values": [
          "true"
        ]
      },
      {
        "Name": "accountName",
        "Type": 0,
        "IsMultiValued": false,
        "SyncRuleName": "In from AD - User Common",
        "ConnectorName": "garage.mcardletech.com",
        "LineageId": "11548997-4721-4ba1-b672-bdcac09d53ca",
        "LastModificationTime": "2021-03-01T22:29:52.627",
        "Values": [
          "smcardle_admin"
        ]
      }
    ]
  }
```

The 'Lineage attribute indicates which connector spaces this metaverse record is connected to and it's id in that connector space, the connector space record on the far side then yields an id that can be used to query the record from the connected directory (ldap, sql, AD, AAD, AWS).

The ergonomics for following those breadcrumbs are terrible but is fit for automation. That would allow tracing a user account through every connected sync engine (AADC, MIM, ADSS) and all directories its provisioned into if only given a single source directory and identifier.

**Update:** this gets easier when using PowerShell and the REST API. Here's some example code to do just that taken from my lab environment. Uses the reference Laparoscope PowerShell module abstraction.
```powershell

$connectorName = 'garage.mcardletech.com'

# Get CSObject from AD DN
$csObject = Get-LapCSObject -ConnectorName 'garage.mcardletech.com' `
-DistinguishedName 'CN=hgroce,OU=Standard,OU=All Users,DC=garage,DC=mcardletech,DC=com'

# Get MVObject from metaverse Id on CSObject
$mvObject = Get-LapMVObject -Id ($csObject.ConnectedMVObjectId)

# Get AAD connector info from metaverse object
$aadConnector = $mvObject.Lineage | where ConnectorName -like "*AAD"

## AAD connector
Get-LapCSObject -ConnectorName ($aadConnector.ConnectorName) -DistinguishedName ($aadConnector.ConnectedCsObjectDN)

```
