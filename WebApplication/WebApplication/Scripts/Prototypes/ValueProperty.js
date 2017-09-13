prototypes.ValueProperty = function (model) {
    var
        sourceName,
        sourceColumn,
        input,
        select,
        editable,
        externalSource,
        formula,
        list,
        empty,

        valueChanged = function () {
            var index = select.prop("selectedIndex");
            model.ExternalSourceName = index == 1 ? sourceName.val() : null;
            model.ExternalSourceColumn = index == 1 ? sourceColumn.val() : null;
            model.ValueFormulaString = index == 2 ? input.val() : null;
            model.ValuesList = index == 3 ? input.val().split(",") : [];
            for (var valueIndex in model.ValuesList)
                if (model.ValuesList[valueIndex])
                    model.ValuesList[valueIndex] = model.ValuesList[valueIndex].trim();
            matematici.canClose = false;
        },

        sourceChanged = function (column) {
            sourceColumn.empty();
            var columns = matematici.sources[prototypes.prototype.getModel().ExternalSources[sourceName.val()]];
            for (index in columns)
                $("<option />").appendTo(sourceColumn).text(columns[index]).attr("value", columns[index]);
            if (column)
                sourceColumn.val(column);
            else
                sourceColumn[0].selectedIndex = 0;
            valueChanged();
        },

        selectChanged = function () {
            var index = select.prop("selectedIndex");
            input.toggle(index >= 2);
            sourceName.parent().toggle(index == 1);
            sourceColumn.parent().toggle(index == 1);
            if (index == 1) sourceChanged();
            valueChanged();
        },

        refresh = function () {
            editable = prototypes.prototype.getModel().State == prototypes.Prototype.states.draft;
            externalSource = model.ExternalSourceName && model.ExternalSourceColumn;
            formula = !!model.ValueFormulaString;
            list = model.ValuesList && model.ValuesList.length > 0;
            empty = !externalSource && !formula && !list;
        };

    this.create = function (valuePanel) {
        refresh();
        valuePanel.addClass("Value");

        if (editable) {
            select = $("<select />")
                .appendTo(valuePanel)
                .append($("<option />").text("empty").attr("selected", empty))
                .append($("<option />").text("external source").attr("selected", externalSource))
                .append($("<option />").text("formula").attr("selected", formula))
                .append($("<option />").text("comma separated values").attr("selected", list));

            select.on("change", selectChanged);

            $("<label />").text("Source").append(sourceName = $("<select />").on("change", sourceChanged)).appendTo(valuePanel);
            $("<label />").text("Column").append(sourceColumn = $("<select />").on("change", valueChanged)).appendTo(valuePanel);
            input = $("<textarea />").appendTo(valuePanel).attr({ type: "text" }).on("change", valueChanged).val(model.ValueFormulaString || (model.ValuesList && model.ValuesList.join()));

            for (var index in prototypes.prototype.getModel().ExternalSources)
                $("<option />").appendTo(sourceName).text(index).attr("value", index);

            if (externalSource) {
                sourceName.val(model.ExternalSourceName);
                sourceChanged(model.ExternalSourceColumn);
                input.hide();
            } else {
                sourceName.parent().hide();
                sourceColumn.parent().hide();
                if (empty)
                    input.hide();
            }
        } else {
            $("<input />")
                .attr({ type: "text", readOnly: true })
                .val(
                    empty ? "empty" :
                    externalSource ? "external source" :
                    formula ? "formula" :
                    "comma separated values"
                )
                .appendTo(valuePanel);

            if (externalSource) {
                $("<label />").text("Source").append($("<input />").attr({ type: "text", readOnly: true }).val(model.ExternalSourceName)).appendTo(valuePanel);
                $("<label />").text("Column").append($("<input />").attr({ type: "text", readOnly: true }).val(model.ExternalSourceColumn)).appendTo(valuePanel);
            } else if (!empty) 
                input = $("<textarea />").appendTo(valuePanel).attr({ type: "text" }).val(model.ValueFormulaString || (model.ValuesList && model.ValuesList.join()));
        }
    }

    this.hasChanges = function () {
        refresh();
        return !empty;
    }
}

