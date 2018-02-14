matematici.TokenManager = function () {
	var
		tokenManager = this,
		cautionTimeOffset = 120000, //ms

		refreshAccessToken = function (user, successHandler, failHandler) {
			console.info("refreshing access token for " + user, new Date());
			var success;
			$.ajax(matematici.urls.accessControl.token, {
				global: false,
				data: { grant_type: "refresh_token", refresh_token: localStorage.refreshToken },
				type: "post"
			}).done(successHandler).fail(failHandler);
		},

		setup = function () {
			setInterval(function () {
				//the access token is refreshed, when a refresh token is present, [cautionTimeOffset] milliseconds before the expiration 
				if (localStorage.refreshToken && new Date().getTime() > localStorage.accessTokenExpiration - cautionTimeOffset)
					refreshAccessToken(tokenManager.getUserCode(), setMainUser, function (jqXhr, textStatus, errorThrown) {
						matematici.dialog.error("system error", "Access token refresh failed, please login again.");
						console.log("Access token refresh failed.", jqXhr, textStatus, errorThrown);
					});

				var
					signers = tokenManager.getSigners(),
					expiredSigner = [];

				for (var index in signers) {
					var signer = signers[index];
					if (signer.refreshToken && new Date().getTime() > signer.accessTokenExpiration - cautionTimeOffset)
						refreshAccessToken(signer.code, function (result) {
							updateSigner(index, result);
						}, function (jqXhr, textStatus, errorThrown) {
							console.log("Access token refresh failed.", jqXhr, textStatus, errorThrown);
							expiredSigner.push(index);
						});
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
			localStorage.roles = result.roles;
			localStorage.userCode = result.code;
			localStorage.userName = result.name;

			matematici.menu.toggleItemsByRole();
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
	};

	tokenManager.getAccessToken = function() {
		return localStorage.accessToken;
	};

	tokenManager.getRefreshToken = function () {
		return localStorage.refreshToken;
	};

	tokenManager.logout = function () {
		if (localStorage.accessToken != null)
			$.ajax(matematici.urls.accessControl.logout, {
				data: { data: { refreshToken: localStorage.refreshToken } },
				type: "post",
				suppressErrors: true
			});
		localStorage.clear();
	};

	tokenManager.getCompany = function () {
		return localStorage.company;
	};

	tokenManager.getUserCode = function () {
		return localStorage.userCode;
	};

	tokenManager.setUserCode = function (userCode, roles) {
		localStorage.userCode = userCode;
		localStorage.roles = roles;
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

	setup();
}