function AzureAd() {
    AzureAd.prototype.initialize = function (msalConfig, authenticationManager) {
        this.authenticationManager = authenticationManager;
        this.msalObj = new Msal.UserAgentApplication(msalConfig);
        this.msalObj.handleRedirectCallback(
            (e, r) => AzureAd.prototype.authRedirectCallBack(this));
    };

    AzureAd.prototype.authRedirectCallBack = function (source) {
        debugger;
        console.log('*****************************');
        source.isCallback = true;
    };

    AzureAd.prototype.signIn = function (scopes) {
        var requestObj = {
            scopes: scopes
        };

        return this.msalObj.loginRedirect(requestObj);
            //.catch(function (error) {
            //    // we ignore errors due to user cancelling or
            //    // non granting consent. User had not logged in
            //    // but we don't want to raise an exception
            //    if (error.errorCode !== "user_cancelled" &&
            //        error.errorCode !== "consent_required") {
            //        throw error;
            //    }
            //});
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
                return self.msalObj.acquireTokenRedirect(requestObj).catch(function (error) {
                    console.log(error);
                });
            });
    };
}

window.azuread = new AzureAd();