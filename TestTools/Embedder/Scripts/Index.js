matematici.Index = function () {
    var
        index = this,

        setup = function () {
            document.body.className = "index";
            var tokenExpiration = parseInt(sessionStorage.tokenExpiration);
            if (sessionStorage.tokenExpiration == null || isNaN(tokenExpiration) || new Date(tokenExpiration) < new Date()) {
                console.log("Token expired");
                location = "login";
            }
        };

    setup();
}