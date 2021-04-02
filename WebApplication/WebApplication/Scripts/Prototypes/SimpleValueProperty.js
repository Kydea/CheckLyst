prototypes.SimpleValueProperty = function (model, property, options) {
    var
        base,
        input,
        check,

        change = function () {
            var value = input.val();
            if (value == "" && options.number) value = 0;
            if (check)
                for (var index in check)
                    if (!check[index].action.call(this, value)) {
                        new matematici.dialog.error("error", check[index].message);
                        return;
                    }
            model[property] = value;
            base.updateModel(model);
            if (property == "name" && value && model.errors) 
                base.setError(model.errors, "name", null);
            if (property == "code")
                base.setError(model.errors, "code", null);
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("SimpleValue");
        options = options || {};
		input = $(options.fullHeight ? "<textarea />" : "<input />")
			.attr({ type: "text", maxlength: options.maxLength, readOnly: options.readOnly })
			.val(model[property]);
        if (!options.readOnly)
            input.on("change", change);
		if (options.number)
			input.keyfilter(/[\d]/);
		else if (options.code)
            input.keyfilter(/^[$A-Z_0-9]*$/i);

        if (options.number && !options.defaultValue)
            options.defaultValue = "0";
        valuePanel.append(input);
    };

    this.hasChanges = function () {
        return !options.defaultValue && !!model[property] || !!options.defaultValue && model[property] != options.defaultValue;
    };

    this.setReadOnly = function (value) {
        options.readOnly = value
    };

    this.setValue = function (node) {
        node.text(this.hasChanges() ? model[property] : "");
    };

    this.setError = function(handler) {
        if (property == "name" || property == "code") handler(model.errors, property);
    };
}