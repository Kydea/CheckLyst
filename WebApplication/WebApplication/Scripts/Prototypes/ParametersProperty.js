prototypes.ParametersProperty = function (model) {
    var
        base,
        readOnly = prototypes.prototype.getModel().state != prototypes.Prototype.states.draft,
        tableBody,

        bindingChanged = function () {
            var
                value = this.value,
                codes = [],
                codeSelect = $("select", this.parentElement.nextElementSibling),
                autoComplete = matematici.AutoComplete.get(codeSelect[0]);

            codeSelect.empty();
            if (value == "expression") {
                var expressions = prototypes.prototype.getModel().expressions;
                for (var expressionIndex in expressions)
                    codes.push(expressions[expressionIndex].code);
            } else if (value == "field") {
                var fields = prototypes.prototype.getElements("Field");
                for (var fieldIndex in fields)
                    codes.push(fieldIndex);
            }

            if (value)
                for (var index in codes) {
                    var option = document.createElement("option");
                    option.setAttribute("value", codes[index]);
                    option.textContent = codes[index];
                    codeSelect.append(option);
                }

            autoComplete.refreshValues();
        },

        addParameter = function (name, expressionCode, elementCode, value) {
            if (readOnly) 
                tableBody.append($("<tr />")
                    .append($("<td />").text(name))
                    .append($("<td />").text(expressionCode ? "expression" : elementCode ? "field" : "free value"))
                    .append($("<td />").text(expressionCode || elementCode || value))
                    .append($("<td />"))
                );
            else {
                var
                    nameInput = $("<input />").addClass("Name").attr({ type: "text" }).val(name),
                    bindingSelect = $("<select />").addClass("Binding"),
                    valueInput = $("<input />").addClass("Value").attr({ type: "text" }).val(value).hide(),
                    valueSelect = $("<select />").addClass("Value"),
                    deleteButton = $("<a />").append($("<i />").addClass("fal fa-trash-alt")).attr("href", "#");

                bindingSelect
                    .append($("<option />").attr("value", "free").text("free value"))
                    .append($("<option />").attr("value", "expression").text("expression"))
                    .append($("<option />").attr("value", "field").text("field"));

                tableBody.append($("<tr />")
                    .append($("<td />").append(nameInput))
                    .append($("<td />").append(bindingSelect))
                    .append($("<td />").append(valueInput).append(valueSelect))
                    .append($("<td />").append(deleteButton))
                );

                setTimeout(function () {
                    var bingingAutoComplete = matematici.AutoComplete.get(bindingSelect[0]);
                    if (!bingingAutoComplete) {
                        bingingAutoComplete = new matematici.AutoComplete(bindingSelect[0]);
                        bingingAutoComplete.initialize();
                    }
                    bingingAutoComplete.toggle(!!name);
                    bingingAutoComplete.setValue(expressionCode ? "expression" : elementCode ? "field" : "free");
                    if (expressionCode || elementCode) bindingChanged.call(bindingSelect[0]);

                    var valueAutoComplete = matematici.AutoComplete.get(valueSelect[0]);
                    if (!valueAutoComplete) {
                        valueAutoComplete = new matematici.AutoComplete(valueSelect[0]);
                        valueAutoComplete.initialize();
                    }
                    valueAutoComplete.toggle(!!name && (expressionCode || elementCode));
                    var code = expressionCode || elementCode;
                    valueAutoComplete.setValue(code);
                    if (code && valueAutoComplete.getValue() != code)
                        valueAutoComplete.setError(code);

                    valueInput.toggle(!!name && !expressionCode && !elementCode);
                    updateModel();
                }, 0);
            }
        },

        updateModel = function () {
            model.parameters = [];
            $("tr", tableBody).each(function () {
                var
                    parameter = { name: $(".Name", this).val() },
                    binding = $(".Binding", this).val(),
                    selectValue = $("select.Value", this).val(),
                    inputValue = $("input.Value", this).val();

                if (parameter.name && parameter.name != "") {
                    switch (binding) {
                        case "expression":
                            parameter.expressionCode = selectValue;
                            break;
                        case "field":
                            parameter.elementCode = selectValue;
                            break;
                        default:
                            parameter.value = inputValue;
                            break;
                    }
                    model.parameters.push(parameter);
                }
            });
            if (!model.parameters.length) model.parameters = null;
            base.updateModel()
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Parameters");
        if (!model.prototypeName)
            valuePanel.text("No prototype is selected.");
        else if (model.actions == 2)
            valuePanel.text("The instance creation action is not enabled.");
        else {
            var table = $("<table />")
                .appendTo(valuePanel)
                .addClass("Grid")
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />").text("parameter"))
                        .append($("<th />").text("binding"))
                        .append($("<th />").text("bound to"))
                        .append($("<th />"))
                    )
                )
                .append(tableBody = $("<tbody />"));
            if (!readOnly)
                table
                    .on("click", "a", function () {
                        $(this).parent().parent().remove();
                        updateModel();
                    })
                    .on("keydown paste", "tr:last-child input", function () {
                        var
                            tr = $(this).parent().parent(),
                            valueAutoComplete = matematici.AutoComplete.get($("select.Binding", tr)[0]);

                        valueAutoComplete.toggle(true);
                        $("input.Value", tr).toggle(true);

                        setTimeout(function () {
                            valueAutoComplete.refresh();
                        }, 0);

                        addParameter();
                    })
                    .on("change", "select.Binding", function () {
                        var
                            bindingSelect = $(this),
                            tr = bindingSelect.parent().parent(),
                            valueAutoComplete = matematici.AutoComplete.get($("select.Value", tr)[0]),
                            binding = bindingSelect.val();

                        valueAutoComplete.toggle(binding != "free");
                        $("input.Value", tr).toggle(binding == "free");

                        setTimeout(function () {
                            if (binding != "free") bindingChanged.call(bindingSelect[0]);
                            valueAutoComplete.refresh();
                        }, 0);
                        base.setError(model.errors, "parameter", null);
                    })
                    .on("change", "select.Value", function () {
                        base.setError(model.errors, "parameter", null);
                    });

            if (model.parameters)
                for (var parameterIndex in model.parameters) {
                    var parameter = model.parameters[parameterIndex];
                    addParameter(parameter.name, parameter.expressionCode, parameter.elementCode, parameter.value);
                }

            if (!readOnly) addParameter();
        }
    };

    this.setValue = function (node) {
        node.text(!model.parameters || !model.parameters.length ? "" : model.parameters.length == 1 ? "1 parameter" : model.parameters.length + " parameters");
    };

    this.setError = function (handler) {
        handler(model.errors, ["parameter"]);
    };

    this.hasChanges = function () {
        return model.parameters && model.parameters.length;
    };

    this.destroy = function () {
        if (!readOnly) updateModel();
    };
}