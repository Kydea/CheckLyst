instances.Group = function (element, typeName, parent, setupModel) {
    var
        group = this,
        items = [],
        itemIndexes = {}, //items[itemIndex[n]] is the item item with id == n
        tableHeaderCells = {},
        title,
        iterationCommands,
        upButton,
        downButton,
        removeButton,
        actionButtonsBar,
        addButton,
        importButton,
        id,
        elementInformation,
        table = false,
        tableContent = false,
        readOnly,
        tableElement,
        tableBody,
        pageIndex,

        updateTableHeaderCell = function (cell, description) {
            cell.html(description);
            var errorSpans = cell[0].querySelectorAll("span.Error, span.Null");
            for (var index = 0; index < errorSpans.length; ++index) {
                var
                    errorSpan = errorSpans[index],
                    i = document.createElement("i");

                i.className = "fas fa-exclamation-triangle";
                i.setAttribute("title", errorSpan.textContent || "no value");
                errorSpan.innerHTML = "";
                errorSpan.appendChild(i);
            }
        },

        updateTableHeader = function (model) {
            for (var index in model.items) {
                itemModel = model.items[index];
                updateTableHeaderCell(tableHeaderCells[itemModel.id], itemModel.descriptionValue);
                if (itemModel.typeName == "Group")
                    updateTableHeader(itemModel);
            }
        },

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
                    cell = $("<th />").appendTo(row);

                updateTableHeaderCell(cell, item.descriptionValue);

                tableHeaderCells[item.id] = cell;

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

            for (var index = 0; index < columnsCount; ++index) {
                var
                    cell = row.children().eq(index),
                    cols = cell.attr("colspan") || 1;
            }

            if (rowIndex == 0) {
                var width = 100 / columnsCount;
                for (var index in tableHeaderCells) {
                    var
                        cell = tableHeaderCells[index],
                        cols = parseInt(cell.attr("colspan")) || 1;
                    cell.css("width", width * cols + "%");
                }

                //commands column
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
            element = parent.isTable() ? document.createElement("tr") : subRow ? parent.getElement() : document.createElement("div");

            if (table) {
                tableBody = document.createElement("tbody");
                tableElement = $("<table />").addClass("Grid").appendTo($("<div />").addClass("TableContainer").appendTo(element)).append($("<thead />")).append(tableBody);
                createTableHeader([], 0, setupModel.items);
            }

            group.appendElement();
        },

        setup = function () {
            if (!element) create();

            element.classList.add("Element");
            element.classList.add(typeName);

            if (!group.isPage()) {
                title = document.createElement("h3");
                if (table)
                    tableElement[0].parentNode.insertBefore(title, tableElement[0]);
                else if (!tableContent)
                    element.appendChild(title);

                if (group.isIteration()) {
                    iterationCommands = document.createElement(tableContent ? "td" : "div");
                    iterationCommands.className = "IterationCommands";

                    upButton = document.createElement("a");
                    upButton.className = "Up";
                    upButton.addEventListener("click", function () {
                        new instances.Event(instances.Events.up, id).trigger();
                    });
                    var i = document.createElement("i");
                    i.className = "fal fa-caret-up";
                    upButton.appendChild(i);
                    iterationCommands.appendChild(upButton);

                    downButton = document.createElement("a");
                    downButton.className = "Down";
                    downButton.addEventListener("click", function () {
                        new instances.Event(instances.Events.down, id).trigger();
                    });
                    var i = document.createElement("i");
                    i.className = "fal fa-caret-down";
                    downButton.appendChild(i);
                    iterationCommands.appendChild(downButton);

                    removeButton = document.createElement("a");
                    removeButton.className = "Remove";
                    removeButton.addEventListener("click", function () {
                        new instances.Event(instances.Events.remove, id).trigger();
                    });
                    var i = document.createElement("i");
                    i.className = "fal fa-trash-alt";
                    removeButton.appendChild(i);
                    iterationCommands.appendChild(removeButton);

                    element.appendChild(iterationCommands);
                }

				if (group.isGroup() && !group.isTableContent()) {
                    elementInformation = new instances.ElementInformation(element);
                    if (setupModel && setupModel.iterator) {
                        actionButtonsBar = document.createElement("div");
                        actionButtonsBar.className = "Item NewLine ActionButtonsBar";
                        element.appendChild(actionButtonsBar);

                        addButton = document.createElement("a");
                        addButton.setAttribute("href", "#");
                        addButton.className = "Button AddIteration";
                        var i = document.createElement("i");
                        i.className = "Icon fa fa-plus";
                        addButton.appendChild(i);
                        addButton.innerHTML += "Add"
                        addButton.addEventListener("click", function () {
                            if (!readOnly) new instances.Event(instances.Events.add, id).trigger();
                        });
                        actionButtonsBar.appendChild(addButton);

                        if (setupModel && setupModel.externalSources) {
                            importButton = document.createElement("a");
                            importButton.setAttribute("href", "#");
                            importButton.className = "Button Import";
                            var i = document.createElement("i");
                            i.className = "Icon fa fa-file-import";
                            importButton.appendChild(i);
                            importButton.innerHTML += "Import"
                            importButton.addEventListener("click", function () {
                                if (!readOnly) {
                                    var buttons = [{ text: "Cancel", icon: "times", cancelButton: true }];
                                    for (var externalSourceIndex in setupModel.externalSources.sort().reverse()) {
                                        buttons.push({
                                            text: setupModel.externalSources[externalSourceIndex],
                                            icon: "file-import",
                                            data: setupModel.externalSources[externalSourceIndex],
                                            handler: function (name) {
                                                new matematici.ExternalSourcesSearchForm(id, name, true);
                                            }
                                        });
                                    }
                                    matematici.dialog.popupMenu(buttons);
                                }
                            });
                            actionButtonsBar.appendChild(importButton);
                        }
                    }
                }
            }
        },

        update = function (model) {
            readOnly = !!model.readOnly;
			if (title) title.innerHTML = model.descriptionValue;
            if (!group.isPage()) {
                id = model.id;
                instances.instance.getItems()[id] = group;
            }
            element.classList.toggle("Hidden", model.hidden || !!model.removed && !instances.instance.areRemovedIterationsVisible());
            element.classList.toggle("Removed", !!model.removed);
            element.classList.toggle("ReadOnly", readOnly);
            element.classList.toggle("Valid", model.valid);
            element.setAttribute("data-code", model.code);
            element.setAttribute("data-id", model.id);
            if (model.iterator) {
                actionButtonsBar.classList.toggle("Hidden", !model.addIterationButton);
                element.classList.toggle("HideRemoveButton", !model.removeIterationButton);
                element.classList.toggle("HideMoveButton", !model.moveIterationButton);
                element.classList.toggle("HideMoveButton", !model.moveIterationButton);
            }

            if (model.countDownToValue) new instances.CountDown(element, model.countDownToValue, group.isPage() ? -group.getPageIndex() : model.id);
            group.setColor(model.color);
            group.setFontSize(model.fontSize);

            if (actionButtonsBar)
                for (var actionButtonIndex = 0; actionButtonIndex < actionButtonsBar.childNodes.length; ++actionButtonIndex)
                    actionButtonsBar.childNodes[actionButtonIndex].classList.toggle("ReadOnly", readOnly);
            if (iterationCommands) {
                if (tableContent)
                    iterationCommands.classList.toggle("Inactive", readOnly);
                else
                    iterationCommands.style.display = readOnly ? "none" : "";
            }
            if (elementInformation) elementInformation.update(model);
        };

    this.appendElement = function () {
        if (parent.isPage())
            parent.getElement().appendChild(element);
        else if (parent.isTable())
            parent.getTableBody().appendChild(element);
        else if (!subRow)
            parent.getElement().appendChild(element);
        element.classList.remove("FirstIteration");
        element.classList.remove("LastIteration");
    };

    this.setColor = function (value) {
        if (title) title.style.color = value;
    };

    this.setFontSize = function (value) {
        if (title) title.style.fontSize = value && value + "px";
    };

    this.update = function (model) {
        var
            index,
            newItems = [],
            newItemIndexes = {},
            lastItem = null,
            item,
            itemModel;

        update(model);

        /* aggiorna ricorsivamente: Groups per le pagine, Items per i gruppi e Iterations per gli iteratori; crea le nuove iterazioni  */
        var
            children = model.groups ? model.groups : model.iterator ? model.iterations : model.items,
            even = true;
        
        for (index in children) {
		    itemModel = children[index];
            
            if (!itemIndexes[itemModel.id]) 
                item = itemModel.typeName == "Group" || itemModel.typeName == "Iteration" ? new instances.Group(null, itemModel.typeName, this, itemModel) : new instances.Item(itemModel, this);
            else
                item = items[itemIndexes[itemModel.id]];

            item.update(itemModel);
            if (model.iterator) {
                var itemElement = item.getElement();
                if (model.moveIterationButton)
                    item.appendElement();
                if (!itemModel.removed) {
                    if (lastItem == null) //first visible items
                        itemElement.classList.add("FirstIteration")
                    lastItem = item;
                }

                if (!itemModel.removed) {
                    if (even) {
                        itemElement.classList.remove("Odd")
                        itemElement.classList.add("Even")
                    } else {
                        itemElement.classList.remove("Even")
                        itemElement.classList.add("Odd")
                    }
                    even = !even;
                }
            }

            newItemIndexes[itemModel.id] = index;
            newItems.push(item);
        }

        if (model.iterator && model.showAsTable) updateTableHeader(model);

        items = newItems;
        itemIndexes = newItemIndexes;

        if (lastItem)
            lastItem.getElement().classList.add("LastIteration")

        if (iterationCommands) element.appendChild(iterationCommands);
        if (actionButtonsBar) element.appendChild(actionButtonsBar);

    };

    this.getSetupModel = function () {
        return setupModel;
    };

    this.clear = function () {
        element.innerHTML = "";
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

    this.getTableBody = function () {
        return tableBody;
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

	this.setPageIndex = function (value) {
		pageIndex = value;
	};

	this.getPageIndex = function () {
		return pageIndex;
	};

    setup();
}