# Blazor.Msal

[![Build Status](https://des.visualstudio.com/Blazor.Msal/_apis/build/status/Blazor.Msal-CI?branchName=master)](https://des.visualstudio.com/Blazor.Msal/_build/latest?definitionId=48&branchName=master)

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Des.Blazor.Authorization.Msal)

This repository contains an example on how to integrate a Single Page Web Application made in Blazor WebAssembly with Azure Active Directory. It allows to authenticate the user and then acquire an access token to make a secure call to an external API:

![Diagram](https://github.com/cradle77/Blazor.Msal/blob/master/Diagram.png?raw=true)

The code internally uses [MSAL.js](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview) to implement the *OpenID Connect* and *OAuth2* flows.

I'll be publishing a few articles on this topic. [*Follow me on twitter*](https://twitter.com/crad77?lang=en) if you are interested and want to be notified when they are out. 

## How to run the example
The first step is registering the 2 applications in Azure Active Directory.

There are a couple of handy scripts to automatically create and clean them up in the `AppCreationScripts` folder.

Run the following in a PowerShell console:
```
.\Create.ps1 -Credential $credential -TenantId $tenantId
```

The `Credential` and `TenantId` parameters are optional, you will be prompted to authenticate against your Azure Active Directory tenant if not provided.

The script will also set the configuration files in the solution to interact with the applications registered.

## Build and run the code

The solution is currently based on .NET Core 3.1 Preview 2. I'll keep updating it when new preview versions become available.

If you have Visual Studio 2019 Preview and .NET Core 3.1 Preview 2 installed, it should compile and run just fine. 

Just make sure to execute both the `Blazor.Msal.Api` project and the `Blazor.Msal.Client` application.

Once the application is started, you will be able to login and grant consent to it by using the button at the upper right corner:

![Login](https://github.com/cradle77/Blazor.Msal/blob/master/Login.png?raw=true)

Then, if you navigate to the *Fetch Data* page, the application will automatically acquire an Access Token and use it to call the external API.

## Clean up your tenant's application registrations

After the applications have been created in Azure Active Directory and you have managed to run the example, you can clean them up just by running the following script:

```
.\Cleanup.ps1 -Credential $credential -TenantId $tenantId
```

## Bits worth looking at

This project provides a sample implementation of a Blazor `AuthenticationStateProvider` class which wraps around MSAL.js and integrates with Azure Active Directory.

`MsalAuthenticationStateProvider` can be used to:
- retrieve the authentication state of the current user
- execute a sign in
- execute a sign out
- acquire an access token for a given scope

A shared `LoginControl` component uses `AuthorizeView` to either display a Login button or a welcome message to the user.

The `blazor.azuread.js` file is a wrapper around the basic functionalities of MSAL.js so that they can be invoked from Blazor via JS Interop.

It requires some configuration bits in order to work (ClientId, Authority, ecc...). Instead of embedding them in the code, `App.razor` downloads them from a static file to simulate the injection of a configuration from a remote endpoint.

## Current limitations

The code has only been tested with the Popup authentication mode. 

*Please note that this only meant to be a Proof of Concept and it's not production-ready code*. If you want to use it in your business-critical application, do it at your own risk :grinning: