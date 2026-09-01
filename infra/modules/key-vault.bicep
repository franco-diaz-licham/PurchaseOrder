// ------------------------------------- Parameters -------------------------------------

@description('Azure region for Key Vault.')
param location string

@description('Key Vault name.')
param keyVaultName string

@description('Principal ID that can read application secrets.')
param readerPrincipalId string

@description('Database connection string stored as a Key Vault secret.')
@secure()
param databaseConnectionString string

@description('Tags applied to Key Vault resources.')
param tags object

// ------------------------------------- Resources -------------------------------------

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enableRbacAuthorization: true
    enableSoftDelete: true
    enabledForTemplateDeployment: true
    publicNetworkAccess: 'Enabled'
    softDeleteRetentionInDays: 7
  }
}

resource databaseConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'PurchaseOrderDbConnectionString'
  parent: keyVault
  properties: {
    value: databaseConnectionString
  }
}

resource keyVaultSecretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, readerPrincipalId, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: readerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ------------------------------------- Outputs -------------------------------------

output databaseConnectionSecretUri string = databaseConnectionSecret.properties.secretUri
output keyVaultName string = keyVault.name
output keyVaultSecretsUserRoleAssignmentId string = keyVaultSecretsUserRole.id
