prototypes.CountDownToProperty = function (model) {
    var
        conditionSelect, valueSelect, offsetInput,
        expressions,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,

        check = function () {
            var
                value = valueSelect.val(),
                condition = !!value ? conditionSelect.val() : 0,
                error;

            if (condition == 3) {
                if (!value)
                    error = "select an expression";
                else if (expressions[value].error)
                    error = "the expression is not valid";
                else if (!prototypes.propertyPanel.checkDataType(expressions[value], "DateTime"))
                    error = "the expression result must be a date";
            } else if (condition == 6) {
                if (!value)
                    error = "select a signature";
            }

            var offset = parseInt(offsetInput.val() || 0);
            offsetInput.val(offset)

            if (error)
                new matematici.dialog.error("error", error);
            else {
                model.countDownCondition = condition;
                model.countDownToCode = condition > 0 ? value : null;
                model.countDownOffset = condition > 0 ? offset : 0;
            }

            matematici.canClose = false;
        },

        toggleControls = function () {
            var condition = conditionSelect.val();

            valueSelect.empty();
            if (condition == 3)
                for (var expressionCode in expressions)
                    valueSelect.append($("<option />").attr({ value: expressionCode }).text(expressionCode));
            else if (condition == 6)
                for (var signatureCode in prototypes.prototype.getModel().signatures)
                    valueSelect.append($("<option />").attr({ value: signatureCode }).text(signatureCode));

            setTimeout(function () {
                var autoComplete = valueSelect.data("autoComplete");
                valueSelect.toggleClass("ReadOnly", condition == 0);
                autoComplete.setValue(model.countDownToCode);
                autoComplete.refresh();
                autoComplete.checkReadOnly();

                matematici.controls.setEditable(offsetInput, condition > 0);
                offsetInput.val(model.countDownOffset)
            }, 0);
        },

        conditionChanged = function () {
            model.countDownCondition = 0;
            model.countDownToCode = null;
            model.countDownOffset = 0;

            toggleControls();
        },

        valueChanged = function () {
            check();
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("CountDown");
        expressions = prototypes.prototype.getModel().expressions;

        if (editable) {
            conditionSelect = $("<select />")
                .on("change", conditionChanged)
                .appendTo(valuePanel)
                .append($("<option />").attr("value", "0").text("(not set)").attr("selected", model.countDownCondition == 0 && !model.countDownToCode))
                .append($("<option />").attr("value", "3").text("expression").attr("selected", model.countDownCondition != 6 && !!model.countDownToCode))
                .append($("<option />").attr("value", "6").text("signature").attr("selected", model.countDownCondition == 6));

            valueSelect = $("<select />")
                .on("change", valueChanged)
                .data("nullable", "-")
                .appendTo(valuePanel);

            $("<label />")
                .text("offset (seconds)")
                .appendTo(valuePanel);

            offsetInput = $("<input />")
                .on("change", valueChanged)
                .keyfilter(/[\d]/)
                .attr({ type: "text" })
                .val(model.countDownOffset || "0")
                .appendTo(valuePanel);

            toggleControls();
        } else {
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    model.countDownCondition == 0 && !model.countDownToCode ? "(not set)" :
                    model.countDownCondition != 6 && !!model.countDownToCode ? "expression" :
                    "signature"
                )
                .appendTo(valuePanel);

            if (!(model.countDownCondition == 0 && !model.countDownToCode))
                $("<input />")
                    .attr({ type: "text", readOnly: true })
                    .val(model.countDownToCode)
                    .appendTo(valuePanel);

            $("<label />")
                .text("offset (seconds)")
                .appendTo(valuePanel);

            $("<input />")
                .attr({ type: "text", readOnly: true  })
                .val(model.countDownOffset || "0")
                .appendTo(valuePanel);
        }
    }

    this.hasChanges = function () {
        return !!model.countDownToCode;
    }
}

