prototypes.ExpressionProperty = function (model, property) {
    var
        base,
        insteadRadio, orRadio, andRadio,
        input,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,
        condition = property + "Condition",
        expressionCode = property + "ExpressionCode",
        expressionPropertyBase,

        radioClicked = function () {
            if (!editable) return;
            var index = input.prop("selectedIndex");
            model[condition] = index <= 2 ? index : insteadRadio.is(":checked") ? 3 : andRadio.is(":checked") ? 4 : 5;
            base.updateModel(model);
        },

        inputChanged = function () {
            var value = input.val();

            if (input.prop("selectedIndex") > 2)
                expressionPropertyBase.check(value);

            var index = input.prop("selectedIndex")
            insteadRadio.parent().toggle(index > 2);
            andRadio.parent().toggle(index > 2);
            orRadio.parent().toggle(index > 2);
            model[expressionCode] = index > 2 ? value : null;
            if (!insteadRadio.is(":checked") && !andRadio.is(":checked") && !orRadio.is(":checked"))
                insteadRadio.prop("checked", true);
            radioClicked();
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Expression");
        expressionPropertyBase = new prototypes.ExpressionPropertyBase(model, expressionCode);
        expressionPropertyBase.addExpressionButton(valuePanel);

        if (editable) {
            input = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "0").text("(default)").attr("selected", model[condition] == 0))
                .append($("<option />").attr("value", "1").text("(true)").attr("selected", model[condition] == 1))
                .append($("<option />").attr("value", "2").text("(false)").attr("selected", model[condition] == 2));

            expressionPropertyBase.setSelect(input);

            var found = model[condition] <= 2 | expressionPropertyBase.fill(found ? null : model[expressionCode]);
            if (!found) input.attr("data-error", model[expressionCode]);

            input.on("change", inputChanged);
        } else {
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model[condition] == 0 ? "(default)" :
                    model[condition] == 1 ? "(true)" :
                    model[condition] == 2 ? "(false)" :
                    model[expressionCode]
                )
                .appendTo(valuePanel);

            //for css consistency
            $("<div />").appendTo(valuePanel);
            $("<div />").appendTo(valuePanel);
        }
        $("<label />").text("instead of default").prepend(insteadRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "instead" }).attr('disabled', !editable).prop("checked", model[condition] == 3)).appendTo(valuePanel).toggle(model[condition] > 2);
        $("<label />").text("and default").prepend(andRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "and" }).attr('disabled', !editable).prop("checked", model[condition] == 4)).appendTo(valuePanel).toggle(model[condition] > 2);
        $("<label />").text("or default").prepend(orRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "or" }).attr('disabled', !editable).prop("checked", model[condition] == 5)).appendTo(valuePanel).toggle(model[condition] > 2);
    }

    this.hasChanges = function () {
        return model[condition] && model[condition] !== 0;
    }

    this.getExpressionsSelect = function () {
        return input;
    }
}

