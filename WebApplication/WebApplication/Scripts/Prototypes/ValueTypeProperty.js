prototypes.ValueTypeProperty = function (model, property, options) {
    var
        input,
        handler,
        setupValues,
        check,
        radioUTC,
        radioLocal,
        defaultValueLabel,
        defaultValue,

        values = [
            { label: "Text", code: "Text" },
            { label: "Boolean", code: "Boolean" },
            { label: "Number", code: "Number" },
            { label: "Date Time", code: "DateTime" },
            { label: "Date", code: "Date" },
            { label: "Time", code: "Time" }
        ],

        defaultValueKeyDown = function (e) {
            if (model[property] == "Number" && (e.key == "." || e.key == ",") && e.key != localStorage.decimalSeparator)
                e.preventDefault();
        },

        defaultValueChange = function () {
        },

        setDefaultValueField = function () {
            if (defaultValue) defaultValue.remove();

            defaultValue =
                true ?
                    $("<input />").attr({ type: "text" }).attr('disabled', options.readOnly).keydown(defaultValueKeyDown).change(defaultValueChange) : //gestire input specifici a seconda di model[property]
                    null;

            if (setupValues.valueType == model[property]) defaultValue.val(setupValues.defaultValue);
            if (!options.readOnly)
                defaultValue.on("change", function () {
                    model.defaultValue = defaultValue.val();
                });
            defaultValueLabel.append(defaultValue);
        },

        toggleRadioButtons = function () {
            var value = model[property] && model[property].indexOf("Date") == 0;
            radioUTC.toggle(value);
            radioLocal.toggle(value);
            setDefaultValueField();
            return value;
        },

        change = function () {
            model[property] = input.val();
            matematici.canClose = false;
            var radioVisible = toggleRadioButtons();
            model.localTimeZone = radioVisible && $("input", radioLocal).prop("checked"); 
            setDefaultValueField();
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
        valuePanel.addClass("ValueType");
        setupValues = {
            valueType: model[property],
            defaultValue: model.defaultValue
        };

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

        radioUTC = $("<label />")
            .text("UTC")
            .prepend($("<input />").attr({ type: "radio", name: "kind" }).prop("checked", !model.localTimeZone).attr('disabled', options.readOnly))
            .appendTo(valuePanel);

        radioLocal = $("<label />")
            .text("Company time zone")
            .prepend($("<input />").attr({ type: "radio", name: "kind" }).prop("checked", model.localTimeZone).attr('disabled', options.readOnly))
            .appendTo(valuePanel);

        if (!options.readOnly) {
            radioUTC.change(change);
            radioLocal.change(change);
        }

        defaultValueLabel = $("<label />")
            .text("default value")
            .appendTo(valuePanel);

        setDefaultValueField();

        toggleRadioButtons();
    }

    this.hasChanges = function () {
        return !!model[property] && model[property] != values[0].code && model[property] != values[0] || !!model.defaultValue;
    }

    this.refresh = function (newValues) {
        values = newValues;
    }
}

