matematici.DetailPage = function (options) {
	var
		detailPage = this,
		data = matematici.popup.getData(),
		sender = matematici.popup.getSender(),
		buttons = options.buttons || [],
		form,

		clickHandler = function (button, event, callback) {
			if (button.hasClass("ReadOnly")) return false;

			form.trigger(event, function (result) {
				form.handleResult(result);
				if (result.valid) {
					matematici.popup.getResult().saved = true;
					matematici.popup.close();
					matematici.blip.ok(callback);
				}
			}, options.parametersPrepareHandler && options.parametersPrepareHandler());

			return true;
		},

		setup = function () {
			options.readOnly = options.readOnly != null ? options.readOnly : sender.getOptions().searchPage.isReadOnly();
			buttons.push({
				cancelButton: true, hidden: true, handler: function () {
					if (matematici.canClose)
						return false;

					matematici.dialog.show("warning", "Warning: any unsaved change will be lost.", [
						{ text: "save", icon: "check", handler: matematici.save },
						{ text: "discard the changes", icon: "exclamation", handler: matematici.popup.close },
						{ text: "cancel", hidden: true, icon: "remove", cancelButton: true }
					]);
					return true;
				}
			});

			if (!options.readOnly) {
				buttons.push({
					text: "delete", icon: "trash-o", handler: function () {
						matematici.dialog.show("warning", "The record will be deleted.", [
							{
								text: "confirm", icon: "check", handler: function () {
									clickHandler($(this), "Delete");
								}
							},
							{
								text: "cancel", icon: "times", cancelButton: true
							}
						]);
						return true;
					}
				});

				buttons.push({
					text: "save", defaultButton: true, icon: "floppy-o", id: "SaveButton", handler: function (e) {
						clickHandler($(this), data == null ? "Add" : "Edit");
						return true;
					}
				});
			}

			matematici.popup.setButtons(buttons);

			matematici.popup.setWidth(options.width || "460px");

			form = new matematici.Form(options.formSelector, {
				applyButton: "#SaveButton",
				bindEvent: options.bindEvent,
				readOnly: options.readOnly
			});
			form.initialize();
			if (!options.readOnly)
				form.addChangeHandler(function () {
					matematici.canClose = false;
					matematici.save = function (callback) {
						clickHandler($("#SaveButton"), data == null ? "Add" : "Edit", callback);
					}
				});

			setTimeout(function () {
				if (options.initHandler) options.initHandler(detailPage);
				form.setValues(data);
				if (options.completeHandler) options.completeHandler(detailPage);
			}, 0);
		};

	this.getElement = function () {
		return matematici.popup.getElement();
	};

	this.getSender = function () {
		return sender;
	};

	this.getForm = function () {
		return form;
	};

	this.isNew = function () {
		return data == null;
	};

	this.isReadOnly = function () {
		return options.readOnly;
	};

	setup();
}