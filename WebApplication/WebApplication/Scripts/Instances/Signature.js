instances.Signature = function (item, model) {
    var
        user,
        date,
        time,
        button,
        cancelButton,
        exclamation,
        element = item.getElement(),
        label,
        check,

        setup = function () {
			label = $("<label />").addClass("Controls3").append($("<span />").addClass("Label").html(model.descriptionValue ? model.descriptionValue : "(no description)"));
            if (!item.isTableContent())
                label.appendTo(element);

			check = $("<i />").addClass("Icon fi flaticon-check").hide().appendTo(element);

			$("<i />").addClass("Icon fi flaticon-user-o").appendTo(item.isTableContent() ? element : label);
			user = $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(item.isTableContent() ? element : label);
			$("<i />").addClass("Icon fi flaticon-calendar").appendTo(item.isTableContent() ? element : label);
			date = $("<input />").addClass("Date").attr({ type: "text", readonly: true }).appendTo(item.isTableContent() ? element : label);
			$("<i />").addClass("Icon fi flaticon-clock-o").appendTo(item.isTableContent() ? element : label);
			time = $("<input />").addClass("Time").attr({ type: "text", readonly: true }).appendTo(item.isTableContent() ? element : label);

			exclamation = $("<i />").addClass("fi flaticon-exclamation").appendTo($("span", label)).attr("title", "This signature has been canceled, see the history").hide();

			button = $("<a />").addClass("Button SignButton").attr({ href: "#" }).appendTo(element);
            if (model.cancelable)
				cancelButton = $("<a />").addClass("Button CancelButton").attr({ href: "#" }).appendTo(element).text("Cancel signature").prepend($("<i >").addClass("fi flaticon-times"));
            
            element
                .on("click", "a.SignButton:not(.Selected):not(.ReadOnly)", function (e) {

                    var 
						signers = matematici.tokenManager.getSigners(),
						mainUser = matematici.tokenManager.getUserCode(),
						sign = function (code, accessToken) {
                            var
                                options = {
									title: model.descriptionValue || "Signature",
									width: 460,
									ajax: {
										type: "post",
										url: matematici.urls.accessControl.signature
									},
									data: {
										showPaddle: model.acceptsPin,
										username: code,
                                        signHandler: function (panel) {
                                            new instances.Event(instances.Events.signature, model.id, {
												userName: mainUser, //used to check the lock of the page, the signer is taken from the accesstoken
                                                password: panel.getPassword(),
                                                pin: panel.getPIN(),
                                                simple: panel.isSimple()
											}, accessToken).done(function (result) {
                                                instances.instance.update(result);
                                                matematici.popup.close();
                                            }).trigger();
                                        }
                                    }
                                };

                            matematici.popup.show(options);
                        },

                        trySign = function(code, accessToken) {
							new instances.Event(instances.Events.signatureCheck, model.id, { userName: code }).done(function (result) {
								sign(code, accessToken);
                            }).trigger();
                        };

					if (signers.length == 0) 
						trySign(mainUser);
                    else {
                    	var buttons = [];
                    	buttons.push({
                    		text: "Cancel",
                    		icon: "times",
                    		cancelButton: true
                    	});
                    	$.each(signers, function () {
                    		var signer = this;
                            buttons.push({
                                text: signer.code,
                                title: signer.name,
                                icon: "user-o",
                                data: signer,
                                handler: function (signer) {
									trySign(signer.code, signer.accessToken || "-");
                    			}
                    		});
                    	});
						buttons.push({
							text: mainUser,
							title: matematici.tokenManager.getUserName(),
							icon: "user-o",
							handler: function () {
								trySign(mainUser);
							}
						});
                    	matematici.dialog.popupMenu(buttons);
					};
                })
                .on("click", "a.CancelButton:not(.Selected):not(.ReadOnly)", function (e) {
                    var
                        button = $(this),
						options = {
							title: model.descriptionValue || "Signature cancelation",
							width: 460,
							ajax: {
								type: "post",
								url: matematici.urls.accessControl.signature
							},
							data: {
								unsign: true,
								username: matematici.tokenManager.getUserCode(),
                                signHandler: function (panel) {
                                    new instances.Event(instances.Events.signatureCancelation, model.id, {
                                        userName: panel.getUserName(),
                                        password: panel.getPassword(),
                                        note: panel.getNote()
                                    }).done(function (result) {
                                        instances.instance.update(result);
										matematici.popup.close();
                                    }).trigger();
                                }
                            }
                        };

                    matematici.popup.show(options);
                });
        };

    this.update = function (model) {
		button.text(model.buttonLabel ? model.buttonLabel : "Signature").prepend($("<i >").addClass("fi flaticon-check"));
		button.toggleClass("ReadOnly", model.readOnly);
		if (cancelButton) cancelButton.toggleClass("ReadOnly", model.readOnly);

		if (model.signed) {
			user.val(model.signedBy + " - " + model.signerDescription);
			var format = matematici.dateFormatter.format(model.signedOn, true);
            date.val(format.date);
            time.val(format.time + ":" + format.seconds);
            element.attr("title", user.val() + " | " + format.date + " " + " " + format.time + ":" + format.seconds + " " + format.offset);
            if (cancelButton) {
				cancelButton.css("display", "inline-block");
				button.css("display", "none");
            } else if (item.isTableContent()) {
				button.css("display", "none");
                check.show();
            }
        } else {
            user.val("");
            date.val("");
            time.val("");
            element.attr("title", "");
            if (cancelButton) {
				button.css("display", "inline-block");
				cancelButton.css("display", "none");
            }
        }

        exclamation.toggle(!!(model.history && model.history.length > 1));

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        label.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    }

    setup();
}