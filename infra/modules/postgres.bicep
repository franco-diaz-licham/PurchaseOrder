@description('Azure region for PostgreSQL.')
param location string

@description('PostgreSQL flexible server name.')
param serverName string

@description('Application database name.')
param databaseName string

@description('PostgreSQL administrator login name.')
param administratorLogin string

@description('PostgreSQL administrator password.')
@secure()
param administratorLoginPassword string

@description('PostgreSQL flexible server version.')
param version string

@description('PostgreSQL SKU name.')
param skuName string

@description('PostgreSQL SKU tier.')
param skuTier string

@description('PostgreSQL storage size in GiB.')
param storageSizeGb int

@description('Allow Azure-hosted services to reach PostgreSQL over its public endpoint.')
param allowAzureServices bool

@description('Additional PostgreSQL public firewall rules.')
param firewallRules array

@description('Tags applied to PostgreSQL resources.')
param tags object

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: serverName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
    storage: {
      storageSizeGB: storageSizeGb
    }
    version: version
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  name: databaseName
  parent: postgresServer
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource allowAzureServicesFirewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = if (allowAzureServices) {
  name: 'AllowAzureServices'
  parent: postgresServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource additionalFirewallRules 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = [
  for rule in firewallRules: {
    name: rule.name
    parent: postgresServer
    properties: {
      startIpAddress: rule.startIpAddress
      endIpAddress: rule.endIpAddress
    }
  }
]

output databaseName string = database.name
output hostName string = '${postgresServer.name}.postgres.database.azure.com'
output serverName string = postgresServer.name
