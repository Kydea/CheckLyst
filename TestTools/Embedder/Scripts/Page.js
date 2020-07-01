matematici.Page = function () {
    var
        page = this,

        setup = function () {
            document.body.className = "page";
            var tokenExpiration = parseInt(sessionStorage.tokenExpiration);
            if (sessionStorage.tokenExpiration == null || isNaN(tokenExpiration) || new Date(tokenExpiration) < new Date()) {
                console.log("Token expired");
                location = ".";
            }
        };

    setup();
}