prototypes.ValueTypeProperty = function (model, property, options) {
    var
        input,
        handler,
        check,
        radioUTC,
        radioLocal,

        values = [
            { label: "Text", code: "Text" },
            { label: "Boolean", code: "Boolean" },
            { label: "Number", code: "Number" },
            { label: "Date Time", code: "DateTime" },
            { label: "Date", code: "Date" },
            { label: "Time", code: "Time" },
        ],

        toggleRadioButtons = function () {
            var value = model[property] && model[property].indexOf("Date") == 0;
            radioUTC.toggle(value);
            radioLocal.toggle(value)
            return value;
        },

        change = function () {
            model[property] = input.val();
            matematici.canClose = false;
            var radioVisible = toggleRadioButtons();
            model.localTimeZone = radioVisible && $("input", radioLocal).prop("checked"); 
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
            if (!!options.nullable) input.data("nullable", true);
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

        toggleRadioButtons();
    }

    this.hasChanges = function () {
        return !!model[property] && model[property] != values[0].code && model[property] != values[0];
    }

    this.refresh = function (newValues) {
        values = newValues;
    }
}

