instances.SignatureEmail = function (itemId) {
    var
        signatureEmail = this,
        emails = {},

        send = function (select) {
            var
                users = select.val(),
                selection = [];

            for (var index in users)
                selection.push(emails[users[index]]);
            new instances.Event(instances.Events.sendEmail, itemId, { emails: selection }).trigger();
        },

        setup = function () {
            $.ajax(matematici.urls.instances.api + "/GetInstanceEditors", {
                method: "post",
                data: { code: instances.instance.getCode() }
            }).done(function (users) {
                var
                    select = $("<select />").appendTo($("<div />").css("position", "relative")).prop("multiple", true).addClass("FullHeight"),
                    value = [];

                for (var index in users) {
                    var user = users[index];
                    if (user.address) {
                        select.append($("<option />").text(user.description + " - " + user.code + " <" + user.address + ">").val(user.code));
                        value.push(user.code);
                        emails[user.code] = user;
                    }
                }
                //select.val(value); //select all by default

                new matematici.AutoComplete(select);

                matematici.popup.show({
                    title: "Email notification",
                    content: select.parent(),
                    buttons: [
                        {
                            text: "send", icon: "check", handler: function () {
                                if (select.val())
                                    send(select);
                                else {
                                    matematici.dialog.show("email notification", "No notification will be sent.", [
                                        { text: "ok", icon: "check", handler: function () { matematici.popup.close(); } },
                                        { text: "cancel", icon: "times", cancelButton: true }
                                    ]);
                                    return true;
                                }
                            }
                        },
                        {
                            cancelButton: true, hidden: true, handler: function () {
                                matematici.dialog.show("email notification", "No notification will be sent.", [
                                    { text: "ok", icon: "check", handler: function () { matematici.popup.close(); } },
                                    { text: "cancel", icon: "times", cancelButton: true }
                                ]);
                                return true;
                            }
                        }
                    ]
                });
            });
        };

    signatureEmail.show = function () {
        setup();
    };
}