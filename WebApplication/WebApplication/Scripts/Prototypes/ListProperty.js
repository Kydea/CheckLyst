prototypes.ListProperty = function (model, property, values, options) {
    var
        base,
        input,

        change = function () {
            model[property] = input.val();
            base.updateModel(model);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Expression");

        if (options.readOnly) {
            var label = $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(model[property])
                .appendTo(valuePanel);

            for (var index in values)
                if (values[index].code == model[property])
                    label.val(values[index].label);
        } else {
            input = $("<select />");
            if (!!options.nullable) input.attr("data-nullable", "");
            input.empty();
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
        var value = !!options.nullable && !!model[property]
            || !options.nullable && !!model[property] && model[property] != values[0].code && model[property] != values[0];
        return value;
    }

    this.refresh = function (newValues) {
        values = newValues;
    }

    this.setValue = function (node) {
        var label = options.nullable ? "" : values[0].label || values[0];
        if (this.hasChanges())
            for (var index in values)
                if (values[index] == model[property] || values[index].code == model[property])
                    label = values[index].label || values[index];
        node.text(label);
    }
}

