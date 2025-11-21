matematici.TokenManager = function () {
    var
        tokenManager = this,
        cautionTimeOffset = 120000, //ms
        warningTimeout,
        companySettings,
        lastActivity,

        getCompanySettings = function () {
            try {
                return companySettings || localStorage.companySettings && (companySettings = JSON.parse(localStorage.companySettings));
            } catch (error) {
                console.log(error);
                return null;
            }
        },

        refreshAccessToken = function (user, accessToken, refreshToken, successHandler, failHandler, index) {
            console.info("refreshing access token for " + user, new Date());

            $.ajax(matematici.urls.accessControl.token, {
                global: false,
                data: { grant_type: "refresh_token", refresh_token: refreshToken },
                type: "post",
                accessToken: accessToken
            }).done(function (result) { successHandler(result, index); }).fail(function () { failHandler(index); });
        },

        logout = function () {
            localStorage.refreshToken = null;
            matematici.canClose = true;
            window.location = matematici.urls.accessControl.login;
        },

        checkLastActivity = function (update) {
            var unchanged = lastActivity == localStorage.lastActivity;
            if (update) {
                lastActivity = localStorage.lastActivity;
                if (!unchanged) setInactivityTimeout();
            }
            return unchanged;
        },

        setInactivityTimeout = function () {
            var
                inactivityTimeoutMilliseconds = parseInt(getCompanySettings() && companySettings.inactivityTimeoutSeconds || 0) * 1000,
                countdownLength = Math.min(inactivityTimeoutMilliseconds, 60000),
                time = inactivityTimeoutMilliseconds - countdownLength - (new Date().getTime() - new Date(parseInt(lastActivity)).getTime());

            if (inactivityTimeoutMilliseconds > 0) 
                warningTimeout = setTimeout(function () {
                    if (localStorage.refreshToken && checkLastActivity(true))
                        matematici.blip.countdown(countdownLength, function () {
                            if (checkLastActivity(true)) {
                                matematici.navigator.setLastPath(matematici.navigator.getLastPath());
                                logout();
                            }
                        }, checkLastActivity);
                }, time);
        },

        resetInactivityTimeout = function (disabled) {
            if (disabled)
                localStorage.disableInactivityTimeout = "true";
            else if (disabled === false)
                localStorage.removeItem("disableInactivityTimeout");
            if (warningTimeout) clearTimeout(warningTimeout);
            if (localStorage.disableInactivityTimeout == null && localStorage.refreshToken) {
                lastActivity = localStorage.lastActivity = new Date().getTime();
                setInactivityTimeout();
            }
        },

        setup = function () {
            matematici.elements.document.on("mousedown keydown	", function () { resetInactivityTimeout(); });
            resetInactivityTimeout();

            setInterval(function () {
                //the access token is refreshed, when a refresh token is present, [cautionTimeOffset] milliseconds before the expiration 
                if (localStorage.refreshToken && new Date().getTime() > localStorage.accessTokenExpiration - cautionTimeOffset)
                    refreshAccessToken(tokenManager.getUserCode(), localStorage.accessToken, localStorage.refreshToken, setMainUser, function () {
                        matematici.dialog.error("system error", "Access token refresh failed, please login again.");
                        console.log("Access token refresh failed.");
                    });

                var
                    signers = tokenManager.getSigners(),
                    expiredSigner = [];

                for (var index in signers) {
                    var signer = signers[index];
                    if (signer.refreshToken && new Date().getTime() > signer.accessTokenExpiration - cautionTimeOffset)
                        (signer.code, signer.accessToken, signer.refreshToken, function (result, index) {
                            updateSigner(index, result);
                        }, function (index) {
                            console.log("Access token refresh failed.");
                            expiredSigner.push(index);
                        }, index);
                }
                if (expiredSigner.length > 0) {
                    for (var index = expiredSigner.length - 1; index >= 0; --index)
                        Array.remove(signers, expiredSigner[index]);

                    saveSigners(signers);
                }

            }, 60000);
        },

        setMainUser = function (result) {
            localStorage.company = result.company;
            //localStorage.companyName is set directly by the login form
            localStorage.accessToken = result.access_token;
            localStorage.accessTokenExpiration = new Date().getTime() + parseInt(result.expires_in) * 1000;
            localStorage.timeZone = result.timeZone;
            localStorage.companyTimeZone = result.companyTimeZone;
            localStorage.roles = result.roles;
            localStorage.userCode = result.code;
            localStorage.userName = result.name;
            localStorage.decimalSeparator = result.decimalSeparator;

            matematici.menu.toggleItems();
            matematici.userPanel.update();
        },

        updateSigner = function (index, result) {
            var
                signers = tokenManager.getSigners(),
                signer = signers[index];
            signer.accessToken = result.access_token;
            signer.accessTokenExpiration = new Date().getTime() + parseInt(result.expires_in) * 1000;
            signer.roles = result.roles;
            signer.name = result.name;

            localStorage.signers = JSON.stringify(signers);
        },

        saveSigners = function (signers) {
            localStorage.signers = JSON.stringify(signers);
        };

    tokenManager.login = function (result) {
        localStorage.refreshToken = result.refresh_token;
        setMainUser(result);
        resetInactivityTimeout(false);
    };

    tokenManager.getAccessToken = function () {
        return localStorage.accessToken;
    };

    tokenManager.getRefreshToken = function () {
        return localStorage.refreshToken;
    };

    tokenManager.notifyLogout = function () {
        if (localStorage.accessToken != null)
            $.ajax(matematici.urls.accessControl.logout, {
                data: { data: { refreshToken: localStorage.refreshToken } },
                type: "post",
                suppressErrors: true
            });
        var company = localStorage.company;
        localStorage.clear();
        localStorage.company = company;
        companySettings = null;
    };

    tokenManager.getCompany = function () {
        return localStorage.company;
    };

    tokenManager.getUserCode = function () {
        return localStorage.userCode;
    };

    tokenManager.getCompanySettings = getCompanySettings;

    //for the app
    tokenManager.setUserCode = function (userCode, roles, timeZone, decimalSeparator, accessToken, refreshToken) {
        localStorage.userCode = userCode;
        localStorage.roles = roles;
        localStorage.timeZone = timeZone;
        localStorage.decimalSeparator = decimalSeparator;
        tokenManager.setTokens(accessToken, refreshToken);
    };

    //called directly by the app
    tokenManager.setTokens = function (accessToken, refreshToken) {
        localStorage.accessToken = accessToken;
        localStorage.refreshToken = refreshToken;
        localStorage.accessTokenExpiration = null;
    };

    tokenManager.getUserName = function () {
        return localStorage.userName;
    };

    tokenManager.getSigners = function () {
        return localStorage.signers ? JSON.parse(localStorage.signers) : [];
    };

    tokenManager.checkRole = function (role) {
        return ("," + localStorage.roles).indexOf("," + role) >= 0;
    };

    tokenManager.addSigner = function (data) {
        var signers = tokenManager.getSigners();
        signers.push({
            refreshToken: data.refresh_token,
            accessToken: data.access_token,
            accessTokenExpiration: new Date().getTime() + parseInt(data.expires_in) * 1000,
            roles: data.roles,
            code: data.code,
            name: data.name
        });
        saveSigners(signers);
    };

    tokenManager.setSigners = function (signersJSON) {
        localStorage.signers = signersJSON;
    };

    tokenManager.removeSigner = function (code) {
        var signers = tokenManager.getSigners();
        for (var index in signers)
            if (signers[index].code == code)
                Array.remove(signers, index);
        saveSigners(signers);
    };

    tokenManager.checkSigner = function (code) {
        var signers = tokenManager.getSigners();
        if (code == "") {
            matematici.dialog.error("Add signer", "Invali user code.");
            return false;
        } else if (code == tokenManager.getUserCode()) {
            matematici.dialog.error("Add signer", code + " is logged as main user.");
            return false;
        } else
            for (var index in signers)
                if (code == signers[index].code) {
                    matematici.dialog.error("Add signer", code + " is already logged as signer.");
                    return false;
                }
        return true;
    };

    //use by the console for disgnostics
    tokenManager.forceRefresh = function () {
        refreshAccessToken(tokenManager.getUserCode(), localStorage.accessToken, localStorage.refreshToken, setMainUser, function (jqXhr, textStatus, errorThrown) {
            matematici.dialog.error("system error", "Access token refresh failed, please login again.");
            console.log("Access token refresh failed.");
        });

        var signers = tokenManager.getSigners();
        for (var index in signers) {
            var signer = signers[index];
            refreshAccessToken(signer.code, signer.accessToken, signer.refreshToken, function (result, index) {
                console.log("Access token refresh worked.", result);
                updateSigner(index, result);
            }, function (index) {
                console.log("Access token refresh failed.");
                expiredSigner.push(index);
            }, index);
        }
    };

    tokenManager.resetInactivityTimeout = resetInactivityTimeout;

    if (!matematici.appMode) setup();
}