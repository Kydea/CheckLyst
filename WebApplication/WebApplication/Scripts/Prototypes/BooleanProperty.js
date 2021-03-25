prototypes.BooleanProperty = function (model, property, readOnly, defaultValue) {
    var
        base,
        input,

        change = function () {
            model[property] = input.prop("checked");
            base.updateModel(model);
        };

    this.create = function (valuePanel, baseClass) {
        var _readOnly = readOnly || prototypes.prototype.getModel().state != prototypes.Prototype.states.draft;
        base = baseClass;
        valuePanel.addClass("Boolean");
        input = $("<input />")
            .attr({ type: "checkbox", readOnly: _readOnly })
            .prop("checked", model[property]);
        if (!_readOnly) input.on("change", change);
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

