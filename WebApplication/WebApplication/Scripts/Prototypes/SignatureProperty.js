prototypes.SignatureProperty = function (model, options) {
    var
        base,
        input,
        sourceName,

        change = function () {
            base.updateModel(model);
        },

        inputChanged = function () {
            model.signatureCode = input.val();
            if (sourceName) {
                sourceName.parent().toggle(!!model.signatureCode);
                if (model.signatureCode)
                    setTimeout(function () { matematici.AutoComplete.get(sourceName[0]).refresh(); }, 0);
                else {
                    model.dumpExternalSourceName = null;
                    externalSourceChanged();
                }
            }
            base.setError(model.errors, "signature", null);
            change();
        },

        externalSourceChanged = function () {
            model.dumpExternalSourceName = !model.signatureCode || !sourceName ? null : sourceName.val();
            base.setError(model.errors, "export-source", null);
            change();
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
            if (model.iterator && model.dumpExternalSourceName)
                $("<label />").addClass("DumpExternalDataSource").append($("<span />").text("Export source"))
                    .append($("<input />").attr({ type: "text", readOnly: true }).val(model.dumpExternalSourceName)).appendTo(valuePanel);
        } else {
            input = $("<select />").attr("data-nullable", "").appendTo(valuePanel);
            var found = false;
            for (var signatureCode in prototypes.prototype.getElements(["Signature"])) {
                input.append($("<option />").attr({ value: signatureCode, selected: signatureCode == model.signatureCode }).text(signatureCode));
                if (signatureCode == model.signatureCode) found = true;
            }

            if (!found) input.attr("data-error", model.signatureCode);
            if (!model.signatureCode) input.val(null);
            input.on("change", inputChanged);

            if (model.iterator && model.code) {
                $("<label />").addClass("DumpExternalDataSource").append($("<span />").text("Export source")).append(sourceName = $("<select />").attr("data-nullable", "")).appendTo(valuePanel);

                found = false;
                for (var dumpExternalSourceName in prototypes.prototype.getModel().externalSources) {
                    sourceName.append($("<option />").attr({ value: dumpExternalSourceName, selected: dumpExternalSourceName == model.dumpExternalSourceName }).text(dumpExternalSourceName));
                    if (dumpExternalSourceName == model.dumpExternalSourceName) found = true;
                }

                if (!found) sourceName.attr("data-error", model.dumpExternalSourceName);
                if (!model.dumpExternalSourceName) sourceName.val(null);
                sourceName.on("change", externalSourceChanged)

                sourceName.parent().toggle(!!model.signatureCode);
            }
        }
    }

    this.hasChanges = function () {
        return !!model.signatureCode;
    }

    this.setValue = function (node) {
        node.text(model.dumpExternalSourceName ? "..." : model.signatureCode);
    }

    this.setError = function (handler) {
        handler(model.errors, ["signature", "export-source"]);
    };
}

