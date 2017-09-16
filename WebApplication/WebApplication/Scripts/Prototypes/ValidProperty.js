prototypes.ValidProperty = function (model) {
    var
        insteadRadio, orRadio, andRadio,
        input,
        expressions,
        editable = prototypes.prototype.getModel().State == prototypes.Prototype.states.draft,

        radioClicked = function () {
            if (!editable) return;
            var index = input.prop("selectedIndex");
            model.ValidCondition = index <= 2 ? index : insteadRadio.is(":checked") ? 3 : andRadio.is(":checked") ? 4 : 5;
            matematici.canClose = false;
        },

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
            insteadRadio.parent().toggle(index > 2);
            andRadio.parent().toggle(index > 2);
            orRadio.parent().toggle(index > 2);
            model.ValidExpressionCode = index > 2 ? value : null;
            if (!insteadRadio.is(":checked") && !andRadio.is(":checked") && !orRadio.is(":checked"))
                insteadRadio.prop("checked", true);
            radioClicked();
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("Valid");
        expressions = prototypes.prototype.getModel().Expressions;
        if (editable)
        {
            input = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "").text("(default)").attr("selected", model.ValidCondition == 0))
                .append($("<option />").attr("value", "").text("(true)").attr("selected", model.ValidCondition == 1))
                .append($("<option />").attr("value", "").text("(false)").attr("selected", model.ValidCondition == 2));

            for (var index in expressions)
                input.append($("<option />").attr({ value: index, selected: model.ValidCondition > 2 && index == model.ValidExpressionCode }).text(index));

            input.on("change", inputChanged);
        } else
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model.ValidCondition == 0 ? "(default)" :
                    model.ValidCondition == 1 ? "(true)" :
                    model.ValidCondition == 2 ? "(false)" :
                    model.ValidExpressionCode
                )
                .appendTo(valuePanel);

        $("<label />").text("instead of default").prepend(insteadRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "instead" }).attr('disabled', !editable).prop("checked", model.ValidCondition == 3)).appendTo(valuePanel).toggle(model.ValidCondition > 2);
        $("<label />").text("and default").prepend(andRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "and" }).attr('disabled', !editable).prop("checked", model.ValidCondition == 4)).appendTo(valuePanel).toggle(model.ValidCondition > 2);
        $("<label />").text("or default").prepend(orRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "or" }).attr('disabled', !editable).prop("checked", model.ValidCondition == 5)).appendTo(valuePanel).toggle(model.ValidCondition > 2);
    }

    this.hasChanges = function () {
        return model.ValidCondition && model.ValidCondition !== 0;
    }
}

