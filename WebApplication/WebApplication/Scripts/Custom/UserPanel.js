matematici.UserPanel = function (selector) {
    var
        element = $(selector),

        setup = function () {
            element
                .on("click", "a.User", function (e) {
                    login($(".UserName", this).attr("title"));
                })
                .on("click", "a.Add", add);
        },

        add = function () {
            var button = $(this);

            options = {
                ajax: { url: matematici.urls.signature, data: "ShowPaddle=false&IsEnabledPIN=false&ButtonOk=Ok" },
                data: {
                    signHandler: function (login) {
                        $.ajax(matematici.urls.addSession, {
                            method: "post",
                            data: { Code: login.getUserName(), Password: login.getPassword() }
                        }).done(function (result) {
                            if (result.ok) {
                                console.log(result.user.Description);
                                button.siblings(".User").last().clone().insertBefore(button).children("span.UserName").attr("title", result.user.Code).text(result.user.Description);
                                matematici.popup.close();
                                matematici.dialog.show("login", "login done with success.", { text: "ok", icon: "check", defaultButton: true, cancelButton: true });
                            } else {
								matematici.popup.close();
                                matematici.dialog.error("error", result.error);
                            }
                        }).fail(function (jqXHR, textStatus, errorThrown) {
                            console.log(jqXHR, textStatus, errorThrown);
                            matematici.dialog.error("server error", "Client unique identifier unavailable");
                        });
                    }
                }
            }

            options.title = 'login';
            matematici.popup.show(options);
        },

        login = function (user) {

            options = {
                ajax: {
                    url: matematici.urls.signature, data: "ShowPaddle=false&IsEnabledPIN=true&ButtonOk=Ok&Logout="+(matematici.userPanel.getUsers()[0] != user),
                    callback: function () {
                        $("input#UserName").val(user);
                    }
                },
                data: {
                    signHandler: function (login) {
                        if (login.getPIN() == "")
                            login.showError("PIN is mandatory");
                        else if (login.getPIN() != login.getConfirmPIN())
                            login.showError("PIN and confirm pin must contain the same values");
                        else if(login.getPIN().length < 6)
                            login.showError("PIN's length must be greater than 5 characters");
                        else
                            $.ajax(matematici.urls.setPIN, {
                                method: "post",
                                data: { Code: login.getUserName(), Password: login.getPassword(), PIN: login.getPIN(), ConfirmPIN: login.getConfirmPIN() }
                            }).done(function (result) {
                                if (result.ok) {
                                    matematici.popup.close();
                                    matematici.dialog.show("pin", "PIN set successfully.", { text: "ok", icon: "check", defaultButton: true, cancelButton: true });
                                } else {
                                    matematici.popup.close();
                                    matematici.dialog.error("error", result.error);
                                }
                            }).fail(function (jqXHR, textStatus, errorThrown) {
                                console.log(jqXHR, textStatus, errorThrown);
                                matematici.dialog.error("server error", "Client unique identifier unavailable");
                            });
                    },

                    logoutHandler: function (login) {
                        $.ajax(matematici.urls.removeSession, {
                            method: "post",
                            data: { Code: login.getUserName() }
                        }).done(function (result) {
                            if (result.ok) {
								element.find('.UserName[title="' + login.getUserName() + '"]').parent().remove();
								matematici.popup.close();
                                matematici.dialog.show("session", "user logged out.", { text: "ok", icon: "check", defaultButton: true, cancelButton: true });
                            } else {
                                matematici.popup.close();
                                matematici.dialog.error("error", result.error);
                            }
                        }).fail(function (jqXHR, textStatus, errorThrown) {
                            console.log(jqXHR, textStatus, errorThrown);
                            matematici.dialog.error("server error", "Client unique identifier unavailable");
                        });
                    }
                }
            }
            options.title = 'pin';
            matematici.popup.show(options);
        };

	this.getUsers = function () {
		var users = [];
		$("a.User", element).each(function () {
			users.push($("span.UserName", this).attr("title"));
		});
		return users;
	};

	this.update = function () {
		$(".CurrentUser", element)
			.attr("title", localStorage.userCode)
			.children(".UserName").text(localStorage.userName)
	};

    setup();
}
