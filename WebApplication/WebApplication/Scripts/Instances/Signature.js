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
				data: $.extend(instances.instance.getKey(), { SignatureLevel: model.SignatureLevel })
			});
		},

        setup = function () {
            label = $("<label />").text(model.Description);
            if (!item.isTableContent())
                label.appendTo(element);

            check = $("<i />").addClass("fa fa-check").hide().appendTo(element);

            user = $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(element);
            $("<i />").addClass("fa fa-user").insertAfter(user);
            date = $("<input />").addClass("Date").attr({ type: "text", readonly: true }).appendTo(element);
            $("<i />").addClass("fa fa-calendar").insertAfter(date);
            time = $("<input />").addClass("Time").attr({ type: "text", readonly: true }).appendTo(element);
            $("<i />").addClass("fa fa-clock-o").insertAfter(time);

            exclamation = $("<i />").addClass("fa fa-exclamation").appendTo(element).hide();

            button = $("<a />").addClass("Button SignButton").attr({ href: "#" }).appendTo(element);
            if (model.Cancelable)
                cancelButton = $("<a />").addClass("Button CancelButton").attr({ href: "#" }).appendTo(element).text("Cancel signature");
            
            element
                .on("click", "a.SignButton:not(.Selected):not(.ReadOnly)", function (e) {

                    var 
                        sign = function (username) {
                            var
                                options = {
                                    ajax: { url: matematici.urls.signature, data: "ShowPaddle=" + (model.AcceptsPIN) + "&Code=" + username + "&Description=" + (model.Description || "Signature") },
                                    data: {
                                        signHandler: function (panel) {
                                            new instances.Event(instances.Events.signature, model.Id, {
                                                UserName: panel.getUserName(),
                                                Password: panel.getPassword(),
                                                PIN: panel.getPIN(),
                                                Simple: panel.isSimple()
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
                            new instances.Event(instances.Events.signatureCheck, model.Id, { UserName: username }).done(function (result) {
                                if (result.Error)
                                    matematici.dialog.error("signature error", result.Id == null ? result.Error : "The element " + (result.Description ? result.Description + " " : "") + " with id " + result.Id + " is not valid.");
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
                            ajax: { url: matematici.urls.signature, data: "Unsign=true&Description=" + model.Description },
                            data: {
                                signHandler: function (panel) {
                                    new instances.Event(instances.Events.signatureCancelation, model.Id, {
                                        UserName: panel.getUserName(),
                                        Password: panel.getPassword(),
                                        Note: panel.getNote()
                                    }).done(function (result) {
                                        if (result.Error) {
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
                });
        };

    this.update = function (model) {
        button.text(model.ButtonLabel ? model.ButtonLabel : "Signature");
        button.toggleClass("ReadOnly", model.ReadOnly);

        if (model.Signed) {
            var zoneIndex = model.SignedOn.indexOf("+");
            var signedOn = new Date(model.SignedOn.endsWith("Z") ? model.SignedOn : (zoneIndex > 0 ? model.SignedOn.substring(0, zoneIndex) : model.SignedOn) + "Z");
			user.val(model.SignedBy + " - " + model.SignerDescription);
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

        exclamation.toggle(!!(model.History && model.History.length > 1));

        this.setColor(model.Color);
        this.setFontSize(model.FontSize);
    };

    this.setColor = function (value) {
        label.css({ color: model.Color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.FontSize == 0 ? "" : model.FontSize + "px" });
    }

    setup();
}