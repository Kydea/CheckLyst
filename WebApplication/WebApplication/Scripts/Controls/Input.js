matematici.input = function (container, type, options) {
    var
        input = this,
        field,
        hoursSelect,
        minutesSelect,

        getValue = function () {
            return field.value;
        }

        setup = function () {
            if (type != "time") {
                field = document.createElement("input");
                field.setAttribute("type", type == "Boolean" ? "checkbox" : "text");
                container.appendChild(field);

                if (type == "Date" || type == "DateTime")
                    new matematici.DatePicker(field);
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
                container.appendChild(hoursSelect);
                new matematici.AutoComplete(hoursSelect).initialize();

                minutesSelect = document.createElement("select");
                minutesSelect.setAttribute("data-nullable", "-");
                for (var i = 0; i < 60; i++) {
                    var option = document.createElement("option");
                    option.text = (i < 10 ? "0" : "") + i;
                    option.value = (i < 10 ? "0" : "") + i;
                    minutesSelect.add(option);
                }
                container.appendChild(minutesSelect);
                new matematici.AutoComplete(minutesSelect).initialize()
            }
        };

    input.getField = function () {
        return field;
    };

    input.getHoursSelect = function () {
        return hoursSelect;
    };

    input.getMinutesSelect = function () {
        return minutesSelect;
    };

    input.getOption = function (name) {
        return options && options[name];
    };

    input.getValue = getValue;

    setup();
};