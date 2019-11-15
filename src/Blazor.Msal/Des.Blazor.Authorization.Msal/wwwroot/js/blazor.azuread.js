function AzureAd() {
    AzureAd.prototype.initialize = function (msalConfig, authenticationManager) {
        this.msalObj = new Msal.UserAgentApplication(msalConfig);
        this.msalObj.handleRedirectCallback(
            (e, r) => this.handleRedirectCallback(authenticationManager, e, r));
    };

    AzureAd.prototype.handleRedirectCallback = function (authenticationManager, e, r) {
        if (r && r.accountState) {
            authenticationManager.invokeMethodAsync("RedirectToSourceUrl", r.accountState);
        }
    };

    AzureAd.prototype.signInRedirect = function (scopes) {
        var requestObj = {
            scopes: scopes,
            state: window.location.href
        };

        return this.msalObj.loginRedirect(requestObj);
    };

    AzureAd.prototype.signInPopup = function (scopes) {
        var requestObj = {
            scopes: scopes,
        };

        return this.msalObj.loginPopup(requestObj)
            .catch(function (error) {
                // we ignore errors due to user cancelling or
                // non granting consent. User had not logged in
                // but we don't want to raise an exception
                if (error.errorCode !== "user_cancelled" &&
                    error.errorCode !== "consent_required") {
                    throw error;
                }
            });
    };

    AzureAd.prototype.signOut = function () {
        return this.msalObj.logout();
    };

    AzureAd.prototype.getAccount = function () {
        return this.msalObj.getAccount();
    };

    AzureAd.prototype.acquireTokenRedirect = function (scopes) {
        var requestObj = {
            scopes: scopes
        };

        var self = this;

        return self.msalObj.acquireTokenSilent(requestObj)
            .catch(function (error) {
                console.log(error);
                return self.msalObj.acquireTokenRedirect(requestObj).catch(function (error) {
                    console.log(error);
                });
            });
    };

    AzureAd.prototype.acquireTokenPopup = function (scopes) {
        var requestObj = {
            scopes: scopes
        };

        var self = this;

        return self.msalObj.acquireTokenSilent(requestObj)
            .catch(function (error) {
                console.log(error);
                return self.msalObj.acquireTokenPopup(requestObj).catch(function (error) {
                    console.log(error);
                });
            });
    };
}

window.azuread = new AzureAd();