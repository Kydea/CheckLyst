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
                    cell = $("<th />").appendTo(row).text(item.Description);

                if (item.TypeName == "Group") {
                    cell.addClass("SubGroup");
                    var size = createTableHeader(rows, rowIndex + 1, item.Items);
                    rowsCount = Math.max(size.rowsCount + 1, rowsCount);
                    columnsCount += size.columnsCount;
                    if (size.columnsCount > 1)
                        cell.attr("colspan", size.columnsCount);
                }
                else
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
            console.log(setupModel);
            tableContent = !parent.isPage() && (parent.isTable() || parent.isTableContent());
            table = !tableContent && setupModel.ShowAsTable;
            subRow = !parent.isTable() && tableContent; 
            element = parent.isTable() ? $("<tr class='Pippo' />") : subRow ? parent.getElement() : $("<div />");

            if (table) {
                tableElement = $("<table />").addClass("Grid").appendTo(element).append($("<thead />")).append($("<tbody />"));
                createTableHeader([], 0, setupModel.Items);
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

                if (group.isIteration()) {
                    iterationCommands = $(tableContent ? "<td />" : "<div />").addClass("IterationCommands").appendTo(element)
                        .append($("<a />").attr("href", "#").addClass("Up").toggleClass("Hidden", parent.getSetupModel().DisableMove).append($("<i />").addClass("fa fa-arrow-up")).on("click", function () {
                            new instances.Event(instances.Events.up, id).trigger();
                        }))
                        .append($("<a />").attr("href", "#").addClass("Down").toggleClass("Hidden", parent.getSetupModel().DisableMove).append($("<i />").addClass("fa fa-arrow-down")).on("click", function () {
                            new instances.Event(instances.Events.down, id).trigger();
                        }))
                        .append($("<a />").attr("href", "#").addClass("Remove").toggleClass("Hidden", parent.getSetupModel().DisableRemove).append($("<i />").addClass("fa fa-close")).on("click", function () {
                            new instances.Event(instances.Events.remove, id).trigger();
                        }));
                }
                if (group.isGroup() && !group.isTableContent()) {
                    elementInformation = new instances.ElementInformation(element);
                    addButton = $("<div />").addClass("Item NewLine").toggleClass("Hidden", setupModel && setupModel.DisableAdd).appendTo(element)
                        .append($("<a />").attr("href", "#").addClass("Button AddIteration").toggleClass("Disabled", setupModel && setupModel.DisableAdd).text("Add").on("click", function () {
                            if (!readOnly) new instances.Event(instances.Events.add, id).trigger();
                        }));
                }
            }
        },

        update = function (model) {
            readOnly = !!model.ReadOnly;
            if (title) title.text(model.Description);
            if (addButton) addButton.toggle(model.Iterator && !model.DisableAdd);
            if (!group.isPage()) id = model.Id;
            element
                .toggleClass("Hidden", model.Hidden).toggleClass("ReadOnly", readOnly).toggleClass("Valid", model.Valid)
                .attr("data-code", model.Code).attr("data-id", model.Id);
            if (model.CountDownTo) new instances.CountDown(element, model.CountDownTo, model.Id);
            group.setColor(model.Color);
            group.setFontSize(model.FontSize);

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
            element.insertAfter(previous ? previous.getElement() : $("> h3", parent.getElement()));
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

        /* instanzia i nuovi oggetti, aggiorna ricorsivamente Groups per le pagine, Items per i gruppi e Iterations per gli iteratori */
        var children = model.Groups ? model.Groups : model.Iterator ? model.Iterations : model.Items;
        for (index in children) {
            var
                item,
                itemModel = children[index];

            if (!itemIndexes[itemModel.Id]) 
                item = itemModel.TypeName == "Group" || itemModel.TypeName == "Iteration" ? new instances.Group(null, itemModel.TypeName, this, index == 0 ? null : newItems[index - 1], itemModel) : new instances.Item(itemModel, this);
            else
                item = items[itemIndexes[itemModel.Id]];

            item.update(itemModel);

            newItemIndexes[itemModel.Id] = index;
            newItems.push(item);
            if (itemModel.TypeName == "Iteration")
                iteratorOrder[itemModel.Index] = index;
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

    this.getParent = function () {
        return parent;
    };

    setup();
}