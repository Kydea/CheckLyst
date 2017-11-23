matematici.controls = function () {
	return {
		setEditable: function (control, value) {
			if (value)
				control.removeAttr("readonly").removeAttr("disabled").removeClass("ReadOnly");
			else
				control.prop({ readonly: true, disabled: true }).addClass("ReadOnly");
			return control
		},

		setValidationMessages: function (control, values) {
			if (values.length == 0)
				$(".ValidationMessagesIcon", control.parent().removeClass("Error")).remove();
			else {
				var errors = $("<div />").addClass("Errors").show();
				for (var index in values)
					errors.append(
						$("<div />").addClass("Error").text(" " + values[index].message).prepend(
							$("<i />").addClass("fa fa-warning")
						)
					);

				control.parent().addClass("Error").prepend($("<i />").addClass("ValidationMessagesIcon fa fa-warning").on("click", function (e) {
					e.preventDefault();
					matematici.dialog.error("Error", errors)
				}));
			}
		}
	};
}();