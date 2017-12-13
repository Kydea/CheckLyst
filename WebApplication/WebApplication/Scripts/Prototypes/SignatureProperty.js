prototypes.SignatureProperty = function (model, options) {
    var
        input,
        setButton,
        cancelButton,

        change = function () {
            model.signatureCode = input.val();
            matematici.canClose = false;
        },

        setSignature = function () {
            if (input.closest(matematici.elements.body).size() == 0) {
                cancel();
                return true;
            }

            var item = $(this);
            if (!item.hasClass("Item") || !item.hasClass("Signature"))
                matematici.dialog.error("error", "Select a signature.");
            else if (!item.data("code"))
                matematici.dialog.error("error", "The signature must have a code.");
            else
                cancel();
            input.val($(this).data("code"));
            change();
            return false;
        },

        cancel = function () {
            prototypes.prototype.setItemHandler();
            setButton.show();
            cancelButton.hide();
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel) {
        valuePanel.addClass("Expression");
        if (options.readOnly)
            $("<input />").attr({ type: "text", readOnly: true }).val(model.signatureCode).appendTo(valuePanel);
        else {
            input = $("<select />").data("nullable", true);
            for (var signatureCode in prototypes.prototype.getModel().Signatures)
                input.append($("<option />").attr({ value: signatureCode, selected: signatureCode == model.signatureCode }).text(signatureCode));
            input.on("change", change);
			valuePanel.append(input);
			if (!model.signatureCode) input.val(null);

			setButton = $("<a />").addClass("Button").text("set by click").on("click", function (e) {
				setButton.hide();
				cancelButton.show();
				prototypes.prototype.setItemHandler(setSignature);
			}); //.appendTo(valuePanel);

            cancelButton = $("<a />").addClass("Button").text("cancel").on("click", function () {
                cancel();
            }).hide().appendTo(valuePanel);
        }
    }

    this.hasChanges = function () {
        return !!model.signatureCode;
    }
}

