@description('Log Analytics workspace name')
param name string

@description('Location')
param location string = resourceGroup().location

resource workspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: name
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

output workspaceId string = workspace.properties.customerId
#disable-next-line outputs-should-not-contain-secrets
output workspaceKey string = workspace.listKeys().primarySharedKey