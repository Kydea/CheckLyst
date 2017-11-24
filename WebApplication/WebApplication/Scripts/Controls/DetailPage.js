matematici.DetailPage = function (options) {
	var
		detailPage = this,
		data = matematici.popup.getData(),
		sender = matematici.popup.getSender(),
		buttons = options.buttons || [],
		form,

		clickHandler = function (event) {
			form.trigger(event, function (result) {
				form.handleResult(result);
				if (result.valid) {
					matematici.popup.getResult().saved = true;
					matematici.popup.close();
				}
			});
		},

		setup = function () {
			buttons.push({ cancelButton: true, hidden: true });

			if (!options.readOnly && data != null)
				buttons.push({
					text: "delete", icon: "trash-o", handler: function () {
						matematici.dialog.show("warning", "The record will be deleted.", [
							{
								text: "confirm", icon: "check", handler: function () {
									clickHandler("Delete");
								}
							},
							{ text: "cancel", icon: "remove", cancelButton: true }
						]);
						return true;
					}
				});

			if (!options.readOnly)
				buttons.push({
					text: "save", icon: "floppy-o", id: "SaveButton", handler: function () {
						clickHandler(data == null ? "Add" : "Edit");
						return true;
					}
				});

			matematici.popup.setButtons(buttons);

			matematici.popup.setWidth(options.width || "420px");

			form = new matematici.Form(options.formSelector, {
				applyButton: "#SaveButton",
				bindEvent: options.bindEvent
			});
			form.initialize();
			setTimeout(function () {
				if (options.initHandler) options.initHandler(detailPage);
				form.setValues(data);
				if (options.completeHandler) options.completeHandler(detailPage);
			}, 0);
			
		};

	this.getElement = function () {
		return matematici.popup.getElement();
	},

	this.getSender = function () {
		return sender;
	};

	this.getForm = function () {
		return form;
	};

	this.isNew = function () {
		return data == null;
	};

	setup();
}