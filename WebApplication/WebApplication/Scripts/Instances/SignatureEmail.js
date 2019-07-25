instances.SignatureEmail = function (itemId) {
    var
        signatureEmail = this,

        getInstanceEditors = function () {
            $.ajax(matematici.urls.instances.api + "/GetInstanceEditors", {
                method: "post",
                data: { instanceCode: instances.instance.getCode() }
            }).done(function (result) {
                console.log(result);
            });
        },

        setup = function () {
            var users = getInstanceEditors();
        };

    signatureEmail.show = function () {
        setup();
    };
}