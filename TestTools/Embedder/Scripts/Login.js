matematici.Login = function () {
    var
        login = this,
        form = document.getElementById("login"),

        ok = function (response) {
            console.log(response);
            sessionStorage.accessToken = response.access_token;
            sessionStorage.refreshToken = response.refresh_token;
            var now = new Date();
            sessionStorage.tokenExpiration = now.setSeconds(now.getSeconds() + parseInt(response.expires_in));
            location = ".";
        },

        send = function () {
            var data = "";
            for (var index in form.elements) {
                var element = form.elements[index];
                if (element.name && element.value)
                    data += element.name + "=" + encodeURIComponent(element.value) + "&";
            }

            var xhr = new XMLHttpRequest();
            xhr.responseType = "json";

            xhr.addEventListener("load", function () {
                if (xhr.status == 200)
                    ok(xhr.response);
                else
                    alert("Error " + xhr.status + ". " + (xhr.response && xhr.response.error_description));
            });

            xhr.addEventListener("error", function () {
                alert("API server error");
            });

            xhr.open("POST", matematici.urls.api + "/AccessControl/Access/Token");
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            xhr.send(data);
            return false;
        },

        setup = function () {
            document.body.className = "login";
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                send();
            });
        };

    login.send = send;
 
    setup();
}