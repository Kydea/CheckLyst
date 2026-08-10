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
        readOnly = false,
        goToNext = 0,

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

        next = function (direction) {
            if (model.valueTypeName != "DateTime" && model.valueTypeName != "Time")
                setTimeout(function () {
                    var nextControl = (direction == 1 ? $("input:visible", $(element).next()) : $("input:visible", $(element).prev())).eq(0);
                    if (nextControl.length == 1)
                        nextControl.focus();
                    else {
                        var group = element;
                        while ((group = $(group).parent().closest(".Group, .Iteration")).length == 1) {
                            nextControl = (direction == 1 ? $("input:visible:first", group.next(":not(.ReadOnly)")) : $("input:visible:last", group.prev(":not(.ReadOnly)")));
                            if (nextControl.length == 1) {
                                nextControl.focus();
                                return;
                            }
                        }
                    }
                }, 0);
        },

        change = function () {
            if (!model.valuesList && (model.valueTypeName == "Date" || model.valueTypeName == "DateTime" || model.valueTypeName == "Time") && !matematici.TypedInput.validate(model.valueTypeName, input, select, select2))
                return;

            raiseChangeEvent(getValue());
        },

        raiseChangeEvent = function (value) {
            var handleGoToNext = goToNext;
            goToNext = 0;
            new instances.Event(instances.Events.valueChange, model.id, { value: value }).done(function (result) {
                //new instances.Event(instances.Events.valueChange, model.id, { value: value == undefined ? getValue() : value }).done(function (result) {
                instances.instance.update(result);
                if (handleGoToNext) next(handleGoToNext);
            }).trigger();

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

        scan = function () {
            new matematici.Scanner(raiseChangeEvent);
        },

        focus = function () {
            goToNext = 0;
        },

        keyPress = function (e) {
            matematici.TypedInput.keyPress(model.valueTypeName, e);
        },

        keyDown = function (e) {
            if (e.keyCode == 119 || e.key == "F8") {
                e.preventDefault();
                valueCopy();
                return;
            }

            if (e.keyCode == 120 || e.key == "F9") {
                e.preventDefault();
                scan();
                return;
            }

            if (e.keyCode == 9 || e.key == "Tab") {
                e.preventDefault();
                goToNext = e.shiftKey ? -1 : 1;
                input.blur();
                setTimeout(function () { if (goToNext) next(goToNext); }, 0);
                return;
            }
        },

        setup = function () {
            var actionButtons = [];
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
                    if (model.valueTypeName == "Date" || model.valueTypeName == "DateTime") {
                        element.classList.add(model.valueTypeName);
                        new matematici.DatePicker(input);
                    } else if (model.valueTypeName == "Boolean") {
                        element.classList.add("Boolean");
                        input.setAttribute("type", "checkbox");
                    } else if (model.valueTypeName != "Time") {
                        input.classList.add("Scan");
                        if (!matematici.appMode)
                            actionButtons.push({ label: "scan (F9)", tooltip: "F9", icon: "barcode-read", handler: scan });
                    }

                    input.addEventListener("focus", focus);
                    input.addEventListener("change", change);
                    input.addEventListener("keypress", keyPress);
                    input.addEventListener("keydown", keyDown);
                }

                if (model.valuesList || model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
                    select = document.createElement("select");
                    select.setAttribute("data-nullable", "-");
                    select.addEventListener("change", change);
                    (item.isTableContent() ? element : label).appendChild(select);
                }

                if (model.valuesList) {
                    element.classList.add("List");
                    for (var index in model.valuesList) {
                        var option = document.createElement("option");
                        option.text = option.value = model.valuesList[index];
                        select.add(option);
                    }
                } else if (model.externalSourceName && model.externalSourceColumn) {
                    var
                        icon = document.createElement("i"),
                        call = function () {
                            if (!element.classList.contains("Inactive"))
                                new matematici.ExternalSourcesSearchForm(model.id, model.externalSourceName, false);
                        };

                    element.classList.add("ExternalSource");
                    input.addEventListener("click", call);
                    icon.className = "ExternalSourceLink fa fa-link";
                    icon.addEventListener("click", call);
                    element.append(icon);
                } else if (model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
                    element.classList.add(model.valueTypeName);
                    for (var i = 0; i < 24; i++) {
                        var option = document.createElement("option");
                        option.text = (i < 10 ? "0" : "") + i;
                        option.value = (i < 10 ? "0" : "") + i;
                        select.add(option);
                    }

                    select2 = document.createElement("select");
                    select2.setAttribute("data-nullable", "-");
                    select2.addEventListener("change", change);
                    (item.isTableContent() ? element : label).appendChild(select2);

                    for (var i = 0; i < 60; i++) {
                        var option = document.createElement("option");
                        option.text = (i < 10 ? "0" : "") + i;
                        option.value = (i < 10 ? "0" : "") + i;
                        select2.add(option);
                    }
                }
                else //if (model.valueTypeName == "Date")
                    element.classList.add(model.valueTypeName);
            }

            field = document.createElement("div");
            field.className = "Input";
            (item.isTableContent() ? element : label).append(field);

            label.classList.toggle("Controls2", model.linesCount == -1);
            label.classList.toggle("FullWidth", model.linesCount > 0)
            element.classList.toggle("MultiLine", model.linesCount > 0);
            if (model.linesCount > 1) {
                input.style.height = 20 * model.linesCount + 12 + "px";
                field.style.height = 20 * model.linesCount + 12 + "px";
            }

            if (select) (autoComplete = new matematici.AutoComplete(select, { noCheck: model.valueTypeName == "DateTime" })).initialize();
            if (select2) (autoComplete2 = new matematici.AutoComplete(select2, { noCheck: model.valueTypeName == "DateTime" })).initialize();

            if (!item.isTableContent() || !item.getParent().isSubRow()) {
                if (!matematici.appMode)
                    actionButtons.push({
                        label: "copy (F8)", tootip: "F8", icon: "clone", handler: valueCopy,
                        check: function (e) {
                            var iteration = $(e.target).closest(".Iteration")
                            return iteration.length > 0 && !iteration.hasClass("FirstIteration");
                        }
                    });

                if (autoComplete) {
                    select.addEventListener("showList", function () { field.classList.add("Focused"); });
                    select.addEventListener("hideList", function () { field.classList.remove("Focused"); });
                }
                if (autoComplete2) {
                    select2.addEventListener("showList", function () { field.classList.add("Focused"); });
                    select2.addEventListener("hideList", function () { field.classList.remove("Focused"); });
                }
            }

            if (actionButtons.length > 0) {
                var toolButton = document.createElement("a");

                toolButton.className = "ActionButton fal fa-bolt";
                toolButton.setAttribute("href", "#");

                toolButton.addEventListener("mousedown", function (e) {
                    var
                        buttons = [
                            { text: "Cancel", icon: "times", cancelButton: true }
                        ],
                        signers = matematici.tokenManager.getSigners();

                    for (var index in actionButtons) {
                        var
                            actionButton = actionButtons[index],
                            button = {
                                text: actionButton.label, icon: actionButton.icon, title: actionButton.tooltip, handler: actionButton.handler
                            };
                        if (actionButton.check && !actionButton.check(e))
                            button.disabled = true;
                        buttons.push(button);
                    }

                    matematici.dialog.popupMenu(buttons)
                });
                (item.isTableContent() ? element : label).appendChild(toolButton);
            }

        };

    this.update = function (model) {
        var formattedDate = null;
        readOnly = !!model.readOnly || !!model.externalSourceName;
        span.innerHTML = model.descriptionValue;
        if (model.descriptionValue) span.setAttribute("title", model.descriptionValue);
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
        else if (model.externalSourceName && model.externalSourceColumn && model.valueTypeName != "Boolean")
            input.value = model.value;
        else
            matematici.TypedInput.setValue(model.valueTypeName, input, select, select2, model.value);

        var value = formattedDate ? formattedDate.date + " " + formattedDate.hours + ":" + formattedDate.minutes : model.value;
        field.innerHTML = readOnly && value ? value.split('\n').join("<br />") : value;

        if (autoComplete) autoComplete.setValue(select.value);
        if (autoComplete2) autoComplete2.setValue(select2.value);

        if (model.valueTypeName == "DateTime" || model.valueTypeName == "Date") {
            if (value)
                value += " UTC" + (!model.localTimeZone ? "" : (model.offset > 0 ? " +" : " -") + ("" + Math.abs(model.offset) / 60).padStart(2, "0") + ":00")
            else
                value = model.localTimeZone ? "Company time zone" : "UTC";
            field.setAttribute("title", value);
            input.setAttribute("title", value);
        } else
            field.setAttribute("title", field.textContent);

        if (model.valueTypeName == "Boolean")
            input.disabled = readOnly;

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (color) {
        label.style.color = color;
    };

    this.setFontSize = function (size) {
        label.style.fontSize = size && size + "px";
    };

    setup();
}