@description('Backend Container App name')
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

@description('PostgreSQL FQDN')
param pgFqdn string

@description('PostgreSQL admin password')
@secure()
param pgAdminPassword string

@description('Storage account key')
@secure()
param storageAccountKey string

@description('Groq API key')
@secure()
param openRouterKey string

@description('Gmail SMTP app password')
@secure()
param emailSmtpPass string

@description('JWT signing key')
@secure()
param jwtKey string

@description('Web PubSub connection string')
@secure()
param webPubSubConnectionString string

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
        { name: 'jwt-key', value: jwtKey }
        { name: 'openrouter-key', value: openRouterKey }
        { name: 'email-smtp-pass', value: emailSmtpPass }
        { name: 'db-password', value: pgAdminPassword }
        { name: 'storage-key', value: storageAccountKey }
        { name: 'webpubsub-connection', value: webPubSubConnectionString }
      ]
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        transport: 'auto'
      }
    }
    template: {
      containers: [{
        name: 'backend'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        command: ['dotnet', 'HotelManagement.API.dll']
        resources: { cpu: json('0.5'), memory: '1Gi' }
        env: [
          { name: 'ASPNETCORE_ENVIRONMENT', value: 'Development' }
          { name: 'ASPNETCORE_URLS', value: 'http://+:8080' }
          { name: 'Jwt__Key', secretRef: 'jwt-key' }
          { name: 'Jwt__Issuer', value: 'HotelManagementAPI' }
          { name: 'Jwt__Audience', value: 'HotelManagementClients' }
          { name: 'OpenAI__ApiKey', secretRef: 'openrouter-key' }
          { name: 'OpenAI__Endpoint', value: 'https://api.groq.com/openai/v1' }
          { name: 'OpenAI__Model', value: 'openai/gpt-oss-20b' }
          { name: 'OpenAI__MaxTokens', value: '6000' }
          { name: 'OpenAI__Temperature', value: '0.1' }
          { name: 'Email__SmtpHost', value: 'smtp.gmail.com' }
          { name: 'Email__SmtpPort', value: '465' }
          { name: 'Email__SmtpUser', value: 'king26devil@gmail.com' }
          { name: 'Email__SmtpPass', secretRef: 'email-smtp-pass' }
          { name: 'Email__SenderEmail', value: 'king26devil@gmail.com' }
          { name: 'Email__SenderName', value: 'Aetheris' }
          { name: 'AzureStorage__AccountUrl', value: 'https://nsdeply00.blob.${environment().suffixes.storage}' }
          { name: 'AzureStorage__AccountKey', secretRef: 'storage-key' }
          { name: 'AzureStorage__ContainerName', value: 'images' }
          { name: 'AzureStorage__QueueName', value: 'image-validation-queue' }
          { name: 'AzureStorage__SasExpiryMinutes', value: '15' }
          { name: 'AzureStorage__MaxSizeBytes', value: '10485760' }
          { name: 'AzureStorage__MaxImagesPerRoomType', value: '5' }
          { name: 'ConnectionStrings__DefaultConnection', value: 'Host=${pgFqdn};Database=HotelManagement;Username=pgadmin;Password=${pgAdminPassword};SSL Mode=Require;Trust Server Certificate=true' }
          { name: 'WebPubSub__ConnectionString', secretRef: 'webpubsub-connection' }
          { name: 'AllowedHosts', value: '*' }
          { name: 'AllowedOrigins__0', value: 'https://hotel-web-${uniqueSuffix}.ambitiousmushroom-274454dc.centralindia.azurecontainerapps.io' }
          { name: 'AllowedOrigins__1', value: 'http://localhost:4200' }
          { name: 'AllowedOrigins__2', value: 'http://localhost:4201' }
        ]
      }]
      scale: {
        minReplicas: 1
        maxReplicas: 3
        rules: [{
          name: 'http-rule'
          http: {
            metadata: {
              concurrentRequests: '50'
            }
          }
        }]
      }
    }
  }
}