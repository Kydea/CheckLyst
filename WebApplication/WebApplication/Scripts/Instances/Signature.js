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

		loadSigners = function () {
			return $.ajax(matematici.urls.instances.signers, {
				type: "POST",
				data: $.extend(instances.instance.getKey(), { SignatureLevel: model.signatureLevel })
			});
		},

        setup = function () {
			label = $("<label />").addClass("Controls3").append($("<span />").addClass("Label").html(model.description ? model.description : "(no description)"));
            if (!item.isTableContent())
                label.appendTo(element);

			check = $("<i />").addClass("Icon fi flaticon-check").hide().appendTo(element);

			$("<i />").addClass("Icon fi flaticon-user-o").appendTo(item.isTableContent() ? element : label);
			user = $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(item.isTableContent() ? element : label);
			$("<i />").addClass("Icon fi flaticon-calendar").appendTo(item.isTableContent() ? element : label);
			date = $("<input />").addClass("Date").attr({ type: "text", readonly: true }).appendTo(item.isTableContent() ? element : label);
			$("<i />").addClass("Icon fi flaticon-clock-o").appendTo(item.isTableContent() ? element : label);
			time = $("<input />").addClass("Time").attr({ type: "text", readonly: true }).appendTo(item.isTableContent() ? element : label);

			exclamation = $("<i />").addClass("fi flaticon-exclamation").appendTo(element).hide();

			button = $("<a />").addClass("Button SignButton").attr({ href: "#" }).appendTo(element);
            if (model.cancelable)
				cancelButton = $("<a />").addClass("Button CancelButton").attr({ href: "#" }).appendTo(element).text("Cancel signature").prepend($("<i >").addClass("fi flaticon-times"));
            
            element
                .on("click", "a.SignButton:not(.Selected):not(.ReadOnly)", function (e) {

                    var 
                        sign = function (username) {
                            var
                                options = {
                                    ajax: { url: matematici.urls.signature, data: "ShowPaddle=" + (model.acceptsPIN) + "&Code=" + username + "&Description=" + (model.description || "Signature") },
                                    data: {
                                        signHandler: function (panel) {
                                            new instances.Event(instances.Events.signature, model.id, {
                                                userName: panel.getUserName(),
                                                password: panel.getPassword(),
                                                pin: panel.getPIN(),
                                                simple: panel.isSimple()
                                            }).done(function (result) {
												if (result.Error) {
													//error with dialog
                                                    var error = $("div.Error", matematici.popup.getElement());
                                                    error.fadeTo(500, .9).children().text(result.Error);
                                                    setTimeout(function () {
                                                        error.fadeTo(500, 0, function () { error.hide(); });
                                                    }, 3000);
                                                } else {
                                                    instances.instance.update(result);
                                                    matematici.popup.close();
                                                }
                                            }).trigger();
                                        }
                                    }
                                };

                            matematici.popup.show(options);
                        },

                        trySign = function(username) {
                            new instances.Event(instances.Events.signatureCheck, model.id, { userName: username }).done(function (result) {
                                if (result.Error)
                                    matematici.dialog.error("signature error", result.id == null ? result.error : "The element " + (result.description ? result.description + " " : "") + " with id " + result.id + " is not valid.");
                                else
                                    sign(username);
                            }).trigger();
                        };

                    loadSigners().done(function (result) {
                    	if (result.error) {
                    		matematici.dialog.error("Signature error", result.error);
                    	} else if (result.length == 0) {
                    		matematici.dialog.error("Signature error", "There aren't signers available");
                    	} else if (result.length == 1) {
                    		trySign(result[0]);
                    	} else {
                    		var buttons = [];
                    		buttons.push({
                    			text: "Cancel",
                    			icon: "times",
                    			cancelButton: true
                    		});
                    		$.each(result, function () {
                    			var username = this;
                    			buttons.push({
                    				text: username,
                    				icon: "user",
                    				handler: function () {
                    					setTimeout(function () {
                    						trySign(username);
                    					}, 100);
                    				}
                    			});
                    		});
                    		matematici.dialog.popupMenu(buttons);
                    	}
                    });
                })
                .on("click", "a.CancelButton:not(.Selected):not(.ReadOnly)", function (e) {
                    var
                        button = $(this),
                        options = {
                            ajax: { url: matematici.urls.signature, data: "Unsign=true&Description=" + model.description },
                            data: {
                                signHandler: function (panel) {
                                    new instances.Event(instances.Events.signatureCancelation, model.id, {
                                        userName: panel.getUserName(),
                                        password: panel.getPassword(),
                                        note: panel.getNote()
                                    }).done(function (result) {
                                        if (result.Error) {
                                            var error = $("div.Error", matematici.popup.getElement());
                                            error.fadeTo(500, .9).children().text(result.error);
                                            setTimeout(function () {
                                                error.fadeTo(500, 0, function () { error.hide(); });
                                            }, 3000);
                                        } else {
                                            instances.instance.update(result);
                                            matematici.popup.close();
                                        }
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

        if (model.signed) {
            var zoneIndex = model.signedOn.indexOf("+");
            var signedOn = new Date(model.signedOn.endsWith("Z") ? model.signedOn : (zoneIndex > 0 ? model.signedOn.substring(0, zoneIndex) : model.signedOn) + "Z");
			user.val(model.signedBy + " - " + model.signerDescription);
			var format = matematici.dateFormatter.date(signedOn, true);
            date.val(format.date);
			time.val(format.time);
            element.attr("title", user.val() + " | " + date.val() + " | " + time.val());
            if (cancelButton) {
                button.hide();
                cancelButton.show();
            } else if (item.isTableContent()) {
                button.hide();
                check.show();
            }
        } else {
            user.val("");
            date.val("");
            time.val("");
            element.attr("title", "");
            if (cancelButton) {
                button.show();
                cancelButton.hide();
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