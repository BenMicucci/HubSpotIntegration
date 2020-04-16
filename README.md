# HubSpotIntegration

The purpose of this repo is to help fellow developers user the OAuth2.0 authentication mechanism. There are two components:

1. Refresh-HubSpotAuthenticationToken.ps1 

  This code is generated so users can place this in an Azure Function that is ran on a timer and it will automatically refresh your authentication token and place it in a preexisting Azure Key Vault. In order to make this work, you need to do a few things:
  * Create a new private Azure Key Vault. There needs to be one secret where the OAuth 2.0 token is stored
  * Manually generate an OAuth 2.0 token within HubSpot. Reference their docs to create this token. Make note of your clientId, clientSecret, & refresh token. 
  * Create your Azure Function with a PowerShell base & timer interval. Replace the values inside of the PowerShell with your actual values where the variables are defined with "[replace..."
  * Make sure your Azure Function has access to the Key Vault
  
2. HubSpotExample.cs 
  
  This code is simply and example on how to call their Company & Contact API endpoints. It retreives thus auth token from KeyVault and makes the API calls. You need to make sure that where you are running this code, it has access to the Azure Key Vault. There are some constants you need to replace in this file as well.
