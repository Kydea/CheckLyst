prototypes.BooleanProperty = function (model, property, readOnly) {
    var
        input,
        handler,
        change = function () {
            model[property] = input.prop("checked");
            matematici.canClose = false;
            if (handler) handler.call(this, model);
        };

    this.setHandler = function (value) {
        handler = value;
        return this;
    };

    this.create = function (valuePanel) {
        valuePanel.addClass("Boolean");
        input = $("<input />")
            .attr({ type: "checkbox", readOnly: !!readOnly })
            .prop("checked", model[property]);
        if (!readOnly) input.on("change", change);
        else input.attr("disabled", "disabled");
        valuePanel.append(input);
    };

    this.hasChanges = function () {
        return !!model[property];
    };

    this.setReadOnly = function (value) {
        readOnly = value;
    }
};

