# Input bindings are passed in via param block.
param($Timer)

# Get the current universal time in the default string format
$currentUTCtime = (Get-Date).ToUniversalTime()

# The 'IsPastDue' porperty is 'true' when the current function invocation is later than scheduled.
if ($Timer.IsPastDue) {
    Write-Host "PowerShell timer is running late!"
}

try 
{
    $uri = "https://api.hubapi.com/oauth/v1/token"
    $keyVaultName = "[replace_with_your_key_vault_name]"
    $keyVaultHubSpotSecretTokenName = "[replace_with_your_key_vault_hub_spot_oauth_token_secret_name]"
    $clientId = "[replace_with_your_clientId]"
    $clientSecret = "[replace_with_your_client_secret]"
    $refreshToken = "[replace_with_your_refresh_token]"

    $currentAuthToken = Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $keyVaultHubSpotSecretTokenName

    Write-Host "Current Token: $currentAuthToken"

    

    $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]";
    $headers.Add("Content-Type", "application/x-www-form-urlencoded");

    $body = "grant_type=refresh_token&client_id=$clientId&client_secret=$clientSecret&refresh_token=$refreshToken"

    $response = Invoke-RestMethod -Method Post -Uri $uri -Body $body -Headers $headers

    Write-Host "New Token: $($response.access_token)"

    $secret = ConvertTo-SecureString -String "$($response.access_token)" -AsPlainText -Force
    Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $keyVaultHubSpotSecretTokenName -SecretValue $secret

    Write-Host "New Token set in Key Vault."

    # Write an information log with the current time.
    Write-Host "PowerShell timer trigger function ran! TIME: $currentUTCtime"
}
catch
{
    Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
        Headers = @{'Content-Type' = 'text/xml'}
        StatusCode =  [HttpStatusCode]::InternalServerError
        Body = $body 
    })
}
