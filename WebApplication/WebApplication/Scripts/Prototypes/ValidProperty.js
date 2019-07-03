prototypes.ValidProperty = function (model) {
    var
        insteadRadio, orRadio, andRadio,
        input,
        expressions,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,

        radioClicked = function () {
            if (!editable) return;
            var index = input.prop("selectedIndex");
            model.validCondition = index <= 2 ? index : insteadRadio.is(":checked") ? 3 : andRadio.is(":checked") ? 4 : 5;
            matematici.canClose = false;
        },

        inputChanged = function () {
            var
                value = input.val(),
                error;

            if (input.prop("selectedIndex") > 2 && expressions[value].error)
                error = "the expression is not valid";
            else if (input.prop("selectedIndex") > 2 && expressions[value].dataType.typeName != "Boolean" && !(expressions[value].innerDataType && expressions[value].innerDataType.typeName == "Boolean"))
                error = "the expression result must be a boolean";

            if (error) {
                new matematici.dialog.error("error", error);
                input.val("");
            }

            var index = input.prop("selectedIndex")
            insteadRadio.parent().toggle(index > 2);
            andRadio.parent().toggle(index > 2);
            orRadio.parent().toggle(index > 2);
            model.validExpressionCode = index > 2 ? value : null;
            if (!insteadRadio.is(":checked") && !andRadio.is(":checked") && !orRadio.is(":checked"))
                insteadRadio.prop("checked", true);
            radioClicked();
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("Valid");
        expressions = prototypes.prototype.getModel().expressions;
        if (editable) {
            input = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "0").text("(default)").attr("selected", model.validCondition == 0))
                .append($("<option />").attr("value", "1").text("(true)").attr("selected", model.validCondition == 1))
                .append($("<option />").attr("value", "2").text("(false)").attr("selected", model.validCondition == 2));

            for (var index in expressions)
                input.append($("<option />").attr({ value: index, selected: model.validCondition > 2 && index == model.validExpressionCode }).text(index));

            input.on("change", inputChanged);
        } else {
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model.validCondition == 0 ? "(default)" :
                        model.validCondition == 1 ? "(true)" :
                            model.validCondition == 2 ? "(false)" :
                                model.validExpressionCode
                )
                .appendTo(valuePanel);

            //for css consistency
            $("<div />").appendTo(valuePanel); 
            $("<div />").appendTo(valuePanel);
        }
        $("<label />").text("instead of default").prepend(insteadRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "instead" }).attr('disabled', !editable).prop("checked", model.validCondition == 3)).appendTo(valuePanel).toggle(model.validCondition > 2);
        $("<label />").text("and default").prepend(andRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "and" }).attr('disabled', !editable).prop("checked", model.validCondition == 4)).appendTo(valuePanel).toggle(model.validCondition > 2);
        $("<label />").text("or default").prepend(orRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "or" }).attr('disabled', !editable).prop("checked", model.validCondition == 5)).appendTo(valuePanel).toggle(model.validCondition > 2);
    }

    this.hasChanges = function () {
        return model.validCondition && model.validCondition !== 0;
    }
}

