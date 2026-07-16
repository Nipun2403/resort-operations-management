@description('Deployment location')
param location string = 'centralindia'

@description('Unique suffix for resource names (e.g., demo1)')
param uniqueSuffix string

@description('PostgreSQL admin password')
@secure()
param pgAdminPassword string

@description('Your public IP for firewall (CIDR)')
param myPublicIP string

@description('Storage account key for nsdeply00')
@secure()
param storageAccountKey string

@description('Groq API key (OpenAI-compatible)')
@secure()
param openRouterKey string

@description('Gmail SMTP app password')
@secure()
param emailSmtpPass string

@description('JWT signing key (base64, 64 chars)')
@secure()
param jwtKey string

@description('Web PubSub connection string')
@secure()
param webPubSubConnectionString string

// ─── Deploy Modules ───

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics-${uniqueSuffix}'
  params: {
    name: 'log-hotel-mgmt-${uniqueSuffix}'
    location: location
  }
}

module containerAppsEnv 'modules/container-apps-env.bicep' = {
  name: 'ca-env-${uniqueSuffix}'
  params: {
    name: 'env-hotel-mgmt-${uniqueSuffix}'
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
    logAnalyticsWorkspaceKey: logAnalytics.outputs.workspaceKey
  }
}

module acr 'modules/acr.bicep' = {
  name: 'acr-${uniqueSuffix}'
  params: {
    name: 'acrhotelmgmt${uniqueSuffix}'
    location: location
  }
}

module webPubSub 'modules/web-pubsub.bicep' = {
  name: 'webpubsub-${uniqueSuffix}'
  params: {
    name: 'wps-hotel-mgmt-${uniqueSuffix}'
    location: location
  }
}

module postgresql 'modules/postgresql.bicep' = {
  name: 'postgresql-${uniqueSuffix}'
  params: {
    serverName: 'pg-hotel-mgmt-${uniqueSuffix}'
    location: location
    adminLogin: 'pgadmin'
    adminPassword: pgAdminPassword
    myPublicIP: myPublicIP
  }
}

module backendApp 'modules/backend-app.bicep' = {
  name: 'backend-app-${uniqueSuffix}'
  params: {
    name: 'hotel-api-${uniqueSuffix}'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    acrName: acr.outputs.acrName
    acrLoginServer: acr.outputs.acrLoginServer
    acrPassword: acr.outputs.acrPassword
    pgFqdn: postgresql.outputs.serverFqdn
    pgAdminPassword: pgAdminPassword
    storageAccountKey: storageAccountKey
    openRouterKey: openRouterKey
    emailSmtpPass: emailSmtpPass
    jwtKey: jwtKey
    webPubSubConnectionString: webPubSubConnectionString
  }
}

module frontendApp 'modules/frontend-app.bicep' = {
  name: 'frontend-app-${uniqueSuffix}'
  params: {
    name: 'hotel-web-${uniqueSuffix}'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    acrName: acr.outputs.acrName
    acrLoginServer: acr.outputs.acrLoginServer
    acrPassword: acr.outputs.acrPassword
    apiBaseUrl: 'https://hotel-api-${uniqueSuffix}.azurecontainerapps.io/api/v1'
    signalrHubUrl: 'https://hotel-api-${uniqueSuffix}.azurecontainerapps.io/notifications'
    webPubSubEndpoint: 'https://wps-hotel-mgmt-${uniqueSuffix}.webpubsub.azure.com'
  }
}

module jobs 'modules/jobs.bicep' = {
  name: 'jobs-${uniqueSuffix}'
  params: {
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    acrName: acr.outputs.acrName
    acrLoginServer: acr.outputs.acrLoginServer
    acrPassword: acr.outputs.acrPassword
    pgFqdn: postgresql.outputs.serverFqdn
    pgAdminPassword: pgAdminPassword
    storageAccountKey: storageAccountKey
    storageAccountName: 'nsdeply00'
  }
}

// ─── Outputs ───
output backendUrl string = 'https://hotel-api-${uniqueSuffix}.azurecontainerapps.io'
output frontendUrl string = 'https://hotel-web-${uniqueSuffix}.azurecontainerapps.io'
output acrName string = acr.outputs.acrName
output acrLoginServer string = acr.outputs.acrLoginServer