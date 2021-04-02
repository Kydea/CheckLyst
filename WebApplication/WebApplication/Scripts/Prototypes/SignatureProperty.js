prototypes.SignatureProperty = function (model, options) {
    var
        base,
        input,
        sourceName,

        change = function () {
            model.signatureCode = input.val();
            if (sourceName) {
                sourceName.parent().toggle(!!model.signatureCode);
                setTimeout(function () { matematici.AutoComplete.get(sourceName[0]).refresh(); }, 0);
            }
            model.dumpExternalSourceName = !model.signatureCode || !sourceName ? null : sourceName.val();
            base.updateModel(model);
        };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Signature");
        if (options.readOnly) {
            $("<input />").attr({ type: "text", readOnly: true }).val(model.signatureCode).appendTo(valuePanel);
            if (model.iterator && !model.dumpExternalSourceName)
                $("<input />").attr({ type: "text", readOnly: true }).val(model.dumpExternalSourceName).appendTo(valuePanel);
        } else {
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

            if (model.iterator && model.code) {
                $("<label />").addClass("DumpExternalDataSource").append($("<span />").text("Source")).append(sourceName = $("<select />").attr("data-nullable", "").on("change", change)).appendTo(valuePanel);

                found = false;
                for (var dumpExternalSourceName in prototypes.prototype.getModel().externalSources) {
                    sourceName.append($("<option />").attr({ value, dumpExternalSourceName, selected: dumpExternalSourceName == model.dumpExternalSourceName }).text(dumpExternalSourceName));
                    if (dumpExternalSourceName == model.dumpExternalSourceName) found = true;
                }

                if (!found)
                    sourceName.attr("data-error", model.dumpExternalSourceName);

                sourceName.parent().toggle(!!model.signatureCode);
            }
        }
    }

    this.hasChanges = function () {
        return !!model.signatureCode;
    }

    this.setValue = function (node) {
        node.text(model.signatureCode);
    }
}

