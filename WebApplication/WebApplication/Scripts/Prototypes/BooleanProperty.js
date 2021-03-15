prototypes.BooleanProperty = function (model, property, readOnly, defaultValue) {
    var
        base,
        input,

        change = function () {
            model[property] = input.prop("checked");
            base.updateModel(model);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Boolean");
        input = $("<input />")
            .attr({ type: "checkbox", readOnly: !!readOnly })
            .prop("checked", model[property]);
        if (!readOnly) input.on("change", change);
        else input.attr("disabled", "disabled");
        valuePanel.append(input);
    };

    this.hasChanges = function () {
        return !!model[property] == !defaultValue;
    };

    this.setReadOnly = function (value) {
        readOnly = value;
    }

    this.setValue = function (node) {
        if (model[property])
            node.append($("<i>").addClass("fal fa-check"));
    }
};

