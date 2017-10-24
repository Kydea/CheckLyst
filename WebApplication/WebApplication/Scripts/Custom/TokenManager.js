matematici.TokenManager = function () {
	var
		tokenManager = this,
		cautionTimeOffset = 120000, //ms
		userPanel = $("header #UserPanel"),

		refreshAccessToken = function () {
			console.log(new Date(), "refreshing access token");
			localStorage.refreshingToken = true;
			$.ajax(matematici.urls.accessControl.token, {
				data: { grant_type: "refresh_token", refresh_token: localStorage.refreshToken },
				type: "post"
			}).done(function (result) {
				parseResult(result);
			}).fail(function (jqXhr, textStatus, errorThrown) {
				matematici.dialog.error("system error", "Access token refresh failed, please login again.");
				console.log(jqXhr, textStatus, errorThrown);
			}).always(function () {
				localStorage.refreshingToken = false;
			});
		},

		setup = function () {
			setInterval(function () {
				//the access token is refreshed by active client, when a refresh token is present, [cautionTimeOffset] milliseconds before the expiration 
				if (matematici.client.isActive() && localStorage.refreshToken && new Date().getTime() > localStorage.accessTokenExpiration - cautionTimeOffset)
					refreshAccessToken();
			}, 60000);
		},

		parseResult = function (result) {
			localStorage.accessToken = result.access_token;
			$(".CurrentUser", userPanel)
				.attr("title", result.code)
				.children(".UserName").text(result.name)
			localStorage.accessTokenExpiration = new Date().getTime() + parseInt(result.expires_in) * 1000;
			localStorage.timeZone = result.timeZone;
			localStorage.roles = result.roles;
			matematici.menu.toggleItemsByRole();
		};

	tokenManager.login = function (result) {
		localStorage.refreshToken = result.refresh_token;
		parseResult(result);
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

	setup();
}