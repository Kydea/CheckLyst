matematici.TokenManager = function () {
    var
        tokenManager = this;

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
}