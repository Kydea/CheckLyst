instances.Field = function (item, model) {
    var
        input,
        select,
        autoComplete,
        select2,
        autoComplete2,
        field,
        label,
        span,
        element = item.getElement(),
        copyButton,
        readOnly = false,

        validate = function () {
            if (model.valueTypeName == "DateTime" || model.valueTypeName == "Date") {
                if (!/^$|\d\d\d\d\-\d\d\-\d\d/ig.test(input.val())) {
                    input.val("");
                    return false;
                }
            } else if (model.valueTypeName == "Number") {
                var value = input.value;
                for (var index = 0; index < value.length; ++index)
                    if (!(
                        value[index] >= "0" && value[index] <= "9"
                        || value[index] == "-"
                        || value[index] == localStorage.decimalSeparator
                    )) {
                        input.value = "";
                        return false;
                    }
            }

            if (model.valueTypeName == "DateTime")
                return (
                    ((input.value || "").trim() != "" && (select.value || "").trim() != "" && (select2.value || "").trim() != "")
                    || ((input.value || "").trim() == "" && (select.value || "").trim() == "" && (select2.value || "").trim() == "")
                );
            else if (model.valueTypeName == "Time")
                return (
                    ((select.value || "").trim() != "" && (select2.value || "").trim() != "")
                    || ((select.value || "").trim() == "" && (select2.value || "").trim() == "")
                );

            return true;
        },

        getValue = function () {
            var value = model.valuesList ? select.value || "" : (input.value || "").trim();
            if (!model.valuesList) {
                if (value != "" && model.valueTypeName == "DateTime") {
                    var
                        momentDate = matematici.dateFormatter.parse(value, select.value + ":" + select2.value, null),
                        formattedDate = matematici.dateFormatter.format(momentDate);

                    value = formattedDate.date + " " + formattedDate.time;
                } else if (model.valueTypeName == "Time")
                    value = select.value != "" && select2.value != "" ? select.value + ":" + select2.value : "";
                else if (model.valueTypeName == "Boolean")
                    value = input.checked ? "true" : "false";
            }
            return value;
        },

        next = function () {
            if (model.valueTypeName != "DateTime" && model.valueTypeName != "Time")
                setTimeout(function () {
                    $("input:visible", $(element).next()).eq(0).focus();
                }, 0);
        },

        change = function () {
            if (model.valuesList || validate()) {
                new instances.Event(instances.Events.valueChange, model.id, { value: getValue() }).done(function (result) {
                    instances.instance.update(result);
                    next();
                }).trigger();
            }
        },

        valueCopy = function (e) {
            if (e) {
                e.preventDefault();
                e.stopPropagation();
            }
            new instances.Event(instances.Events.valueCopy, model.id).done(function (result) {
                instances.instance.update(result);
                next();
            }).trigger();
        },

        keyPress = function (e) {
            //f8
            if (e.which == 119) {
                e.preventDefault();
                valueCopy();
                return;
            }

            if (
                e.ctrlKey || e.altKey
                || e.key == '-'
                || e.key >= '0' && e.key <= '9'
            )
                return;

            if (model.valueTypeName == "Number" && (e.key == ',' || e.key == '.') && e.key != localStorage.decimalSeparator) {
                e.preventDefault();
                matematici.dialog.error("Error", "Invalid decimal separator, use the character " + localStorage.decimalSeparator + " instead");
                return;
            } 

            if (model.valueTypeName == "Number" || model.valueTypeName == "Date" || model.valueTypeName == "DateTime")
                e.preventDefault();
        },

        setup = function () {
            label = document.createElement("label");
            span = document.createElement("span");
            span.className = "Label";
            label.appendChild(span);

            if (!item.isTableContent())
                element.appendChild(label)
            if (model.linesCount < 2) {
                input = document.createElement("input");
                input.setAttribute("type", "text");
            } else
                input = document.createElement("textarea");
            input.className = "Description";
            (item.isTableContent() ? element : label).append(input);

            if (!model.valueFormulaString) {
                if (!model.valuesList) {
                    if (model.valueTypeName == "Date" || model.valueTypeName == "DateTime") 
                        new matematici.DatePicker(input);
                    else if (model.valueTypeName == "Boolean") {
                        element.className += " Boolean";
                        input.setAttribute("type", "checkbox");
                    }
                    else
                        input.className += " Scan";

                    input.addEventListener("change", change);
                    input.addEventListener("keypress", keyPress);
                }

                if (model.valuesList || model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
                    select = document.createElement("select");
                    select.setAttribute("data-nullable", "true");
                    select.addEventListener("change", change)
                    select.addEventListener("keypress", keyPress);
                    (item.isTableContent() ? element : label).appendChild(select);
                }

                if (model.valuesList) {
                    element.className += " List";
                    for (var index in model.valuesList)
                        select.append($("<option />").attr("value", model.valuesList[index]).text(model.valuesList[index]));
                } else if (model.externalSourceName && model.externalSourceColumn) {
                    var
                        icon = document.createElement("i"),
                        call = function () {
                            if (!element.classList.contains("Inactive"))
                                new instances.ExternalSources(model.id, model.externalSourceName);
                        };

                    element.className += " ExternalSource";
                    input.addEventListener("click", call);
                    icon.className = "ExternalSourceLink fa fa-link";
                    icon.addEventListener("click", call);
                    element.append(icon);
                } else if (model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
                    element.className += " " + model.valueTypeName;
                    for (var i = 0; i < 24; i++) {
                        var option = document.createElement("option");
                        option.text = (i < 10 ? "0" : "") + i;
                        option.value = (i < 10 ? "0" : "") + i;
                        select.add(option);
                    }

                    select2 = document.createElement("select");
                    select2.setAttribute("data-nullable", "true");
                    select2.addEventListener("change", change)
                    select2.addEventListener("keypress", keyPress);
                    (item.isTableContent() ? element : label).appendChild(select2);

                    for (var i = 0; i < 60; i++) {
                        var option = document.createElement("option");
                        option.text = (i < 10 ? "0" : "") + i;
                        option.value = (i < 10 ? "0" : "") + i;
                        select2.add(option);
                    }
                }
                else if (model.valueTypeName == "Date")
                    element.calssName += model.valueTypeName;
            }

            field = document.createElement("div");
            field.className = "Input";
            (item.isTableContent() ? element : label).append(field);

            if (model.linesCount > 0)
                element.className += " MultiLine";
            if (model.linesCount > 1) {
                input.style.height = 20 * model.linesCount + 12 + "px";
                field.style.height = 20 * model.linesCount + 12 + "px";
            }

            copyButton = document.createElement("a");
            copyButton.className = "Copy fal fa-clone";
            copyButton.setAttribute("href", "#");
            copyButton.setAttribute("title", "F8");
            copyButton.addEventListener("mousedown", valueCopy);

            if (!item.isTableContent() || !item.getParent().isSubRow())
                (item.isTableContent() ? element : label).appendChild(copyButton);

            if (select) autoComplete = new matematici.InstanceAutoComplete(select).initialize();
            if (select2) autoComplete2 = new matematici.InstanceAutoComplete(select2).initialize();
        };

    this.update = function (model) {
        var formattedDate = null;
        readOnly = !!model.readOnly || !!model.externalSourceName;
        span.innerHTML = model.descriptionValue;
        element.classList.toggle("Inactive", model.readOnly);
        element.classList.toggle("ReadOnly", readOnly);
        input.readOnly = readOnly;
        if (readOnly) 
            field.setAttribute("readonly", "readonly");
        else
            field.removeAttribute("readonly");
        if (model.valuesList)
            select.value = model.value;
        else if (model.valueFormulaString)
            input.value = model.value;
        else if (model.valueTypeName == "DateTime") {
            if (!model.value || model.value == "") {
                input.value = "";
                select.value = "";
                select2.value = "";
            } else {
                var
                    index = model.value.indexOf(" "),
                    date = model.value.substr(0, index),
                    time = model.value.substr(index + 1),
                    m = matematici.dateFormatter.parse(date, time, null);
                formattedDate = matematici.dateFormatter.format(m);
                input.val(formattedDate.date);
                select.val(formattedDate.hours);
                select2.val(formattedDate.minutes);
            }
        } else if (model.valueTypeName == "Time") {
            input.val(model.value);
            var index = (model.value || "").indexOf(":");
            if (index >= 0) {
                var hours = model.value.substr(0, index);
                select.val(hours);
                var minutes = model.value.substr(index + 1);
                select2.val(minutes);
            } else {
                select.value = "";
                select2.value = "";
            }
        } else if (model.valueTypeName == "Boolean") {
            input.checked = input.value = model.value && model.value.toLowerCase() == "true";
            input.disabled = readOnly;
        } else
            input.value = model.value;

        var value = formattedDate ? formattedDate.date + " " + formattedDate.hours + ":" + formattedDate.minutes : model.value;
        field.innerHTML = readOnly && value ? value.split('\n').join("<br />") : value;

        if (model.valueTypeName == "DateTime" || model.valueTypeName == "Date")
            if (value)
                value += " UTC" + (!model.localTimeZone ? "" : (model.offset > 0 ? " +" : " -") + ("" + Math.abs(model.offset) / 60).padStart(2, "0") + ":00")
            else
                value = model.localTimeZone ? "Company time zone" : "UTC";
        field.setAttribute("title", field.textContent);

        if (autoComplete) autoComplete.setValue(select.value);
        if (autoComplete2) autoComplete2.setValue(select2.value);

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function () {
        label.style.color = model.color;
    };

    this.setFontSize = function () {
        label.style.fontSize = model.fontSize && model.fontSize + "px";
    };

    setup();
}