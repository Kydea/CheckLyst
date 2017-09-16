prototypes.DateProperty = function (model, property, options) {
    var
        input,
        check,
        change = function () {
        	var date = $.datepicker.parseDate("dd/mm/yy", input.val());
        	if (check)
        	    for (var index in check)
        	        if (!check[index].action.call(this, date)) {
        	            new matematici.dialog.error("error", check[index].message);
        	            return;
        	        }
        	if (date) {
        		model[property] = String.format("{0:yyyy-MM-dd}T00:00:00", date);
        	} else {
        		model[property] = null;
        		input.val("");
        	}
            matematici.canClose = false;
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel) {
    	valuePanel.addClass("Date");
        input = $("<input />")
            .attr({ type: "text", readOnly: options.readOnly })
            .val(model[property] ? String.format("{0:dd/MM/yyyy}", new Date(model[property])) : null)
        if (!options.readOnly) {
            input
                .datepicker()
                .on("change", change);
        }
        valuePanel.append(input);
    }

    this.hasChanges = function () {
        return !!model[property];
    }
}

