prototypes.ValueProperty = function (model) {
    var
        base,
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
			model.externalSourceName = index == 1 ? sourceName.val() : null;
			model.externalSourceColumn = index == 1 ? sourceColumn.val() : null;
			model.valueFormulaString = index == 2 ? input.val() : null;
			model.valuesList = index == 3 ? input.val().split(",") : [];
			for (var valueIndex in model.valuesList)
				if (model.valuesList[valueIndex])
					model.valuesList[valueIndex] = model.valuesList[valueIndex].trim();
            base.updateModel(model);
            base.setError(model.errors, "value", null);
		},

		sourceChanged = function (column) {
			sourceColumn.empty();
			if (!!sourceName.val() && !!prototypes.prototype.getModel().externalSources[sourceName.val()])
				$.ajax({
					type: "post",
					url: matematici.urls.dataSources.api + "/Data/GetDataSource",
					dataType: "json",
					contentType: "application/json; charset=utf-8",
					data: JSON.stringify({
						code: prototypes.prototype.getModel().externalSources[sourceName.val()]
					})
				}).done(function (result) {
					var columns = result.columns;
					for (index in columns)
                        $("<option />").appendTo(sourceColumn).text(columns[index]).attr("value", columns[index]);
                    matematici.AutoComplete.get(sourceName[0]).refresh();
                    matematici.AutoComplete.get(sourceColumn[0]).refresh();
					if (column)
						sourceColumn.val(column);
					else
						sourceColumn[0].selectedIndex = 0;
					valueChanged();
				});
			else
				setTimeout(function () {
                    matematici.AutoComplete.get(sourceName[0]).refresh();
                    matematici.AutoComplete.get(sourceColumn[0]).refresh();
                }, 0);

            base.setError(model.errors, "value", null);
		},

		selectChanged = function () {
			var index = select.prop("selectedIndex");
			input.toggle(index >= 2);
			sourceName.parent().toggle(index == 1);
			sourceColumn.parent().toggle(index == 1);
            if (index == 1) sourceChanged();
            base.setError(model.errors, "value", null);
			valueChanged();
		},

		refresh = function () {
			editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft;
			externalSource = model.externalSourceName && model.externalSourceColumn;
			formula = !!model.valueFormulaString;
			list = model.valuesList && model.valuesList.length > 0;
			empty = !externalSource && !formula && !list;
		};

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
		refresh();
		valuePanel.addClass("Value");

		if (editable) {
			select = $("<select />")
				.appendTo(valuePanel)
				.append($("<option />").text("empty").attr({
					value: 0,
					selected: empty
				}))
				.append($("<option />").text("external source").attr({
					value: 1,
					selected: externalSource
				}))
				.append($("<option />").text("formula").attr({
					value: 2,
					selected: formula
				}))
				.append($("<option />").text("comma separated values").attr({
					value: 3,
					selected: list
				}));

			select.on("change", selectChanged);

			$("<label />").append($("<span />").text("Source")).append(sourceName = $("<select />").on("change", sourceChanged)).appendTo(valuePanel);
			$("<label />").append($("<span />").text("Column")).append(sourceColumn = $("<select />").on("change", valueChanged)).appendTo(valuePanel);
			input = $("<textarea />").appendTo(valuePanel).attr({ type: "text" }).on("change", valueChanged).val(model.valueFormulaString || (model.valuesList && model.valuesList.join()));

			for (var index in prototypes.prototype.getModel().externalSources)
				$("<option />").appendTo(sourceName).text(index).attr("value", index);

			if (externalSource) {
                sourceName.val(model.externalSourceName);
                if (sourceName.val() != model.externalSourceName) {
                    sourceName.attr("data-error", model.externalSourceName);
                } else
				    sourceChanged(model.externalSourceColumn);
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
				$("<label />").text("Source").append($("<input />").attr({ type: "text", readOnly: true }).val(model.externalSourceName)).appendTo(valuePanel);
				$("<label />").text("Column").append($("<input />").attr({ type: "text", readOnly: true }).val(model.externalSourceColumn)).appendTo(valuePanel);
			} else if (!empty)
                input = $("<textarea />").appendTo(valuePanel).attr({ type: "text", readOnly: true }).val(model.valueFormulaString || (model.valuesList && model.valuesList.join()));
		}
	}

	this.hasChanges = function () {
		refresh();
		return !empty;
	}

    this.setError = function (handler) {
        handler(model.errors, "value");
    };
}

