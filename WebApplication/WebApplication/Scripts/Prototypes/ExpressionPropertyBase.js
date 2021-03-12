prototypes.ExpressionPropertyBase = function (model, property, valuePanel) {
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

        verify = function (handler) {
            prototypes.prototype.verify(function (error) {
                editor.setError();
                if (handler) handler(error);
            });
        },

        hidePropertyPanel = function () {
            currentProperty = prototypes.propertyPanel.getCurrentProperty();
            matematici.popup.close();
        },

        resumePropertyPanel = function () {
            matematici.popup.close();
            prototypes.propertyPanel.select();
            prototypes.propertyPanel.setCurrentProperty(currentProperty);
            select = currentProperty.getType().getExpressionsSelect();
        },

        changeExpression = function (row) {
            matematici.canClose = false;
            expressions[row.code] = row;
            verify(function () {
                resumePropertyPanel();
                if (expressionPropertyBase.check(row.code))
                    model[property] = row.code;
            });
        },

        viewExpression = function (e) {
            e.preventDefault();
            hidePropertyPanel();
            verifyHandler = function (editor, row) {
                var oldValue = expressions[row.code];
                expressions[row.code] = row;
                verify(function () {
                    expressions[row.code] = oldValue;
                });
            };
            editor = new prototypes.ExpressionList(expressionsValues, expressionsKeys.indexOf(expressionCode), changeExpression, null, verifyHandler, resumePropertyPanel);
        },

        setup = function () {
            var buttonsPanel = $("<div />").addClass("ButtonsPanel").appendTo(valuePanel);
            buttonsPanel.append(viewButton = $("<a />").addClass("Icon Button").attr({ href: "#" }).text("Expression editor").prepend($("<i />").addClass("Icon fal fa-calculator")).on("click", viewExpression));
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
        var error;
        if (expressions[value].error)
            error = "the expression is not valid";
        else if (!prototypes.propertyPanel.checkDataType(expressions[value], "Boolean"))
            error = "the expression result must be a boolean";

        if (error) {
            new matematici.dialog.error("error", error);
            select.prop("selectedIndex", 0);
            matematici.AutoComplete.get(select[0]).setError(value);
        }

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

