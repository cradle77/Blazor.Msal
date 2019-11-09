[CmdletBinding()]
    param(
        [PSCredential] $Credential,
        [string] $tenantId
    )

$ErrorActionPreference = "Stop"

if ($Credential -and $tenantId){
    Connect-AzureAD -Credential $Credential -TenantId $tenantId  
} elseif (!$Credential -and $tenantId) {
    Connect-AzureAD -TenantId $tenantId
} elseif ($Credential -and !$tenantId) {
    Connect-AzureAD -Credential $Credential
} else {
    Connect-AzureAD
}

$serverApp = Get-AzureADApplication -Filter "IdentifierUris eq 'app://blazor.msal.api'"

if ($serverApp) {
    Remove-AzureADApplication -ObjectId $serverApp.ObjectId
}

$clientApp = Get-AzureADApplication -Filter "IdentifierUris eq 'app://blazor.msal.client'"

if ($clientApp) {
    Remove-AzureADApplication -ObjectId $clientApp.ObjectId
}