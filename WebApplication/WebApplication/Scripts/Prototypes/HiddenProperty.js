prototypes.HiddenProperty = function (model) {
    var
        base,
        input,
        expressions,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,

        inputChanged = function () {
            var
                value = input.val(),
                error;

            if (input.prop("selectedIndex") > 1 && expressions[value].error)
                error = "the expression is not valid";
            else if (input.prop("selectedIndex") > 1 && !prototypes.propertyPanel.checkDataType(expressions[value], "Boolean"))
                error = "the expression result must be a boolean";

            if (error) {
                new matematici.dialog.error("error", error);
                input.prop("selectedIndex", 0);
                matematici.AutoComplete.get(input[0]).setError(value);
            }

            var index = input.prop("selectedIndex");
            model.hiddenExpressionCode = index > 1 ? value : null;
            model.hiddenCondition = index == 0 ? 2 : index == 1 ? 1 : 3;
            base.updateModel(model);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("HiddenProperty");
		expressions = prototypes.prototype.getModel().expressions;

        if (editable)
        {
            input = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "2").text("(false)").attr("selected", model.hiddenCondition == 2))
                .append($("<option />").attr("value", "1").text("(true)").attr("selected", model.hiddenCondition == 1));

            var found = model.hiddenCondition == 1 || model.hiddenCondition == 2;
            for (var index in expressions) {
                input.append($("<option />").attr({ value: index, selected: model.hiddenCondition > 2 && index == model.hiddenExpressionCode }).text(index));
                if (model.hiddenCondition > 2 && index == model.hiddenExpressionCode) found = true;
            }

            if (!found) 
                input.attr("data-error", model.hiddenExpressionCode);

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
}

