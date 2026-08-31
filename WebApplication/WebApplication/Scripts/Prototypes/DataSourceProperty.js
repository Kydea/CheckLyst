prototypes.DataSourceProperty = function (model, property) {
    var
        base,
        sourceName,
        editable,

        change = function () {
            model[property] = sourceName.val();
            console.log(model[property]);
            base.updateModel(model);
        },

        refresh = function () {
            editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft;
        };


    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        refresh();
        valuePanel.addClass("DataSource");
        if (editable) {
            valuePanel.append(sourceName = $("<select />").attr("data-nullable", "").on("change", change));
            for (var index in prototypes.prototype.getModel().externalSources)
                $("<option />").appendTo(sourceName).text(index).attr("value", index);

            sourceName.val(model[property] == null ? null : model[property]);
            if (sourceName.val() != model[property])
                sourceName.attr("data-error", model[property]);
        } else
            valuePanel.append($("<input />").attr({ type: "text", readOnly: true }).val(model[property]));
    }

    this.hasChanges = function () {
        refresh();
        return !!model[property];
    }

    this.setError = function (handler) {
        handler(model.errors, "value");
    };

    this.setValue = function (node) {
        node.text(model[property]).attr("title", model[property]);
    }
}
