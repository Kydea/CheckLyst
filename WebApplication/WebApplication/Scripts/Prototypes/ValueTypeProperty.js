prototypes.ValueTypeProperty = function (model, property, options) {
    var
        base,
        input,
        setupValues,
        radioUTC,
        radioLocal,
        defaultValueLabel,
        defaultValue,
        precisionInput,

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
                    $("<input />").attr({ type: "text" }).attr('readonly', options.readOnly).keydown(defaultValueKeyDown).change(defaultValueChange) : //gestire input specifici a seconda di model[property]
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
            var
                value = model[property] = input.val(),
                number = value == "Number",
                radioVisible = toggleRadioButtons();
            model.localTimeZone = radioVisible && $("input", radioLocal).prop("checked"); 
            setDefaultValueField();
            precisionInput.parent().toggle(number);
            if (!number) model.precision = null;
            base.updateModel(model);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
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
            for (var index in values) {
                var
                    value = values[index],
                    code = value.code,
                    label = value.label;
                input.append($("<option />").attr({ value: code, selected: code == model[property] }).text(label));
            }
            input.on("change", change);
            valuePanel.append(input);
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

        precisionInput = $("<input />")
            .attr("type", "text")
            .keyfilter(/^[0-9]*$/i)
            .val(model.precision)
            .on("change", function () { model.precision = this.value; });

        $("<label />")
            .addClass("Precision")
            .text("Allowed decimals")
            .appendTo(valuePanel)
            .append(precisionInput);

        defaultValueLabel = $("<label />")
            .addClass("DefaultValue")
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

