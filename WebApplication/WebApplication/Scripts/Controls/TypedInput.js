matematici.TypedInput = function (container, type, options) {
    var
        typedInput = this,
        input,
        hoursSelect,
        minutesSelect,
        dataPicker,

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

        setValue = function (value) {
            matematici.TypedInput.setValue(type, input, hoursSelect, minutesSelect, value);
        },

        setup = function () {
            var readOnly = options && options.readOnly;
            container.classList.add(type);
            if (type != "Time") {
                input = document.createElement("input");
                input.setAttribute("type", type == "Boolean" ? "checkbox" : "text");
                input.setAttribute("autocomplete", "off");
                input.addEventListener("keypress", function (e) {
                    matematici.TypedInput.keyPress(type, e);
                });
                input.addEventListener("change", function (e) {
                    matematici.TypedInput.validate(type, input, hoursSelect, minutesSelect);
                });
                if (readOnly) matematici.controls.setEditable($(input), false);
                if (options.maxLength) input.setAttribute("maxlength", options.maxLength);
                container.appendChild(input);

                if (type == "Date" || type == "DateTime")
                    dataPicker = new matematici.DatePicker(input);
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
                if (readOnly) matematici.controls.setEditable($(hoursSelect), false);
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
                if (readOnly) matematici.controls.setEditable($(minutesSelect), false);
                container.appendChild(minutesSelect);
                new matematici.AutoComplete(minutesSelect, { noCheck: type != "Time" }).initialize();
            }

            if (options && options.value) setValue(options.value);
        },

        destroy = function () {
            if (input) {
                input.remove();
                delete input;
            }

            if (hoursSelect) {
                var hoursAutoComplete = matematici.AutoComplete.get(hoursSelect);
                hoursAutoComplete.destroy();
                delete hoursAutoComplete;
                hoursSelect.remove();
                delete hoursSelect;
            }

            if (minutesSelect) {
                var minutesAutoComplete = matematici.AutoComplete.get(minutesSelect);
                minutesAutoComplete.destroy();
                delete minutesAutoComplete;
                minutesSelect.remove();
                delete minutesSelect;
            }

            if (dataPicker)
                dataPicker.destroy();

            container.classList.remove(type)
        };

    typedInput.getInput = function () {
        return input;
    };

    typedInput.getElements = function () {
        return elements;
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

    typedInput.setValue = setValue;
    typedInput.getValue = getValue;
    typedInput.destroy = destroy;

    setup();
};

matematici.TypedInput.setValue = function (type, input, hoursSelect, minutesSelect, value) {
    if (type == "DateTime") {
        if (!value || value == "") {
            input.value = "";
            hoursSelect.value = "";
            minutesSelect.value = "";
        } else {
            var
                index = value.indexOf(" "),
                date = value.substr(0, index),
                time = value.substr(index + 1),
                m = matematici.dateFormatter.parse(date, time, null);
            formattedDate = matematici.dateFormatter.format(m);
            input.value = formattedDate.date;
            hoursSelect.value = formattedDate.hours;
            minutesSelect.value = formattedDate.minutes;
        }
    } else if (type == "Time") {
        var index = (value || "").indexOf(":");
        if (index >= 0) {
            var hours = value.substr(0, index);
            hoursSelect.value = hours;
            var minutes = value.substr(index + 1);
            minutesSelect.value = minutes;
        } else {
            hoursSelect.value = "";
            minutesSelect.value = "";
        }
    } else if (type == "Boolean")
        input.checked = input.value = value && value.toLowerCase() == "true";
    else
        input.value = value;
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