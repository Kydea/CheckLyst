matematici.TokenManager = function (inactivityTimeout) {
    var
        tokenManager = this,
        lastActivity = new Date(),

        setup = function () {
            matematici.elements.document.on("mousedown keydown	", function () {
                lastActivity = new Date();
            });

            setInterval(function () {
                if (!matematici.isLoginPage && new Date() - lastActivity > inactivityTimeout * 1000) {
                    matematici.canClose = true;
                    location = matematici.urls.login;
                    return;
                }
            }, 60000);
        };


    tokenManager.login = function (company, userCode, userName) {
        localStorage.company = company;
        localStorage.userCode = userCode;
        localStorage.userName = userName;
	};

	tokenManager.getCompany = function () {
		return localStorage.company;
	};

	tokenManager.getUserCode = function () {
		return localStorage.userCode;
	};

	tokenManager.getUserName = function () {
		return localStorage.userName;
	};

	tokenManager.checkRole = function (role) {
        return true;
    };

    setup();
}