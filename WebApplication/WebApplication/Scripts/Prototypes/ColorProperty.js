prototypes.ColorProperty = function (model, property, options) {
    var
        base,
        select,
        input,

        values = [
            { label: "(Default)", code: "" },
            { label: "(Formula)", code: "-" },
            { label: "Black", code: "#000" },
            { label: "Red", code: "#cc0000" },
            { label: "Cyan", code: "#e8f7fc" },
            { label: "Green", code: "#009688" },
            { label: "Grey", code: "#999" },
            { label: "Yellow", code: "#ffba00" },
        ],

        change = function () {
            var value = select.val();
            input.toggle(value == "-");
            model[property] = value == "-" ? input.val() : value;
            base.updateModel(model);
        },

        getDescription = function () {
            var value = model[property];
            if (value == "" || value == null) return "";

            for (var index in values)
                if (values[index].code == value) 
                    return values[index].label;

            return value;
        };

    this.create = function (valuePanel, baseClass) {
        if (!options) options = {};
        var value = model[property];
        base = baseClass;
        valuePanel.addClass("Expression");

        if (options.readOnly || prototypes.prototype.getModel().state != prototypes.Prototype.states.draft) 
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(getDescription())
                .appendTo(valuePanel);
        else {
            select = $("<select />");
            select.empty();
            var custom = true;
            for (var index in values) {
                var option = values[index];
                select.append($("<option />").attr({ value: option.code, selected: value == option.code }).text(option.label));
                if (value == option.code) custom = false;
            }
            if (custom) select.val("-");
            select.on("change", change);
            valuePanel.append(select);

            input = $("<input />")
                .attr({ type: "text" })
                .val(value)
                .on("change", change)
                .appendTo(valuePanel)
                .toggle(custom);
        }
    }

    this.hasChanges = function () {
        return model[property] != null && model[property] != "";
    }

    this.setValue = function (node) {
        node.text(getDescription());
    };

    this.setReadOnly = function (value) {
        options.readOnly = value;
    };
}

