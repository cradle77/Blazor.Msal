# Des.Blazor.Authorization.Msal

[![Build Status](https://des.visualstudio.com/Blazor.Msal/_apis/build/status/Blazor.Msal-CI?branchName=master)](https://des.visualstudio.com/Blazor.Msal/_build/latest?definitionId=48&branchName=master)

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Des.Blazor.Authorization.Msal)

> _Updated to Blazor .NET Core 3.1 Preview 4_

This NuGet package adds support to Azure Active Directory to your Blazor Web Assembly application. It allows  to authenticate the user and then acquire an access token to make a secure call to an external API:

![Diagram](https://github.com/cradle77/Blazor.Msal/blob/master/Diagram.png?raw=true)

The code internally uses [MSAL.js](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview) to implement the *OpenID Connect* and *OAuth2* flows.

I'll be publishing a few articles on this topic. [*Follow me on twitter*](https://twitter.com/crad77?lang=en) if you are interested and want to be notified when they are out. 

## How to use the package

### 1. Add the required references

The first step is to reference the NuGet package on the *Blazor Client* project:

```
Install-Package Des.Blazor.Authorization.Msal
```

Then we need to include MSAL.js in *index.html* page, before the reference to blazor.webassembly.js *and the script from the component library*. This could either be a static file we include to our project, or just a link pointing to a CDN URL:

```
    ...
    <script src="https://alcdn.msftauth.net/lib/1.1.3/js/msal.js"></script>
    <script src="_content/Des.Blazor.Authorization.Msal/js/blazor.azuread.js"></script>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
```

> _*Note*: this has changed since Preview2: automatic script embedding doesn't work anymore, please remember to add the script reference manually_

### 2. Register the services and configure the authentication

At this point we are ready to configure the authentication in our *Startup* class, by adding the following lines to the *ConfigureServices* method:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthorizationCore();
    services.AddAzureActiveDirectory(myConfig);
}
```

We have a few options here on how to configure the authentication. We can:
- pass an object that implements the `IMsalConfig` interface:
```
services.AddAzureActiveDirectory(myConfig);
```
- use a Url to a Json file that deserializes into the same interface. This is a quite simple way to not hardcode your configuration parameters and having been fetched dynamically when needed, either from a static file or from an API endpoint:
```
services.AddAzureActiveDirectory(new Uri('config/config.json', UriKind.Relative));
```
- provide an async function that returns the config:
```
services.AddAzureActiveDirectory(async sp => 
{
    var http = sp.GetService<HttpClient>();

    var config = await http.GetJsonAsync<ClientConfig>($"/config/appsettings.json?{DateTime.Now.Ticks}");

    return (IMsalConfig)config;
});
```

The content of `IMsalConfig` is the following:
```
public interface IMsalConfig
{
    string ClientId { get; }
    string Authority { get; }
    LoginModes LoginMode { get; }
}
```
The parameters are self-explanatory:
- `ClientId` is the `AppId` registered in Azure Active Directory.
- `Authority` is the URL of the Tenant, in the `https://login.microsoftonline.com/{tenantId}` form
- `LoginMode` can either be `Popup` or `Redirect` depending on how we want to display the Azure AD interfaces.

### 3. Add the authentication capabilities to your Blazor application
Once all the services are in place, we can start adding the support to authenticating users to our Blazor pages. This relies on a main component, called `CascadingAuthenticationState`, which you usually place in the `App.razor` file:

```
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

Thanks to this, we can start building *authentication-aware* components. One first example could be a `LoginStatus` one, which we might put in the *Shared* folder and use in the `MainLayout`:

```
@inject IAuthenticationManager AuthenticationManager

<AuthorizeView>
    <NotAuthorized>
        <button class="btn btn-primary" 
            @onclick="this.AuthenticationManager.SignInAsync">Login</button>
    </NotAuthorized>
    <Authorized>
        <span class="mr-5">Welcome <b>@context.User.Identity.Name</b></span>
        <button class="btn btn-primary"
            @onclick="this.AuthenticationManager.SignOutAsync">Logout</button>
    </Authorized>
</AuthorizeView>
```

There are quite a few interesting aspect in the code snippet above:
- We want to display different content depending on whether the uesr is authenticated or not. In order to do that, we leverage `AuthorizeView` and provide two subtemplates for the `Authorized` and `NotAuthorized` cases;
- When `NotAuthorized`, we display a Login button which calls a `SignInAsync` method of the `IAuthenticationManager`. This is one of the services injected by `AddAzureActiveDirectory` and provides primitives to *Sign in*, *Sign out* and request *Access Tokens* for the user. 
- When `Authorized`, we use the current context to display a welcome message to the user. `IAuthenticationManager` implements all the logic needed to translate the *IdToken* retrieved from *Azure Active Directory* into a .NET *ClaimsPrincipal* object. This means that we can also inspect the claims and retrieve information around the *user email address*, its *ObjectID*, the *application roles* he belongs to, etc.

This is all we need to do to implement a *Login flow* in our Blazor WebAssembly application.

### 4. Acquire an access token for an endpoint
If we want to call an API which is protected by *OAuth*, we need to retrieve a *Access Token* and pass it in the Authorization header.

We can easily do it thorugh the `IAuthenticationManager` service. Let's have a look at the ![FetchData.razor](https://github.com/cradle77/Blazor.Msal/blob/master/src/Sample/Blazor.Msal.Client/Pages/FetchData.razor) component in the sample application, starting from the markup:

```
@page "/fetchdata"
@inject HttpClient Http
@inject IAuthenticationManager AuthenticationManager

<h1>Weather forecast</h1>

<p>This component demonstrates fetching data from the server.</p>

<AuthorizeView>
    <Authorized>
        <table class="table">
            <thead>
              ....
    </Authorized>
    <NotAuthorized>
        Please <LoginStatus></LoginStatus> before accessing this page
    </NotAuthorized>
</AuthorizeView>
```

Here we are also using `AuthorizeView` to prompt the user to sign in if not authenticated, whilst showing the usual weather forecast grid in case `Authorized`.

The interesting bits are in the code section:

```
@code {
    private string _apiScope = "app://blazor.msal.api/api.access";
    private WeatherForecast[] _forecasts;

    [CascadingParameter]
    private Task<AuthenticationState> authenticationStateTask { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        var state = await this.authenticationStateTask;
        if (!state.User.Identity.IsAuthenticated)
        {
            return;
        }

        var token = await this.AuthenticationManager.GetAccessTokenAsync(
            // passing the list of requested scopes
            _apiScope);

        this.Http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

        _forecasts = await Http.GetJsonAsync<WeatherForecast[]>("https://localhost:5101/weatherforecast");
    }
}
```

If the user is *not authenticated*, we don't want to run any fetch logic, as we wouldn't be able to retrieve a valid *access tokent*. Therefore:
- we are using a `CascadingParameter` to retrieve the current *authentication state* and proceed only if the user *IsAuthenticated*;
- we are doing this within `OnParameterSetAsync`, instead of `OnInitializedAsync`. There is a subtle difference between the two: `OnInitializedAsync` is only executed once, whereas here we do want to re-evaluate the method should the authentication state change. That's why we have used the former;
- we then call `GetAccessTokenAsync` of the *Authentication Manager* to retrieve an access token for the given scope;
- we pass that *Bearer token8 in the *Authorization header*.

## How to run the example
The sample code requires 2 applications to be registered in Azure Active Directory: one for the Blazor.Client application, one for the Api.

There are a couple of handy scripts to automatically create and clean them up in the `AppCreationScripts` folder.

Run the following in a PowerShell console:
```
.\Create.ps1 -Credential $credential -TenantId $tenantId
```

The `Credential` and `TenantId` parameters are optional, you will be prompted to authenticate against your Azure Active Directory tenant if not provided.

The script will also set the configuration files in the solution to interact with the applications registered.

## Build and run the code

The solution is currently based on .NET Core 3.1 Preview 3. I'll keep updating it when new preview versions become available.

If you have Visual Studio 2019 Preview and .NET Core 3.1 Preview 3 installed, it should compile and run just fine. 

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