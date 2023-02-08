prototypes.CountDownToProperty = function (model) {
    var
        base,
        conditionSelect, valueSelect, offsetInput,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,
        expressionPropertyBase,

        check = function (value, ignoreNull) {
            var
                condition = conditionSelect.val(),
                error = null,
                expressions = expressionPropertyBase.getExpressions();

            if (condition == 3) {
                if (!value)
                    error = "select an expression";
                else if (expressions[value] == undefined)
                    error = "expression not found";
                else if (expressions[value].error)
                    error = "the expression is not valid";
                else if (!prototypes.propertyPanel.checkDataType(expressions[value], "DateTime"))
                    error = "the expression result must be a date/time";
            } else if (condition == 6) {
                if (!value)
                    error = "select a signature";
                else if (prototypes.prototype.getElement(value, ["Signature"]) == null)
                    error = "signature not found";
            } else
                base.setError(model.errors, "countdown", null);

            var offset = parseInt(offsetInput.val() || 0);
            offsetInput.val(offset)

            if (error) {
                var autoComplete = matematici.AutoComplete.get(valueSelect[0]);
                if (autoComplete)
                    autoComplete.setError(value);
                else
                    valueSelect.attr("data-error", value);
            }

            model.countDownCondition = condition;
            model.countDownToCode = condition > 0 ? value : null;
            model.countDownOffset = condition > 0 ? offset : 0;

            base.setError(model.errors, "countdown", error);
            base.updateModel(model);
        },

        toggleControls = function (checkValues) {
            var
                condition = conditionSelect.val(),
                expressions = expressionPropertyBase.getExpressions();

            valueSelect.empty();
            if (condition == 3)
                for (var expressionCode in expressions)
                    valueSelect.append($("<option />").attr({ value: expressionCode }).text(expressionCode));
            else if (condition == 6)
                for (var signatureCode in prototypes.prototype.getElements(["Signature"]))
                    valueSelect.append($("<option />").attr({ value: signatureCode }).text(signatureCode));

            setTimeout(function () {
                var autoComplete = matematici.AutoComplete.get(valueSelect[0]);
                valueSelect.toggleClass("ReadOnly", condition == 0);
                autoComplete.setValue(model.countDownToCode);
                autoComplete.refresh();
                autoComplete.checkReadOnly();

                matematici.controls.setEditable(offsetInput, condition > 0);
                offsetInput.val(model.countDownOffset)

                if (checkValues && condition > 0) check(model.countDownToCode);
            }, 0);
        },

        conditionChanged = function () {
            model.countDownCondition = 0;
            model.countDownToCode = null;
            model.countDownOffset = 0;

            toggleControls(false);

            base.setError();
        },

        valueChanged = function () {
            check(valueSelect.val());
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;

        valuePanel.addClass("CountDown");
        expressionPropertyBase = new prototypes.ExpressionPropertyBase(model, "countDownToCode", valuePanel, base, "countdown", true);

        if (editable) {
            conditionSelect = $("<select />")
                .on("change", conditionChanged)
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "0").text("(not set)").attr("selected", model.countDownCondition == 0 && !model.countDownToCode))
                .append($("<option />").attr("value", "3").text("expression").attr("selected", model.countDownCondition != 6 && !!model.countDownToCode))
                .append($("<option />").attr("value", "6").text("signature").attr("selected", model.countDownCondition == 6));

            valueSelect = $("<select />")
                .on("change", valueChanged)
                .attr("data-nullable", "-")
                .appendTo(valuePanel);

            expressionPropertyBase.setSelect(valueSelect);

            $("<label />")
                .text("offset (seconds)")
                .appendTo(valuePanel);

            offsetInput = $("<input />")
                .on("change", valueChanged)
                .keyfilter(/[\d]/)
                .attr({ type: "text" })
                .val(model.countDownOffset || "0")
                .appendTo(valuePanel);

            toggleControls(true);
        } else {
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model.countDownCondition == 0 && !model.countDownToCode ? "(not set)" :
                        model.countDownCondition != 6 && !!model.countDownToCode ? "expression" :
                            "signature"
                )
                .appendTo(valuePanel);

            if (!(model.countDownCondition == 0 && !model.countDownToCode))
                $("<input />")
                    .attr({ type: "text", readOnly: true })
                    .val(model.countDownToCode)
                    .appendTo(valuePanel);

            $("<label />")
                .text("offset (seconds)")
                .appendTo(valuePanel);

            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(model.countDownOffset || "0")
                .appendTo(valuePanel);
        }
    };

    this.hasChanges = function () {
        return !!model.countDownToCode;
    };

    this.getExpressionsSelect = function () {
        return valueSelect;
    };

    this.setExpressionCode = function (value) {
        model.countDownCondition = 3;
        model.countDownToCode = value;
    };

    this.setError = function (handler) {
        handler(model.errors, "countdown");
    };
};

