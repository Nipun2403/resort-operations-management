@description('ACR name (globally unique)')
param name string

@description('Location')
param location string = resourceGroup().location

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: name
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
  }
}

output acrName string = acr.name
output acrLoginServer string = acr.properties.loginServer
#disable-next-line outputs-should-not-contain-secrets
output acrPassword string = acr.listCredentials().passwords[0].value