# An Introspection Tool for Azure AD Connect
## TODO
- [x] Embed powershell runner
- [x] Return json from powershell commands
- [ ] List which OUs are synced per connector. This will necessarily need to be compared with AD proper to derive which OUs are in scope but excluded/included/implicitly-included could be indicated.
- [ ] Role based connector interaction. The results of Get-ADSyncConnector shouldn't be filtered by role but the ability to query a CSObject or inspect a connector's configuration should by granted by role. In instances where a single sync service handles many disparite Active Directory domains, the admins of those domains shouldn't have complete visibility into one another's business.
- [ ] MVC views for some of the object search functionality, a UI aids in troubleshooting sync errors.
- [ ] Admin view for managing roles.
- [ ] Admin API for configuration change at runtime.
- [ ] Rate limiting for queries. Globally tunable. Need performance testing to establish baseline.

### Priority Commands
- [x] Get-ADSyncScheduler
- [ ] Start-ADSyncSyncCycle * Rate limit, should not invoke if syncing or last sync cycle was < 10 minutes ago.
- [ ] Get-ADSyncAADPasswordResetConfiguration *
- [ ] Get-ADSyncAADPasswordSyncConfiguration *
- [ ] Get-ADSyncCSObject *
- [x] Get-ADSyncConnector *

### Other ADSync Commands (might not do all of them).
- [ ] Get-ADSyncAADCompanyFeature
- [ ] Get-ADSyncADConnectorSchemaDsml
- [ ] Get-ADSyncAutoUpgrade
- [ ] Get-ADSyncConnectorHierarchyProvisioningDNComponent
- [ ] Get-ADSyncConnectorHierarchyProvisioningMapping
- [ ] Get-ADSyncConnectorHierarchyProvisioningObjectClass
- [ ] Get-ADSyncConnectorParameter
- [ ] Get-ADSyncConnectorPartition
- [ ] Get-ADSyncConnectorPartitionHierarchy
- [ ] Get-ADSyncConnectorRunStatus
- [ ] Get-ADSyncConnectorStatistics
- [ ] Get-ADSyncConnectorTypes
- [ ] Get-ADSyncCSGroupStatistics
- [ ] Get-ADSyncCSGroupSummary
- [ ] Get-ADSyncCSObjectLog
- [ ] Get-ADSyncCSObjectReferences
- [ ] Get-ADSyncDatabaseConfiguration
- [ ] Get-ADSyncDomainReachabilityStatus
- [ ] Get-ADSyncExportDeletionThreshold
- [ ] Get-ADSyncGlobalSettings
- [ ] Get-ADSyncGlobalSettingsParameter
- [ ] Get-ADSyncMVObject
- [ ] Get-ADSyncPartitionPasswordSyncState
- [ ] Get-ADSyncRule
- [ ] Get-ADSyncRuleAudit
- [ ] Get-ADSyncRunProfileResult
- [ ] Get-ADSyncRunStepResult
- [ ] Get-ADSyncSchedulerConnectorOverride
- [ ] Get-ADSyncSchema
- [ ] Get-ADSyncServerConfiguration
- [ ] Start-ADSyncPurgeRunHistory
