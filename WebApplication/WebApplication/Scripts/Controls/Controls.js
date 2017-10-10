matematici.controls = function () {
	return {
		setEditable: function (control, value) {
			if (value)
				control.removeAttr("readonly").removeAttr("disabled").removeClass("ReadOnly");
			else
				control.prop({ readonly: true, disabled: true }).addClass("ReadOnly");
			return control
		}
	};
}();