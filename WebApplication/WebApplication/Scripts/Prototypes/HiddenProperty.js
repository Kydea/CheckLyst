prototypes.HiddenProperty = function (model) {
    var
        input,
        expressions,
        editable = prototypes.prototype.getModel().State == prototypes.Prototype.states.draft,

        inputChanged = function () {
            var
                value = input.val(),
                error;

            if (input.prop("selectedIndex") > 2 && expressions[value].Error)
                error = "the expression is not valid";
            else if (input.prop("selectedIndex") > 2 && expressions[value].DataType.TypeName != "Boolean")
                error = "the expression must result must be a boolean";

            if (error) {
                new matematici.dialog.error("error", error);
                input.val("");
            }

            var index = input.prop("selectedIndex")
            model.HiddenExpressionCode = index > 1 ? value : null;
            model.HiddenCondition = index == 0 ? 2 : index == 1 ? 1 : 3;
            matematici.canClose = false;
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("HiddenProperty");
        expressions = prototypes.prototype.getModel().Expressions;
        if (editable)
        {
            input = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "").text("(false)").attr("selected", model.HiddenCondition == 2))
                .append($("<option />").attr("value", "").text("(true)").attr("selected", model.HiddenCondition == 1));

            for (var index in expressions)
                input.append($("<option />").attr({ value: index, selected: model.HiddenCondition > 2 && index == model.HiddenExpressionCode }).text(index));

            input.on("change", inputChanged);
        } else
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model.HiddenCondition == 2 ? "(false)" :
                    model.HiddenCondition == 1 ? "(true)" :
                    model.HiddenExpressionCode
                )
                .appendTo(valuePanel);
    }

    this.hasChanges = function () {
        return model.HiddenCondition && model.HiddenCondition !== 2;
    }
}

