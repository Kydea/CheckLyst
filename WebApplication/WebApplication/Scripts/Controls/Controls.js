matematici.controls = function () {
	return {
		setEditable: function (control, value) {
			if (value)
				control.removeAttr("readonly").removeAttr("disabled").removeClass("ReadOnly");
			else
				control.prop({ readonly: true, disabled: true }).addClass("ReadOnly");
			return control;
		},

		setValidationMessages: function (control, values) {
			if (values.length === 0) {
				$(".ValidationMessagesIcon", control.parent().removeClass("Error")).remove();
				control.off(".ValidationMessages");
			} else {
				var errors = $("<div />").addClass("Errors").show();
				for (var index in values)
					errors.append(
						$("<div />").addClass("Error").text(" " + values[index].message).prepend(
                            $("<i />").addClass("fas fa-exclamation-triangle")
						)
					);

				if (control.attr("type") === "hidden") { // Avoid validation messages on hidden fields
					return;
				}

                control.parent().addClass("Error").prepend($("<i />").addClass("ValidationMessagesIcon fas fa-exclamation-triangle").on("click", function (e) {
					e.preventDefault();
					matematici.dialog.error("Error", errors);
				}));

				control
					.on("keypress.ValidationMessages", function (e) {
						if (e.key != "Tab" && e.which !== 0 && !e.ctrlKey && !e.metaKey && !e.altKey) {
							matematici.controls.setValidationMessages($(this), []);
						}
					})
					.on("change.ValidationMessages", function (e) {
						matematici.controls.setValidationMessages($(this), []);
					})
					.on("paste.ValidationMessages", function (e) {
						matematici.controls.setValidationMessages($(this), []);
					});
			}
		}
	};
}();