targetScope = 'resourceGroup'

@description('Azure region for regional resources.')
param location string = resourceGroup().location

@description('Azure region for Static Web Apps. Static Web Apps is global/static-edge backed, but the managed service still requires a supported creation region.')
param staticWebAppLocation string = 'eastus2'

@description('Short workload name used in Azure resource names.')
param workloadName string = 'purchase-order'

@description('Environment label used in Azure resource names and tags.')
param environmentName string = 'dev'

@description('Tags applied to all Azure resources.')
param tags object = {}

@description('Initial API image. The API workflow replaces this with the image built from docker/api.dockerfile.')
param apiImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Port exposed by the API container.')
param apiTargetPort int = 8080

@description('Minimum API Container App replicas.')
@minValue(0)
param apiMinReplicas int = 0

@description('Maximum API Container App replicas.')
@minValue(1)
param apiMaxReplicas int = 3

@description('CPU allocated to each API replica.')
@allowed([
  '0.25'
  '0.5'
  '0.75'
  '1'
  '1.25'
  '1.5'
  '1.75'
  '2'
])
param apiCpu string = '0.5'

@description('Memory allocated to each API replica.')
@allowed([
  '0.5Gi'
  '1Gi'
  '1.5Gi'
  '2Gi'
  '3Gi'
  '4Gi'
])
param apiMemory string = '1Gi'

@description('Optional CORS origins for the API. When empty, the provisioned Static Web App origin is used.')
param apiCorsAllowedOrigins array = []

@description('PostgreSQL flexible server version.')
@allowed([
  '13'
  '14'
  '15'
  '16'
  '17'
])
param postgresVersion string = '17'

@description('PostgreSQL administrator login name.')
param postgresAdminLogin string = 'purchaseadmin'

@description('PostgreSQL administrator password.')
@secure()
@minLength(12)
param postgresAdminPassword string

@description('PostgreSQL SKU name.')
param postgresSkuName string = 'Standard_B1ms'

@description('PostgreSQL SKU tier.')
@allowed([
  'Burstable'
  'GeneralPurpose'
  'MemoryOptimized'
])
param postgresSkuTier string = 'Burstable'

@description('PostgreSQL storage size in GiB.')
@minValue(32)
param postgresStorageSizeGb int = 32

@description('Application database name.')
param databaseName string = 'purchase_order'

@description('Allow Azure-hosted services to reach PostgreSQL over its public endpoint. For stricter production networking, replace this with private networking.')
param postgresAllowAzureServices bool = true

@description('Additional PostgreSQL public firewall rules.')
param postgresFirewallRules array = []

var normalizedWorkloadName = toLower(replace(workloadName, '_', '-'))
var normalizedEnvironmentName = toLower(replace(environmentName, '_', '-'))
var compactWorkloadName = toLower(replace(replace('${normalizedWorkloadName}${normalizedEnvironmentName}', '-', ''), '_', ''))
var resourceToken = uniqueString(resourceGroup().id, normalizedWorkloadName, normalizedEnvironmentName)
var resourcePrefix = '${normalizedWorkloadName}-${normalizedEnvironmentName}'
var sharedTags = union(tags, {
  workload: workloadName
  environment: environmentName
})

var names = {
  apiContainerApp: '${resourcePrefix}-api'
  containerAppsEnvironment: '${resourcePrefix}-cae'
  containerRegistry: take('acr${compactWorkloadName}${resourceToken}', 50)
  keyVault: take('kv-${compactWorkloadName}-${resourceToken}', 24)
  logAnalyticsWorkspace: '${resourcePrefix}-log'
  managedIdentity: '${resourcePrefix}-aca-id'
  postgresServer: take('psql-${resourcePrefix}-${resourceToken}', 63)
  staticWebApp: '${resourcePrefix}-swa'
}

module frontend './modules/static-web-app.bicep' = {
  name: 'static-web-app'
  params: {
    location: staticWebAppLocation
    name: names.staticWebApp
    tags: sharedTags
  }
}

var apiCorsOrigins = empty(apiCorsAllowedOrigins) ? [
  frontend.outputs.origin
] : apiCorsAllowedOrigins

module observability './modules/observability.bicep' = {
  name: 'observability'
  params: {
    containerAppsEnvironmentName: names.containerAppsEnvironment
    location: location
    logAnalyticsWorkspaceName: names.logAnalyticsWorkspace
    tags: sharedTags
  }
}

module registry './modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    containerRegistryName: names.containerRegistry
    location: location
    managedIdentityName: names.managedIdentity
    tags: sharedTags
  }
}

module postgres './modules/postgres.bicep' = {
  name: 'postgres'
  params: {
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    allowAzureServices: postgresAllowAzureServices
    databaseName: databaseName
    firewallRules: postgresFirewallRules
    location: location
    serverName: names.postgresServer
    skuName: postgresSkuName
    skuTier: postgresSkuTier
    storageSizeGb: postgresStorageSizeGb
    tags: sharedTags
    version: postgresVersion
  }
}

var databaseConnectionString = 'Host=${postgres.outputs.hostName};Port=5432;Database=${databaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};Ssl Mode=Require;Trust Server Certificate=true;Maximum Pool Size=100;Timeout=60'

module secrets './modules/key-vault.bicep' = {
  name: 'key-vault'
  params: {
    databaseConnectionString: databaseConnectionString
    keyVaultName: names.keyVault
    location: location
    readerPrincipalId: registry.outputs.managedIdentityPrincipalId
    tags: sharedTags
  }
}

module api './modules/api-container-app.bicep' = {
  name: 'api-container-app'
  params: {
    apiContainerAppName: names.apiContainerApp
    apiCorsAllowedOrigins: apiCorsOrigins
    apiImage: apiImage
    apiTargetPort: apiTargetPort
    containerAppsEnvironmentId: observability.outputs.containerAppsEnvironmentId
    containerRegistryLoginServer: registry.outputs.containerRegistryLoginServer
    databaseConnectionSecretUri: secrets.outputs.databaseConnectionSecretUri
    location: location
    managedIdentityId: registry.outputs.managedIdentityId
    maxReplicas: apiMaxReplicas
    memory: apiMemory
    minReplicas: apiMinReplicas
    cpu: apiCpu
    tags: sharedTags
  }
}

output acrName string = registry.outputs.containerRegistryName
output apiBaseUrl string = api.outputs.apiBaseUrl
output apiContainerAppName string = api.outputs.apiContainerAppName
output keyVaultName string = secrets.outputs.keyVaultName
output postgresServerName string = postgres.outputs.serverName
output resourceGroupName string = resourceGroup().name
output staticWebAppName string = frontend.outputs.name
output staticWebAppUrl string = frontend.outputs.origin
output githubSecrets object = {
  ACR_NAME: registry.outputs.containerRegistryName
  API_BASE_URL: api.outputs.apiBaseUrl
  API_CONTAINER_APP: api.outputs.apiContainerAppName
  RESOURCE_GROUP: resourceGroup().name
  SWA_NAME: frontend.outputs.name
}
