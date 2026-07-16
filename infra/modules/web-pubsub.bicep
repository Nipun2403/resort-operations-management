@description('Web PubSub name')
param name string

@description('Location')
param location string = resourceGroup().location

resource wps 'Microsoft.SignalRService/webPubSub@2023-02-01' = {
  name: name
  location: location
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    capacity: 1
  }
  properties: {}
}