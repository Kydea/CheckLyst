prototypes.TargetProperty = function (model, filter) {
    var
        base,
        select,
        readOnly = prototypes.prototype.getModel().state != prototypes.Prototype.states.draft,

        fill = function () {
            var found = false;
            for (var code in prototypes.prototype.getElements([filter])) {
                select.append($("<option />").attr({ value: code }).text(code));
                if (code == model.target) found = true;
            }
            setTimeout(function () {
                var autoComplete = matematici.AutoComplete.get(select[0]);
                if (found)
                    autoComplete.setValue(model.target);
                else if (model.target) {
                    autoComplete.setError(model.target);
                    autoComplete.setValue(null);
                } else
                    autoComplete.setValue(null);
                autoComplete.refresh();
            }, 0);
        },

        change = function () {
            model.target = select.val();
            base.updateModel(model);
            base.setError(model.errors, "target", null);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Target");

        if (readOnly)
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(model.target)
                .appendTo(valuePanel);
        else {
            select = $("<select />")
                .attr("data-nullable", "(independent)")
                .on("change", change)
                .appendTo(valuePanel);

            fill();
        }
    };

    this.hasChanges = function () {
        return model.target;
    };

    this.setError = function(handler) {
        handler(model.errors, "target");
    };
}