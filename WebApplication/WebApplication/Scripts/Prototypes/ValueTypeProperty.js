prototypes.ValueTypeProperty = function (model, property, options) {
    var
        base,
        input,
        setupValues,
        radioUTC,
        radioLocal,
        radioDecimal,
        radioScientific,
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

        toggleControls = function () {
            var
                dateMode = model[property] == "Date" || model[property] == "DateTime",
                numberMode = model[property] == "Number";

            radioUTC.toggle(dateMode);
            radioLocal.toggle(dateMode);
            radioDecimal.toggle(numberMode);
            radioScientific.toggle(numberMode);

            precisionInput.parent().toggle(numberMode);

            setDefaultValueField();
        },

        change = function () {
            var
                value = model[property] = input.val(),
                dateMode = model[property] == "Date" || model[property] == "DateTime",
                numberMode = value == "Number";

            toggleControls();
            model.localTimeZone = dateMode && $("input", radioLocal).prop("checked");
            model.scientificNotation = numberMode && $("input", radioScientific).prop("checked"); 

            if (!numberMode) model.precision = null;
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

        radioDecimal = $("<label />")
            .text("Decimal notation")
            .prepend($("<input />").attr({ type: "radio", name: "notation" }).prop("checked", !model.scientificNotation).attr('disabled', options.readOnly))
            .appendTo(valuePanel);

        radioScientific = $("<label />")
            .text("Scientific notation")
            .prepend($("<input />").attr({ type: "radio", name: "notation" }).prop("checked", model.scientificNotation).attr('disabled', options.readOnly))
            .appendTo(valuePanel);

        if (!options.readOnly) {
            radioUTC.change(change);
            radioLocal.change(change);
            radioDecimal.change(change);
            radioScientific.change(change);
        }

        precisionInput = $("<input />")
            .attr({ type: "text", readOnly: options.readOnly })
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
            .text("Default value")
            .appendTo(valuePanel);

        setDefaultValueField();

        toggleControls();
    }

    this.hasChanges = function () {
        return !!model[property] && model[property] != values[0].code && model[property] != values[0] || !!model.defaultValue;
    }

    this.refresh = function (newValues) {
        values = newValues;
    }
}

