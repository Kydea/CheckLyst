prototypes.WorkFlowProperty = function (model, property, options) {
    var
        input,
        check,
		expressions,

		loadWorkFlows = function () {
			$.ajax({
				type: "post",
				url: matematici.urls.api + "/WorkFlows/List/GetData",
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				data: JSON.stringify({
	                itemsPerPage: 1000,
					data: {
						description: ""
					}
				})
			}).done(function (result) {
				for (var index in result.data) {
					var row = result.data[index];
					input.append($("<option />").attr({ value: row.id }).text(row.description));
				}
				input.val(model[property]);
			});
		},

        change = function () {
			model[property] = input.val();
            matematici.canClose = false;
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("Expression");
		input = $("<select />").attr("data-nullable", "");
		valuePanel.append(input);
		loadWorkFlows();
		if (options.readOnly)
			matematici.controls.setEditable(input, false);
		else
			input.on("change", change);
    }

	this.hasChanges = function () {
        return !!model[property];
    }
}

