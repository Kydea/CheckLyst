instances.Group = function (element, typeName, parent, previous, setupModel) {
    var
        group = this,
        items = [],
        itemIndexes = {}, //items[itemIndex[n]] is the item item with id == n
        title,
        iterationCommands,
        addButton,
        id,
        elementInformation,
        table = false,
        tableContent = false,
        readOnly,
        tableElement,

		createTableHeader = function (rows, rowIndex, items) {
            var
                rowsCount = 1,
                columnsCount = 0,
                row;

            if (rows.length < rowIndex + 1) {
                row = $("<tr />");
                if (rowIndex == 0)
                    row.appendTo(tableElement.children("thead"));
                else
                    row.insertAfter(rows[rowIndex - 1]);
                rows.push(row);
            } else
                row = rows[rowIndex];

            for (var index = 0; index < items.length; ++index) {
                var
                    item = items[index],
                    cell = $("<th />").appendTo(row).text(item.description);

                if (item.typeName == "Group") {
                    cell.addClass("SubGroup");
                    var size = createTableHeader(rows, rowIndex + 1, item.items);
                    rowsCount = Math.max(size.rowsCount + 1, rowsCount);
                    columnsCount += size.columnsCount;
                    if (size.columnsCount > 1)
                        cell.attr("colspan", size.columnsCount);
                } else
                    columnsCount++;
            }

            //commands column
            if (rowIndex == 0) {
                $("<th />").appendTo(row);
                columnsCount++;
            }

            if (rowsCount > 1)
                row.children(":not(.SubGroup)").attr("rowspan", rowsCount);
            return { rowsCount: rowsCount, columnsCount: columnsCount };
        },

        create = function () {
            tableContent = !parent.isPage() && (parent.isTable() || parent.isTableContent());
			table = !tableContent && setupModel.showAsTable;
            subRow = !parent.isTable() && tableContent; 
            element = parent.isTable() ? $("<tr />") : subRow ? parent.getElement() : $("<div />");

            if (table) {
                tableElement = $("<table />").addClass("Grid").appendTo(element).append($("<thead />")).append($("<tbody />"));
                createTableHeader([], 0, setupModel.items);
            }

            group.appendElement();
        },

        setup = function () {
            if (!element) create();

            element.addClass("Element " + typeName);

            if (!group.isPage()) {

                title = $("<h3 />");
                if (table)
                    title.insertBefore(tableElement);
                else if (!tableContent)
                    title.appendTo(element);

				if (group.isIteration()) 
                    iterationCommands = $(tableContent ? "<td />" : "<div />").addClass("IterationCommands").appendTo(element)
						.append($("<a />").attr("href", "#").addClass("Up").toggleClass("Hidden", !!parent.getSetupModel().disableMove).append($("<i />").addClass("fi flaticon-arrow-up")).on("click", function () {
                            new instances.Event(instances.Events.up, id).trigger();
                        }))
						.append($("<a />").attr("href", "#").addClass("Down").toggleClass("Hidden", !!parent.getSetupModel().disableMove).append($("<i />").addClass("fi flaticon-arrow-down")).on("click", function () {
                            new instances.Event(instances.Events.down, id).trigger();
                        }))
						.append($("<a />").attr("href", "#").addClass("Remove").toggleClass("Hidden", !!parent.getSetupModel().disableRemove).append($("<i />").addClass("fi flaticon-trash-o")).on("click", function () {
                            new instances.Event(instances.Events.remove, id).trigger();
                        }));
				if (group.isGroup() && !group.isTableContent()) {
                    elementInformation = new instances.ElementInformation(element);
					addButton = $("<div />").addClass("Item NewLine").toggleClass("Hidden", setupModel && setupModel.disableAdd).appendTo(element)
						.append($("<a />").attr("href", "#").addClass("Button AddIteration").toggleClass("Disabled", setupModel && setupModel.disableAdd).text("Add").prepend($("<i />").addClass("fi flaticon-plus")).on("click", function () {
                            if (!readOnly) new instances.Event(instances.Events.add, id).trigger();
                        }));
                }
            }
        },

        update = function (model) {
            readOnly = !!model.readOnly;
            if (title) title.text(model.description);
            if (addButton) addButton.toggle(model.iterator && !model.disableAdd);
            if (!group.isPage()) id = model.id;
            element
                .toggleClass("Hidden", model.hidden).toggleClass("ReadOnly", readOnly).toggleClass("Valid", model.valid)
                .attr("data-code", model.code).attr("data-id", model.id);
            if (model.countDownToValue) new instances.CountDown(element, model.countDownToValue, model.id);
            group.setColor(model.color);
            group.setFontSize(model.fontSize);

            if (addButton) addButton.children("a").toggleClass("ReadOnly", readOnly);
            if (iterationCommands) iterationCommands.toggle(!readOnly);
            if (elementInformation) elementInformation.update(model);
        };

    this.appendElement = function () {
        if (parent.isPage()) 
            element.appendTo(parent.getElement());
        else if (parent.isTable()) 
            element.appendTo($("tbody", parent.getElement()));
        else if (!subRow) 
			element.appendTo(parent.getElement());
    };

    this.setColor = function (value) {
        if (title)
            title.css({ color: value });
    };

    this.setFontSize = function (value) {
        if (title)
            title.css({ fontSize: value == 0 ? "" : value + "px" });
    };

    this.update = function (model) {
        var
            index,
            itemIndex = 0,
            modelIndex = 0,
            newItems = [],
            newItemIndexes = {},
            iteratorOrder = {};

        update(model);

        /* istanzia i nuovi oggetti, aggiorna ricorsivamente Groups per le pagine, Items per i gruppi e Iterations per gli iteratori */
        var children = model.groups ? model.groups : model.iterator ? model.iterations : model.items;
        for (index in children) {
            var
                item,
                itemModel = children[index];

            if (!itemIndexes[itemModel.id]) 
                item = itemModel.typeName == "Group" || itemModel.typeName == "Iteration" ? new instances.Group(null, itemModel.typeName, this, index == 0 ? null : newItems[index - 1], itemModel) : new instances.Item(itemModel, this);
            else
                item = items[itemIndexes[itemModel.id]];

            item.update(itemModel);

            newItemIndexes[itemModel.id] = index;
            newItems.push(item);
            if (itemModel.typeName == "Iteration")
                iteratorOrder[itemModel.index] = index;
        }

        /* elimina gli oggetti che non esistono più */
        for (index in itemIndexes)
            if (!newItemIndexes[index])
                items[itemIndexes[index]].delete();

        items = newItems;
        itemIndexes = newItemIndexes;

        /* riordina gli oggetti iteration */
        if (iteratorOrder)
            for (index in iteratorOrder)
                items[iteratorOrder[index]].appendElement();
                //element.append(items[iteratorOrder[index]].getElement());

        if (iterationCommands) iterationCommands.appendTo(element);

        element.append(addButton);

    };

    this.getSetupModel = function () {
        return setupModel;
    };

    this.clear = function () {
        element.empty();
        items = [];
        itemIndexes = {};
    };

    this.delete = function () {
        element.remove();
        delete this;
    };

    this.getElement = function () {
        return element;
    };

    this.isPage = function () {
        return typeName == "Page";
    };

    this.isGroup = function () {
        return typeName == "Group";
    };

    this.isIteration = function () {
        return typeName == "Iteration";
    };

    this.isTable = function () {
        return table;
    };

    this.isTableContent = function () {
        return tableContent;
    };

	this.isSubRow = function () {
		return subRow;
	};

    this.getParent = function () {
        return parent;
    };

    setup();
}