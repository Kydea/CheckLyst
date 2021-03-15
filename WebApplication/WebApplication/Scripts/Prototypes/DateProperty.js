prototypes.DateProperty = function (model, property, options) {
    var
        base,
        input,
		check,
        datePicker,

		change = function () {
			var date = datePicker.getValue();
        	if (check)
        	    for (var index in check)
        	        if (!check[index].action.call(this, date)) {
        	            new matematici.dialog.error("error", check[index].message);
        	            return;
					}

        	if (date) {
				model[property] = date;
        	} else {
        		model[property] = null;
        		input.val("");
			}

            base.updateModel(model);
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
    	valuePanel.addClass("Date");
		input = $("<input />")
			.attr({ type: "text", readOnly: options.readOnly });
		valuePanel.append($("<label >").append(input));
		datePicker = new matematici.DatePicker(input[0]);
		if (!!model[property])
			datePicker.setValue(model[property]);
		if (!options.readOnly)
			input
				.addClass("DatePicked")
				.on("change", change);
    }

    this.hasChanges = function () {
        return !!model[property];
    }

    this.setValue = function (node) {
        node.text(model[property]);
    }
}