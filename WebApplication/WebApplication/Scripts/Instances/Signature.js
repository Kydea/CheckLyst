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

			exclamation = $("<i />").addClass("fi flaticon-exclamation").appendTo($("span", label)).attr("title", "This signature has been canceled, see the history").hide();

			button = $("<a />").addClass("Button SignButton").attr({ href: "#" }).appendTo(element);
            if (model.cancelable)
				cancelButton = $("<a />").addClass("Button CancelButton").attr({ href: "#" }).appendTo(element).text("Cancel signature").prepend($("<i >").addClass("fi flaticon-times"));
            
            element
                .on("click", "a.SignButton:not(.Selected):not(.ReadOnly)", function (e) {

                    var 
                        sign = function (username) {
                            var
                                options = {
									title: model.description || "Signature",
									width: 460,
									ajax: {
										type: "post",
										url: matematici.urls.instances.signature
									},
									data: {
										showPaddle: model.acceptsPin,
										username: username,
                                        signHandler: function (panel) {
                                            new instances.Event(instances.Events.signature, model.id, {
                                                userName: panel.getUserName(),
                                                password: panel.getPassword(),
                                                pin: panel.getPIN(),
                                                simple: panel.isSimple()
                                            }).done(function (result) {
                                                instances.instance.update(result);
                                                matematici.popup.close();
                                            }).trigger();
                                        }
                                    }
                                };

                            matematici.popup.show(options);
                        },

                        trySign = function(username) {
                            new instances.Event(instances.Events.signatureCheck, model.id, { userName: username }).done(function (result) {
                                sign(username);
                            }).trigger();
                        };

					if (true) //TODO
						trySign(matematici.tokenManager.getUserCode());
                    else {
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
					};
                })
                .on("click", "a.CancelButton:not(.Selected):not(.ReadOnly)", function (e) {
                    var
                        button = $(this),
						options = {
							title: model.description || "Signature cancelation",
							width: 460,
							ajax: {
								type: "post",
								url: matematici.urls.instances.signature
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

		if (model.signed) {
			user.val(model.signedBy + " - " + model.signerDescription);
			var format = matematici.dateFormatter.format(model.signedOn, true);
            date.val(format.date);
			time.val(format.time);
			element.attr("title", user.val() + " | " + format.date + " " + " " + format.time + " " + format.offset);
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