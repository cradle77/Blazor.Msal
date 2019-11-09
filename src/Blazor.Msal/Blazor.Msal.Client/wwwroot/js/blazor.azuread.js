function AzureAd() {
    AzureAd.prototype.initialize = function (msalConfig) {
        this.msalObj = new Msal.UserAgentApplication(msalConfig);
    };

    AzureAd.prototype.signIn = function () {
        return this.msalObj.loginPopup({});
    };

    AzureAd.prototype.signOut = function () {
        return this.msalObj.logout();
    };

    AzureAd.prototype.getAccount = function () {
        return this.msalObj.getAccount();
    };

    AzureAd.prototype.acquireToken = function (scopes) {
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