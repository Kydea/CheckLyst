prototypes.ExpressionPropertyBase = function (model, property, valuePanel, base, errorCode) {
    var
        expressionPropertyBase = this,
        expressions = prototypes.prototype.getModel().expressions,
        expressionsKeys = Object.keys(expressions),
        expressionsValues = Object.values(expressions),
        expressionCode,
        viewButton,
        addButton,
        currentProperty,
        select,
        editor,

        hidePropertyPanel = function () {
            currentProperty = prototypes.propertyPanel.getCurrentProperty();
            prototypes.propertyPanel.confirm();
            matematici.popup.close();
        },

        resumePropertyPanel = function () {
            matematici.popup.close();
            prototypes.propertyPanel.select();
            prototypes.propertyPanel.setCurrentProperty(currentProperty);
            select = currentProperty.getType().getExpressionsSelect();
        },

        clickHandler= function (e) {
            e.preventDefault();
            hidePropertyPanel();

            new prototypes.ExpressionsList({
                closeHandler: resumePropertyPanel,
                useHandler: function (expression) {
                    currentProperty.getType().setExpressionCode(expression.code);
                    expressionPropertyBase.check(expression.code);
                }
            });
        },

        setup = function () {
            var buttonsPanel = $("<div />").addClass("ButtonsPanel").appendTo(valuePanel);
            buttonsPanel.append(viewButton = $("<a />").addClass("Icon Button").attr({ href: "#" }).prepend($("<i />").addClass("Icon fal fa-calculator")).on("click", clickHandler));
        };

    this.fill = function (value) {
        var found = false;
        for (var index in expressions) {
            select.append($("<option />").attr({ value: index, selected: index == value }).text(index));
            if (index == value) found = true;
        }
        return found;
    };

    this.check = function (value) {
        var error = null;
        if (expressions[value] == undefined)
            error = "expression not found";
        else if (expressions[value].error)
            error = "the expression is not valid";
        else if (!prototypes.propertyPanel.checkDataType(expressions[value], "Boolean"))
            error = "the expression result must be a boolean";

        if (error) {
            var autoComplete = matematici.AutoComplete.get(select[0]);
            if (autoComplete)
                autoComplete.setError(value);
            else
                select.attr("data-error", value)
        }

        base.setError(model.errors, errorCode, error);

        return !error;
    };

    this.toggleButtons = function (visible, expression) {
        expressionCode = expression;
        viewButton.toggle(visible && !!expression);
        addButton.toggle(visible && !expression);
    };

    this.setSelect = function (value) {
        select = value;
    };

    setup();
}

