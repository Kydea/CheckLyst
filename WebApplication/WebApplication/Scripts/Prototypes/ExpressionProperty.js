prototypes.ExpressionProperty = function (model, property, errorCode, showRadioButtons, showDefault, defaultValue) {
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
            model[condition] =
                index == 0 && showDefault ? 0 :
                index == 1 && showDefault || index == 0 && !showDefault ? 1 :
                index == 2 && showDefault || index == 1 && !showDefault ? 2 :
                !showRadioButtons || insteadRadio.is(":checked") ? 3 :
                andRadio.is(":checked") ? 4 : 5;
            base.updateModel(model);
        },

        inputChanged = function () {
            var value = input.val();

            var index = input.prop("selectedIndex");

            if (showDefault && index > 2 || !showDefault && index > 1)
                expressionPropertyBase.check(value);
            else
                base.setError(model.errors, errorCode, null);

            insteadRadio.parent().toggle(showRadioButtons && index > 2);
            andRadio.parent().toggle(showRadioButtons && index > 2);
            orRadio.parent().toggle(showRadioButtons && index > 2);
            model[expressionCode] = showDefault && index > 2 || !showDefault && index > 1 ? value : null;
            if (!insteadRadio.is(":checked") && !andRadio.is(":checked") && !orRadio.is(":checked"))
                insteadRadio.prop("checked", true);
            radioClicked();
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Expression");
        expressionPropertyBase = new prototypes.ExpressionPropertyBase(model, expressionCode, valuePanel, base, errorCode);

        if (editable) {
            input = $("<select />").appendTo(valuePanel);

            if (showDefault)
                input.append($("<option />").attr("value", "0").text("(default)").attr("selected", model[condition] == 0));

            input
                .append($("<option />").attr("value", "1").text("(true)").attr("selected", model[condition] == 1))
                .append($("<option />").attr("value", "2").text("(false)").attr("selected", model[condition] == 2));

            expressionPropertyBase.setSelect(input);

            expressionPropertyBase.fill(model[condition] <= 2 ? null : model[expressionCode]);
            if (model[condition] > 2) expressionPropertyBase.check(model[expressionCode]);

            input.on("change", inputChanged);
        } else {
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    showDefault && model[condition] == 0 ? "(default)" :
                    model[condition] == 1 ? "(true)" :
                    model[condition] == 2 ? "(false)" :
                    model[expressionCode]
                )
                .appendTo(valuePanel);

            //for css consistency
            $("<div />").appendTo(valuePanel);
            $("<div />").appendTo(valuePanel);
        }
        $("<label />").text("instead of default").prepend(insteadRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "instead" }).attr('disabled', !editable).prop("checked", model[condition] == 3)).appendTo(valuePanel).toggle(showRadioButtons && model[condition] > 2);
        $("<label />").text("and default").prepend(andRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "and" }).attr('disabled', !editable).prop("checked", model[condition] == 4)).appendTo(valuePanel).toggle(showRadioButtons && model[condition] > 2);
        $("<label />").text("or default").prepend(orRadio = $("<input />").on("click", radioClicked).attr({ type: "radio", name: "ReadOnly", value: "or" }).attr('disabled', !editable).prop("checked", model[condition] == 5)).appendTo(valuePanel).toggle(showRadioButtons && model[condition] > 2);
    }

    this.hasChanges = function () {
        return model[condition] && model[condition] !== defaultValue ;
    };

    this.getExpressionsSelect = function () {
        return input;
    };

    this.setExpressionCode = function (value) {
        model[condition] = 3;
        model[expressionCode] = value;
    };

    this.setError = function (handler) {
        handler(model.errors, errorCode);
    };

    this.setReadOnly = function (value) {
        editable = value;
    };
}

