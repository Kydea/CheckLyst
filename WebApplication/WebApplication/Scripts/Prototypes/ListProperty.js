prototypes.ListProperty = function (model, property, values, options) {
    var
        input,
        handler,
        check,
        expressions,

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
            expressions = prototypes.prototype.getModel().Expressions;
            for (var index in values)
                input.append($("<option />").attr({ value: values[index].code, selected: values[index].code == model[property] }).text(values[index].label));
            input.on("change", change);
            valuePanel.append(input);
        }
    }

    this.hasChanges = function () {
        return !!model[property] && model[property] != values[0].code;
    }
}

