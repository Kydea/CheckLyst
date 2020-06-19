prototypes.SignatureProperty = function (model, options) {
    var
        input,

        change = function () {
            model.signatureCode = input.val();
            matematici.canClose = false;
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel) {
        valuePanel.addClass("Signature");
        if (options.readOnly)
            $("<input />").attr({ type: "text", readOnly: true }).val(model.signatureCode).appendTo(valuePanel);
        else {
            input = $("<select />").attr("data-nullable", "");
            var found = false;
            for (var signatureCode in prototypes.prototype.getModel().signatures) {
                input.append($("<option />").attr({ value: signatureCode, selected: signatureCode == model.signatureCode }).text(signatureCode));
                if (signatureCode == model.signatureCode) found = true;
            }

            if (!found) 
                input.attr("data-error", model.signatureCode);

            input.on("change", change);
			valuePanel.append(input);
			if (!model.signatureCode) input.val(null);
        }
    }

    this.hasChanges = function () {
        return !!model.signatureCode;
    }
}

