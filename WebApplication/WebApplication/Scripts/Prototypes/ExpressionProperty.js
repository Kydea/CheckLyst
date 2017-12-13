prototypes.ExpressionProperty = function (model, property, options) {
    var
        input,
        check,
        expressions,

        change = function () {
            var
                value = input.val(),
                error;

            if (input.prop("selectedIndex") > 2 && expressions[value].Error)
                error = "the expression is not valid";
            else if (check)
                for (var index in check)
                    if (!check[index].action.call(this, expressions[value])) {
                        error = check[index].message;
                        break;
                    }

            if (error) {
                new matematici.dialog.error("error", error);
                input.val("");
                return;
            }

            model[property] = value;
            matematici.canClose = false;
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel) {
        valuePanel.addClass("Expression");
        if (options.readOnly)
            $("<input />").attr({ type: "text", readOnly: true }).val(model[property]).appendTo(valuePanel);
        else {
            input = $("<select />");
            expressions = prototypes.prototype.getModel().expressions;
            input.append($("<option />").attr("value", "").text("(not set)"));
            for (var index in expressions)
                input.append($("<option />").attr({ value: index, selected: index == model[property] }).text(index));
            input.on("change", change);
            valuePanel.append(input);
        }
    }

    this.hasChanges = function () {
        return !!model[property];
    }
}

