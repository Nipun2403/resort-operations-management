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

@description('Storage account name')
param storageAccountName string = 'nsdeply00'

// Common secrets for all jobs
var commonSecrets = [
  { name: 'storage-connection-string', value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccountKey};EndpointSuffix=${environment().suffixes.storage}' }
  { name: 'db-connection', value: 'Host=${pgFqdn};Database=HotelManagement;Username=pgadmin;Password=${pgAdminPassword};SSL Mode=Require;Trust Server Certificate=true' }
  { name: 'acr-password', value: acrPassword }
  { name: 'storage-key', value: storageAccountKey }
]

var commonRegistry = [{
  server: acrLoginServer
  username: acrName
  passwordSecretRef: 'acr-password'
}]

var commonContainerEnv = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'AzureStorage__AccountUrl', value: 'https://${storageAccountName}.blob.${environment().suffixes.storage}' }
  { name: 'AzureStorage__AccountKey', secretRef: 'storage-key' }
  { name: 'AzureStorage__ContainerName', value: 'images' }
  { name: 'AzureStorage__QueueName', value: 'image-validation-queue' }
  { name: 'ConnectionStrings__DefaultConnection', secretRef: 'db-connection' }
]

// ─── Image Validation Worker (Event-driven, Queue) ───
resource imageValidationJob 'Microsoft.App/jobs@2023-05-01' = {
  name: 'image-validation-job'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: 'Event'
      replicaTimeout: 1800
      eventTriggerConfig: {
        replicaCompletionCount: 1
        parallelism: 1
        scale: {
          minExecutions: 0
          maxExecutions: 10
          rules: [{
            name: 'queue-rule'
            type: 'azure-queue'
            metadata: {
              queueName: 'image-validation-queue'
              queueLength: '1'
            }
            auth: [{
              secretRef: 'storage-connection-string'
              triggerParameter: 'connection'
            }]
          }]
        }
      }
      registries: commonRegistry
      secrets: commonSecrets
    }
    template: {
      containers: [{
        name: 'worker'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        command: ['dotnet', 'HotelManagement.API.dll', '--worker', 'ImageValidation']
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
        env: commonContainerEnv
      }]
    }
  }
}

// ─── Orphan Cleanup Worker (Daily 02:00 UTC) ───
resource orphanCleanupJob 'Microsoft.App/jobs@2023-05-01' = {
  name: 'orphan-cleanup-job'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 1800
      scheduleTriggerConfig: {
        cronExpression: '0 2 * * *'
        replicaCompletionCount: 1
        parallelism: 1
      }
      registries: commonRegistry
      secrets: commonSecrets
    }
    template: {
      containers: [{
        name: 'worker'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        command: ['dotnet', 'HotelManagement.API.dll', '--worker', 'OrphanCleanup']
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
        env: commonContainerEnv
      }]
    }
  }
}

// ─── Blob Cleanup Worker (Hourly) ───
resource blobCleanupJob 'Microsoft.App/jobs@2023-05-01' = {
  name: 'blob-cleanup-job'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 1800
      scheduleTriggerConfig: {
        cronExpression: '0 * * * *'
        replicaCompletionCount: 1
        parallelism: 1
      }
      registries: commonRegistry
      secrets: commonSecrets
    }
    template: {
      containers: [{
        name: 'worker'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        command: ['dotnet', 'HotelManagement.API.dll', '--worker', 'BlobCleanup']
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
        env: commonContainerEnv
      }]
    }
  }
}

// ─── Proposal Cleanup Worker (Every 5 min) ───
resource proposalCleanupJob 'Microsoft.App/jobs@2023-05-01' = {
  name: 'proposal-cleanup-job'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 1800
      scheduleTriggerConfig: {
        cronExpression: '*/5 * * * *'
        replicaCompletionCount: 1
        parallelism: 1
      }
      registries: commonRegistry
      secrets: commonSecrets
    }
    template: {
      containers: [{
        name: 'worker'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        command: ['dotnet', 'HotelManagement.API.dll', '--worker', 'ProposalCleanup']
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
        env: commonContainerEnv
      }]
    }
  }
}

// ─── Idempotency Cleanup Worker (Every 1 min) ───
resource idempotencyCleanupJob 'Microsoft.App/jobs@2023-05-01' = {
  name: 'idempotency-cleanup-job'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 1800
      scheduleTriggerConfig: {
        cronExpression: '* * * * *'
        replicaCompletionCount: 1
        parallelism: 1
      }
      registries: commonRegistry
      secrets: commonSecrets
    }
    template: {
      containers: [{
        name: 'worker'
        image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        command: ['dotnet', 'HotelManagement.API.dll', '--worker', 'IdempotencyCleanup']
        resources: { cpu: json('0.25'), memory: '0.5Gi' }
        env: commonContainerEnv
      }]
    }
  }
}
