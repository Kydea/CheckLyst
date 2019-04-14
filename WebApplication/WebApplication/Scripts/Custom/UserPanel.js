matematici.UserPanel = function (selector) {
	var
		element = $(selector),

		setup = function () {
			element
				.on("click", "a.User", function (e) {
					matematici.dialog.popupMenu([
						{ text: "Cancel", icon: "times", cancelButton: true },
						{
							text: "Logout", icon: "logout", handler: function () {
                                matematici.dialog.close();
                                window.location = matematici.urls.accessControl.login;
								return true;
							}
						},
						{
							text: "Change PIN", icon: "key-o", handler: function () {
								profile(matematici.tokenManager.getUserCode());
							}
						},
					]);
				})
				.on("click", "a.Add", function (e) {
					var
						buttons = [
							{ text: "Cancel", icon: "times", cancelButton: true },
							{ text: "Add", icon: "plus", handler: add }
						],
                        signers = matematici.tokenManager.getSigners();

                    for (var index in signers) {
                        var signer = signers[index];
						buttons.push({
                            text: signer.code, icon: "user-o", title: signer.name, data: signer, handler: function (signer) {
								profile(signer.code, signer.accessToken || "-");
							}
						});
					}

					matematici.dialog.popupMenu(buttons);
				});
		},

		add = function () {
			var button = $(this);

			options = {
				title: "login",
				width: 460,

				ajax: {
					type: "post",
					url: matematici.urls.accessControl.signature
				},
				data: {
					signHandler: function (panel) {
						if (matematici.tokenManager.checkSigner(panel.getUserName()))
							$.ajax(matematici.urls.accessControl.token, {
								type: "post",
								suppressToken: true,
								data: {
									grant_type: "password",
									companyId: matematici.tokenManager.getCompany(),
									username: panel.getUserName(),
									password: panel.getPassword(),
								}
							}).done(function (result) {
								button.siblings(".User").last().clone().insertBefore(button).children("span.UserName")
									.attr("title", result.code)
									.text(result.name);
								matematici.tokenManager.addSigner(result);
								matematici.popup.close();
								matematici.blip.ok();
							});
					}
				},
			}

			matematici.popup.show(options);
		},

		profile = function (userCode, accessToken) {
			options = {
				title: "PIN",
				width: 460,
				ajax: {
					type: "post",
					url: matematici.urls.accessControl.signature,
					//data: "ShowPaddle=false&IsEnabledPIN=true&ButtonOk=Ok&Logout=" + (matematici.userPanel.getUsers()[0] != userCode)
				},
				data: {
					pin: true,
					logout: matematici.tokenManager.getUserCode() != userCode,
					username: userCode,
					signHandler: function (panel) {
						if (panel.getPIN() == "")
							matematici.dialog.error("PIN", "PIN is mandatory");
						else if (panel.getPIN() != panel.getConfirmPIN())
							matematici.dialog.error("PIN", "PIN and confirm pin must contain the same values");
						else if (panel.getPIN().length < 6)
							matematici.dialog.error("PIN", "PIN's length must be greater than 5 characters");
						else if (!panel.getPassword())
							matematici.dialog.error("PIN", "Provide your password");
						else
							$.ajax(matematici.urls.accessControl.setPIN, {
								method: "post",
								accessToken: accessToken,
								data: {
									password: panel.getPassword(),
									pin: panel.getPIN()
								}
							}).done(function (result) {
								matematici.popup.close();
								matematici.blip.ok();
							});
					},

					logoutHandler: function (panel) {
						matematici.tokenManager.removeSigner(userCode);
						matematici.popup.close();
						matematici.blip.ok();
					}
				}
			}
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
			.children(".UserName").text(matematici.tokenManager.getUserName());

		$(".CompanyName", matematici.elements.footer).text(localStorage.companyName);
	};

	this.profile = function () {
		profile(matematici.tokenManager.getUserCode(), matematici.tokenManager.getAccessToken());
	};

	setup();
}
