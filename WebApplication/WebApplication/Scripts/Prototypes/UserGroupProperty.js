prototypes.UserGroupProperty = function (model, property, options) {
	var
		base,
		input,

		loadRecords = function () {
			$.ajax({
				type: "post",
				url: matematici.urls.api + "/AccessControl/Groups/GetSyntheticData",
				dataType: "json",
				contentType: "application/json; charset=utf-8"
			}).done(function (result) {
				for (var index in result) {
					var group = result[index];
					input.append($("<option />").attr({ value: group }).text(group));
				}
				input.val(model[property]);
			});
		},

		change = function () {
			model[property] = input.val();
			base.updateModel(model);
		};

	this.create = function (valuePanel, baseClass) {
		base = baseClass;
		valuePanel.addClass("Expression");
		input = $("<select />").attr("data-nullable", "");
		valuePanel.append(input);
		loadRecords();
		if (options.readOnly)
			matematici.controls.setEditable(input, false);
		else
			input.on("change", change);
	}

	this.hasChanges = function () {
		return !!model[property];
	}
}

