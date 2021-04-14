matematici.TypedInput = function (container, type, options) {
    var
        typedInput = this,
        input,
        hoursSelect,
        minutesSelect,

        getValue = function () {
            if (!matematici.TypedInput.validate(type, input, hoursSelect, minutesSelect)) return null;

            var value = input && (input.value || "").trim();

            if (value != "" && type == "DateTime") {
                var
                    momentDate = matematici.dateFormatter.parse(value, hoursSelect.value + ":" + minutesSelect.value, null),
                    formattedDate = matematici.dateFormatter.format(momentDate);

                value = formattedDate.date + " " + formattedDate.time;
            } else if (type == "Time")
                value = hoursSelect.value != "" && minutesSelect.value != "" ? hoursSelect.value + ":" + minutesSelect.value : "";
            else if (type == "Boolean")
                value = input.checked ? "true" : "false";

            return value;
        },

        setup = function () {
            container.classList.add(type);
            if (type != "Time") {
                input = document.createElement("input");
                input.setAttribute("type", type == "Boolean" ? "checkbox" : "text");
                input.addEventListener("keypress", function (e) {
                    matematici.TypedInput.keyPress(type, e);
                });
                input.addEventListener("change", function (e) {
                    matematici.TypedInput.validate(type, input, hoursSelect, minutesSelect);
                });
                container.appendChild(input);

                if (type == "Date" || type == "DateTime")
                    new matematici.DatePicker(input);
            }

            if (type == "DateTime" || type == "Time") {
                hoursSelect = document.createElement("select");
                hoursSelect.setAttribute("data-nullable", "-");
                for (var i = 0; i < 24; i++) {
                    var option = document.createElement("option");
                    option.text = (i < 10 ? "0" : "") + i;
                    option.value = (i < 10 ? "0" : "") + i;
                    hoursSelect.add(option);
                }
                hoursSelect.addEventListener("change", function (e) {
                    matematici.TypedInput.validate(type, input, hoursSelect, minutesSelect);
                });
                hoursSelect.value = null;
                container.appendChild(hoursSelect);
                new matematici.AutoComplete(hoursSelect, { noCheck: type != "Time" }).initialize();

                minutesSelect = document.createElement("select");
                minutesSelect.setAttribute("data-nullable", "-");
                for (var i = 0; i < 60; i++) {
                    var option = document.createElement("option");
                    option.text = (i < 10 ? "0" : "") + i;
                    option.value = (i < 10 ? "0" : "") + i;
                    minutesSelect.add(option);
                }
                minutesSelect.addEventListener("change", function (e) {
                    matematici.TypedInput.validate(type, input, hoursSelect, minutesSelect);
                });
                minutesSelect.value = null;
                container.appendChild(minutesSelect);
                new matematici.AutoComplete(minutesSelect, { noCheck: type != "Time" }).initialize();
            }
        };

    typedInput.getInput = function () {
        return input;
    };

    typedInput.getHoursSelect = function () {
        return hoursSelect;
    };

    typedInput.getMinutesSelect = function () {
        return minutesSelect;
    };

    typedInput.getOption = function (name) {
        return options && options[name];
    };

    typedInput.validate = function () {
        return matematici.TypedInput.validate(type, input, hoursSelect, minutesSelect, true);
    };

    typedInput.getValue = getValue;

    setup();
};

matematici.TypedInput.toggleError = function (input, hoursSelect, minutesSelect, valid) {
    input && input.classList.toggle("Error", !valid);
    hoursSelect && matematici.AutoComplete.get(hoursSelect).getLabelNode().classList.toggle("Error", !valid);
    minutesSelect && matematici.AutoComplete.get(minutesSelect).getLabelNode().classList.toggle("Error", !valid);
};

matematici.TypedInput.validate = function (type, input, hoursSelect, minutesSelect, toggleError) {
    var valid = true;

    if (type == "DateTime" || type == "Date") {
        if (!/^$|\d\d\d\d\-\d\d\-\d\d$/ig.test(input.value)) {
            valid = false;
            input.value = "";
        }
    } else if (type == "Number") {
        input.value = (input.value || "").trim();
        if (input.value != "" && (
            localStorage.decimalSeparator == '.' && !/^-?\d*\.?\d+(e[+-]?\d+)?$/ig.test(input.value)
            || localStorage.decimalSeparator == ',' && !/^-?\d*\,?\d+(e[+-]?\d+)?$/ig.test(input.value)
        ))
            valid = false;
    }

    if (valid) {
        if (type == "DateTime")
            valid =
                ((input.value || "").trim() != "" && (hoursSelect.value || "").trim() != "" && (minutesSelect.value || "").trim() != "")
                || ((input.value || "").trim() == "" && (hoursSelect.value || "").trim() == "" && (minutesSelect.value || "").trim() == "");
        else if (type == "Time")
            valid =
                ((hoursSelect.value || "").trim() != "" && (minutesSelect.value || "").trim() != "")
                || ((hoursSelect.value || "").trim() == "" && (minutesSelect.value || "").trim() == "");
    }

    if (toggleError || valid) matematici.TypedInput.toggleError(input, hoursSelect, minutesSelect, valid);

    return valid;
};

matematici.TypedInput.keyPress = function (type, e) {
    if (
        e.ctrlKey || e.altKey
        || e.key == '-'
        || e.key == '+'
        || e.key >= '0' && e.key <= '9'
        || (e.key == 'e' || e.key == 'E' || e.key == localStorage.decimalSeparator) && type == "Number"
    )
        return;

    if (type == "Number" && (e.key == ',' || e.key == '.')) {
        e.preventDefault();
        matematici.dialog.error("Error", "Invalid decimal separator, use the character " + localStorage.decimalSeparator + " instead");
        return;
    }

    if (type == "Number" || type == "Date" || type == "DateTime")
        e.preventDefault();
};