matematici.UserSettingsClient = function () {
    return {
        load: function (setting, code, handler) {
            var data = JSON.stringify({
                data: {
                    setting: setting,
                    code: code
                }
            });
            $.ajax({
                url: matematici.urls.api + "/AccessControl/Users/LoadSettings",
                type: "post",
                contentType: "application/json; charset=utf-8",
                data: data
            }).done(function (data) {
                handler(data);
            });
        },

        save: function (settings) {
            var data = JSON.stringify({
                data: settings
            });
            $.ajax({
                url: matematici.urls.api + "/AccessControl/Users/SaveSettings",
                type: "post",
                contentType: "application/json; charset=utf-8",
                data: data
            });
        }
    }
};

matematici.userSettingsClient = new matematici.UserSettingsClient();