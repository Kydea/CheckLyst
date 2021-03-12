prototypes.HiddenProperty = function (model) {
    var
        base,
        input,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,
        condition = "hiddenCondition",
        expressionCode = "hiddenExpressionCode",

        inputChanged = function () {
            var value = input.val();

            if (input.prop("selectedIndex") > 1)
                expressionPropertyBase.check(value);

            var index = input.prop("selectedIndex");
            model.hiddenExpressionCode = index > 1 ? value : null;
            model.hiddenCondition = index == 0 ? 2 : index == 1 ? 1 : 3;
            base.updateModel(model);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("HiddenProperty");
        expressionPropertyBase = new prototypes.ExpressionPropertyBase(model, expressionCode, valuePanel);

        if (editable) {
            input = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "2").text("(false)").attr("selected", model.hiddenCondition == 2))
                .append($("<option />").attr("value", "1").text("(true)").attr("selected", model.hiddenCondition == 1));

            expressionPropertyBase.setSelect(input);

            var found = model[condition] <= 2 | expressionPropertyBase.fill(found ? null : model[expressionCode]);
            if (!found) input.attr("data-error", model.hiddenExpressionCode);

            input.on("change", inputChanged);
        } else
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model.hiddenCondition == 2 ? "(false)" :
                        model.hiddenCondition == 1 ? "(true)" :
                            model.hiddenExpressionCode
                )
                .appendTo(valuePanel);
    }

    this.hasChanges = function () {
        return model.hiddenCondition && model.hiddenCondition !== 2;
    }

    this.getExpressionsSelect = function () {
        return input;
    }
}

