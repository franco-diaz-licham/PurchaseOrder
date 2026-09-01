// ------------------------------------- Parameters -------------------------------------

@description('Azure region for the API Container App.')
param location string

@description('API Container App name.')
param apiContainerAppName string

@description('Container Apps environment resource ID.')
param containerAppsEnvironmentId string

@description('User-assigned managed identity resource ID.')
param managedIdentityId string

@description('Container registry login server.')
param containerRegistryLoginServer string

@description('Key Vault secret URI containing the API database connection string.')
param databaseConnectionSecretUri string

@description('API CORS allowed origin.')
param apiCorsAllowedOrigin string

@description('Tags applied to the API Container App.')
param tags object

// ------------------------------------- Variables -------------------------------------

var apiImage = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
var apiTargetPort = 8080
var minReplicas = 0
var maxReplicas = 1
var cpu = '0.25'
var memory = '0.5Gi'
var fixedEnvironmentVariables = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'ASPNETCORE_URLS'
    value: 'http://+:${apiTargetPort}'
  }
  {
    name: 'Database__PurchaseOrderDb'
    secretRef: 'purchase-order-db'
  }
]
var corsEnvironmentVariables = [
  {
    name: 'Cors__AllowedOrigins__0'
    value: apiCorsAllowedOrigin
  }
]

// ------------------------------------- Resources -------------------------------------

resource apiContainerApp 'Microsoft.App/containerApps@2026-01-01' = {
  name: apiContainerAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: apiTargetPort
        transport: 'auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistryLoginServer
          identity: managedIdentityId
        }
      ]
      secrets: [
        {
          name: 'purchase-order-db'
          keyVaultUrl: databaseConnectionSecretUri
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          env: concat(fixedEnvironmentVariables, corsEnvironmentVariables)
          resources: {
            cpu: json(cpu)
            memory: memory
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

// ------------------------------------- Outputs -------------------------------------

output apiBaseUrl string = 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
output apiContainerAppName string = apiContainerApp.name
