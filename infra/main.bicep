targetScope = 'resourceGroup'

@description('Azure region for regional resources.')
param location string

@description('Azure region for Static Web Apps. Static Web Apps Free is used, but this resource type is not currently available in Australia regions.')
param staticWebAppLocation string

@description('Short workload name used in Azure resource names.')
param workloadName string

@description('Environment label used in Azure resource names and tags.')
param environmentName string

@description('Tags applied to all Azure resources.')
param tags object

@description('PostgreSQL administrator login name.')
param postgresAdminLogin string

@description('PostgreSQL administrator password.')
@secure()
@minLength(12)
param postgresAdminPassword string

@description('Allow Azure-hosted services to reach PostgreSQL over its public endpoint. For stricter production networking, replace this with private networking.')
param postgresAllowAzureServices bool

@description('Additional PostgreSQL public firewall rules.')
param postgresFirewallRules array

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
    firewallRules: postgresFirewallRules
    location: location
    serverName: names.postgresServer
    tags: sharedTags
  }
}

var databaseConnectionString = 'Host=${postgres.outputs.hostName};Port=5432;Database=${postgres.outputs.databaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};Ssl Mode=Require;Trust Server Certificate=true;Maximum Pool Size=100;Timeout=60'

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
    apiCorsAllowedOrigin: frontend.outputs.origin
    containerAppsEnvironmentId: observability.outputs.containerAppsEnvironmentId
    containerRegistryLoginServer: registry.outputs.containerRegistryLoginServer
    databaseConnectionSecretUri: secrets.outputs.databaseConnectionSecretUri
    location: location
    managedIdentityId: registry.outputs.managedIdentityId
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
