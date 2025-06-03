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
        - [x] Show last password hash sync time for a given connector
        - [ ] Show last time your connector was synced
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
- [x] Get-ADSyncRule
- [ ] Get-ADSyncRuleAudit
- [x] Get-ADSyncRunProfileResult
- [ ] Get-ADSyncRunStepResult
- [ ] Get-ADSyncSchedulerConnectorOverride
- [ ] Get-ADSyncSchema
- [x] Get-ADSyncScheduler
- [ ] Get-ADSyncServerConfiguration
- [ ] Start-ADSyncPurgeRunHistory
- [x] Set-ADSyncSchedulerConnectorOverride
- [x] Get-ADSyncSchedulerConnectorOverride

## TODO v2
- [x] Refactor PowerShell commands into .net framework Windows service sidecar.
- [x] Port Owin MVC app to Asp.Net Core LTS.
- [ ] ~~Move authorization processing to Asp.Net Core [resource based authorization]~~(https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-6.0).
  - > The current solution works well enough. This app is largely stateless and doesn't move tons of data, it can be a little inefficient and not end the world. Moving from asp.net to asp.net core has resulted in a nearly 10x reduction in production memory usage. I'm ok with 10x.
+ [x] Run sidecar as an unprivileged Windows Service.
+ [x] Swagger documentation for API. Found at /swagger/v1/swagger.json.

## TODO v2.5
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
-DistinguishedName 'CN=rgrace,OU=Standard,OU=All Users,DC=garage,DC=mcardletech,DC=com'

# Get MVObject from metaverse Id on CSObject
$mvObject = Get-LapMVObject -Id ($csObject.ConnectedMVObjectId)

# Get AAD connector info from metaverse object
$aadConnector = $mvObject.Lineage | where ConnectorName -like "*AAD"

## AAD connector
Get-LapCSObject -ConnectorName ($aadConnector.ConnectorName) -DistinguishedName ($aadConnector.ConnectedCsObjectDN)
```

# Deployment Notes - Configuration Options
The application can be configured with: appsettings.json, environment variables, command line arguments and/or Azure App Config (AAC). That is an ordered list, last wins. When the `AppConfigConnectionString` is set, Azure App Config is utilized, explicitly set this to an empty string if AAC undesired.

# Authorization Rules
This application uses Linq filters for making authorization decisions. These filters are provided as json documents like the following built-in admin policy:
```json
[
    {'Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'*id*','ModelValue':'*','ModelValues':[]},
    {'Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'ConnectorName','ModelValue':'*','ModelValues':[]}
]
```
> **Note:** Policies are loaded from configuration values. Configurations who's names begin with "Rule." will be considered for policy initialization. Failures are logged to the debug trace facility as rule initialization may occur before initializing the logging facility. Those messages can be viewed with [Sysinternals DbgView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview).
> 
> * Authorization policies are loaded by enumerating all IConfiguration sources. If the `LoadedFrom` (not listed above) is unset, the name of the configuration source name is used. Hard coded OOB policies have a LoadedFrom value of 'Built-in'.

This policy is converted to a `List<RoleFilterModel>`. Rules are associated to a given request based on roles present on the ClaimsPrincipal of the request and the controller name of the request context. These rules only match for a principal with the role of `Admin` in any (`*`) context. If there's a desire to check fo values of principal claims, the claim and intended value are indicated with the `ClaimProperty` and `ClaimValue` respectively. Models, objects returned in response to a request, are inspected with the `ModelProperty` and `ModelValue` or `ModelValues` for specifying multiple eligible values. All matching supports wildcard filters with the '*' and are performed case-insensitive. Yes, this does open the possibility of overlapping or overly permissive policies, **be careful**.

Rules are evaluated inclusively. Let's say a user has the assigned roles of: Admin and Operator.Identity. The built-in rules and the following rule would all match and be considered for the context of 'Scheduler' (the SchedulerController in SchedulerController.cs). However the first rule to match wins. If no claim or model criteria match, rule evaluation fails. 

```json
    {'Role':'Operator.*','Context':'Scheduler','ClaimProperty':'','ClaimValue':'','ModelProperty':'*id*','ModelValue':'*','ModelValues':[]}
```

## Examples
## Authorizing a request implicitly
Implicit use of controller names for authorization is the norm. This example comes from GlobalSettingsController.cs:
```json
{
  "Role": "Admin",
  "Context": "GlobalSettings",
  "ClaimProperty": null,
  "ClaimValue": null,
  "ModelProperty": "Authorized",
  "ModelValue": "true",
  "ModelValues": null
}
```
> This rule allows the `Admin` role to access the GlobalSettings controller.
```csharp
public dynamic Get()
{
  // Test for authorization rules for this context with ModelProperty: Authorized,  ModelValue: true
  if (!this.IsAuthorized(new { Authorized = true }))
  {
      throw new HttpResponseException(HttpStatusCode.Unauthorized);
  }
```
Once again, since this use evaluates the request rather than response, we have to synthesize an object to be inspected. Note how the anonymous object passed to `IsAuthorized` has a single property `Authorized` which matches the rule. Roles and the given context are resolved implicitly. The implementation can be found in ControllerAuthorizationExtensions.cs.

### Authorizing a request explicitly
That depends on how the rules are used, there's a couple ways. First, rules may be used to test if a user is authorized to even make a given request. Taking an examle from ConnectorStatisticsController.cs and Filter.cs with a rule from my lab:
```json
{
  "Role": "Garage.Api",
  "Context": "Connector",
  "ClaimProperty": null,
  "ClaimValue": null,
  "ModelProperty": "Name",
  "ModelValue": "garage.mcardletech.com",
  "ModelValues": null
}
```
> This rule would allow the role `Garage.Api` to query for info on the connector named garage.mcardletech.com. Pretty straight forwared.

```csharp
// ConnectorStatisticsController.cs

public dynamic Get(string Name)
{
  ...
  // Construct an anonymous object as the Model for IsAuthorized so we can
  // pass in Connector as the context. This will allow the authorization engine
  // to re-use the rules for /api/Connector. If you have rights to view a given
  // connector, there is no reason you shouldn't see it's statistics.
  var roles = ((ClaimsPrincipal)RequestContext.Principal).RoleClaims();
  if (!Filter.IsAuthorized<dynamic>(new { Name = Name, ConnectorName = Name, Identifier = Name }, "Connector", roles)) {
      throw new HttpResponseException(HttpStatusCode.Forbidden);
  }
...

------------------------
// Filter.cs
public static bool IsAuthorized<T>(T Model, string Context, IEnumerable<string> Roles)
{
  var rules = RegisteredRoleControllerRules.GetRoleControllerModelsByContext(Context);
...
```

The `IsAuthorized ()` method takes an object to be inspected, the context where it occurs (the controller name by convention) and any roles specified on the claims principal. Its first operation collects all the rules for the designated context. Since this filter is matching against the request, not the response, an anonymous object is created with the name of the connector specified in the request. If the requestor doesn't have a rule that would allow querying for the connector of the given name, it denies your request for statistics related that same connector. _Passing the context/controller name into the rule evaulation isn't usually needed._

## Resource based authorization
_Filtering a response._
This example comes from ConnectorController.cs:
```json
{
  "Role": "Garage.Api",
  "Context": "Connector",
  "ClaimProperty": null,
  "ClaimValue": null,
  "ModelProperty": "Name",
  "ModelValue": "garage.mcardletech.com",
  "ModelValues": null
}
```
```csharp

public dynamic Get(string Name=null)
{
  // Run PowerShell command to get AADC connector configurations
  using (var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncConnectorsBasic))
  {
    runner.Parameters.Add("Name", Name);
    runner.Run();

    // Map PowerShell objects to models, C# classes with matching property names.
    // All results should be AadcConnector but CapturePSResult can return as
    // type: dynamic if the PowerShell object doesn't successfully map to the
    // desired model type. For those that are the correct model, we pass them
    // to the IsAuthorized method which loads and runs any rules for this connector
    // who match the requestors roles.
    var resultValues = runner.Results.CapturePSResult<AadcConnector>()
        .Where(x => x is AadcConnector)     // Filter out results that couldn't be captured as AadcConnector.
        .Select(x => x as AadcConnector);   // Take as AadcConnector so typed call to WhereAuthorized avoids GetType() call.

    resultValues = this.WhereAuthorized<AadcConnector>(resultValues);

    if (resultValues != null)
    {
        var result = Ok(resultValues);
        return result;
    }
  }

  ...
```
This example functions on the response side and more closely resembles what one would call a filter. Unlike previous examples, the claims principal is not expected except to extract roles, not to authorize the request itself. Instead, the policy rules which make up the filter act on the AadcConnector models which make up our PowerShell command results. Filtering is performed by our WhereAuthorized extension method. Just like the IsAuthorized extension method above, it acts on the controller object itself to resolve everything needed to assemble a relevant rule list. It takes an IEnumerable<T> to filter out, returning only results _Where_ a rule matched, indicating the result is _Authorized_. If no rules match, an empty set is returned.