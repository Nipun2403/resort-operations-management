@description('Frontend Container App name')
param name string

@description('Location')
param location string = resourceGroup().location

@description('Container Apps Environment ID')
param environmentId string

@description('ACR name')
param acrName string

@description('ACR login server')
param acrLoginServer string

@description('ACR admin password')
@secure()
param acrPassword string

@description('Web PubSub endpoint')
param webPubSubEndpoint string

@description('Backend API URL')
param apiBaseUrl string

@description('SignalR Hub URL')
param signalrHubUrl string

resource app 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      registries: [{
        server: acrLoginServer
        username: acrName
        passwordSecretRef: 'acr-password'
      }]
      secrets: [
        { name: 'acr-password', value: acrPassword }
      ]
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
        transport: 'auto'
      }
    }
    template: {
      containers: [{
        name: 'frontend'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
        env: [
          { name: 'API_BASE_URL', value: apiBaseUrl }
          { name: 'SIGNALR_HUB_URL', value: signalrHubUrl }
          { name: 'WEB_PUBSUB_ENDPOINT', value: webPubSubEndpoint }
        ]
      }]
      scale: {
        minReplicas: 1
        maxReplicas: 3
        rules: [{
          name: 'http-rule'
          http: {
            metadata: {
              concurrentRequests: '100'
            }
          }
        }]
      }
    }
  }
}