prototypes.ListProperty = function (model, property, values, options) {
    var
        input,
        handler,
        check,

        change = function () {
            model[property] = input.val();
            matematici.canClose = false;
            if (handler) handler.call(this, model);
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.setHandler = function (value) {
        handler = value;
        return this;
    };

    this.create = function (valuePanel) {
        valuePanel.addClass("Expression");

        if (options.readOnly)
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(model[property])
                .appendTo(valuePanel);
        else {
			input = $("<select />");
			if (!!options.nullable) input.data("nullable", true);
			for (var index in values) {
				var
					string = typeof (values[index]) == "string",
					value = string ? values[index] : values[index].code,
					label = string ? values[index] : values[index].label;
				input.append($("<option />").attr({ value: value, selected: value == model[property] }).text(label));
			}
            input.on("change", change);
			valuePanel.append(input);
			if (!!options.nullable && !model[property]) input.val(null);
        }
    }

    this.hasChanges = function () {
        return !!model[property] && model[property] != values[0].code;
    }
}

