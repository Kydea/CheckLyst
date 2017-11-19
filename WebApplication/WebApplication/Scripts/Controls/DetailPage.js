matematici.DetailPage = function (options) {
	var
		data = matematici.popup.getData(),
		sender = matematici.popup.getSender(),
		buttons = [],
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

			if (data != null)
				buttons.push({
					text: "delete", icon: "delete", handler: function () {
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

			buttons.push({
				text: "save", icon: "save", id: "SaveButton", handler: function () {
					clickHandler(data == null ? "Add" : "Edit");
					return true;
				}
			});

			matematici.popup.setButtons(buttons);

			matematici.popup.setWidth(420);

			form = new matematici.Form(options.formSelector, {
				applyButton: "#SaveButton"
			});
			form.initialize();
			if (data != null) form.setValues(data);
		}

	setup();
}