[CmdletBinding()]
    param(
        [PSCredential] $Credential,
        [string] $tenantId
    )

$ErrorActionPreference = "Stop"

if ($Credential -and $tenantId){
    $auth = Connect-AzureAD -Credential $Credential -TenantId $tenantId
} elseif (!$Credential -and $tenantId) {
    $auth = Connect-AzureAD -TenantId $tenantId
} elseif ($Credential -and !$tenantId) {
    $auth = Connect-AzureAD -Credential $Credential
} else {
    $auth = Connect-AzureAD
}

$tenantId = $auth.TenantId
$tenantDomain = $auth.TenantDomain

# create Api application
$oauth2Permissions = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]

$oauth2Permission = New-Object Microsoft.Open.AzureAD.Model.OAuth2Permission
$oauth2Permission.AdminConsentDescription = "Allow access to the api"
$oauth2Permission.AdminConsentDisplayName = "Api Access"
$oauth2Permission.UserConsentDescription = "Allow access to the api"
$oauth2Permission.UserConsentDisplayName = "Api Access"
$oauth2Permission.Type = "User"
$oauth2Permission.Value = "api.access"
$oauth2Permission.Id = [System.Guid]::NewGuid()
$oauth2Permissions.Add($oauth2Permission)

$apiApplication = New-AzureADApplication -DisplayName "Blazor.Msal.Api" `
    -IdentifierUris "app://blazor.msal.api" `
    -PublicClient $false `
    -Oauth2Permissions $oauth2Permissions

"Api application created:"
$apiApplication | Select-Object -Property ObjectId, AppId, DisplayName


# create client application
$requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]

$requiredAccess = New-Object Microsoft.Open.AzureAD.Model.RequiredResourceAccess
$requiredAccess.ResourceAppId = $apiApplication.AppId

$requiredAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]
$resourceAccess = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess
$resourceAccess.Type = "Scope"
$resourceAccess.Id = $oauth2Permission.Id # api.access
$requiredAccess.ResourceAccess.Add($resourceAccess)

$requiredResourcesAccess.Add($requiredAccess)

$clientApplication = New-AzureADApplication -DisplayName "Blazor.Msal.Client" `
    -Oauth2AllowImplicitFlow $true `
    -Homepage "https://localhost:5001/" `
    -ReplyUrls "https://localhost:5001/" `
    -IdentifierUris "app://blazor.msal.client" `
    -RequiredResourceAccess $requiredResourcesAccess

"Client application created:"
$clientApplication | Select-Object -Property ObjectId, AppId, DisplayName

# set configuration for the server application
$apiSettings = Get-Content "..\Blazor.Msal.Api\appsettings.json" | ConvertFrom-Json
$apiSettings | Add-Member -Force -MemberType NoteProperty -Name AzureAD -Value @{
    "Instance" = "https://login.microsoftonline.com/"
    "Domain" = $tenantDomain
    "TenantId" = $tenantId
    "ClientId" = $apiApplication.IdentifierUris[0]
}

Set-Content "..\Blazor.Msal.Api\appsettings.json" -Value ($apiSettings | ConvertTo-Json -Depth )

# set configuration for the client application
$clientSettings = @{
    ClientId = $clientApplication.AppId
    Authority = "https://login.microsoftonline.com/$tenantId"
    ApiScope = "app://blazor.msal.api/api.access"
}

Set-Content "..\Blazor.Msal.Client\wwwroot\config\appsettings.json" -Value ($clientSettings | ConvertTo-Json)