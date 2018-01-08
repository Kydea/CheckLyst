prototypes.ExpressionProperty = function (model, property, options) {
    var
        input,
        check,
        expressions,

        change = function () {
			//console.log(model[property], input.val());
            var
                value = input.val(),
                error;

            if (input.prop("selectedIndex") > 1 && expressions[value].Error)
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
			input = $("<select />").data("nullable", true);
            expressions = prototypes.prototype.getModel().expressions;
            for (var index in expressions)
				input.append($("<option />").attr({ value: index }).text(index));
			input.on("change", change);
			input.val(model[property]);
			valuePanel.append(input);
        }
    }

    this.hasChanges = function () {
        return !!model[property];
    }
}

