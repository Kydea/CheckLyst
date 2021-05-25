reports.ColumnsList = function (options) {
    var
        columnsList = this,
        model = reports.reportEditor.getModel(),
        element,
        autoCenter,
        table,
        tableBody,
        noRecord,
        currentIndex,
        list = [],
        openButton,
        orderBar,
        filtersBar,
        fixedHeader,

        getRow = function (index) {
            return $("tr", tableBody).eq(index);
        },

        toggleMoveButton = function () {
            orderBar.children().eq(0).toggleClass("Off", currentIndex == null || currentIndex == 0 || filtersBar.isFiltering());
            orderBar.children().eq(1).toggleClass("Off", currentIndex == null || currentIndex == list.length - 1 || filtersBar.isFiltering());
        },

        toggleNoRecord = function () {
            if ($("tr:not(.NoRecord):visible", tableBody).length == 0)
                noRecord.appendTo(tableBody);
            else
                noRecord.remove();
        },

        up = function () {
            var
                item = list[currentIndex],
                row = getRow(currentIndex);

            list[currentIndex] = list[currentIndex - 1];
            list[currentIndex - 1] = item;

            row.insertBefore(row.prev());
            --currentIndex;

            toggleMoveButton();
            matematici.canClose = false;
        },

        down = function () {
            var
                item = list[currentIndex],
                row = getRow(currentIndex);

            list[currentIndex] = list[currentIndex + 1];
            list[currentIndex + 1] = item;

            row.insertAfter(row.next());
            ++currentIndex;

            toggleMoveButton();
            matematici.canClose = false;
        },

        select = function (index) {
            if (currentIndex != null) {
                getRow(currentIndex).removeClass("Selected");
                if (currentIndex == index) open(currentIndex);
            }
            currentIndex = index;
            if (currentIndex != null)
                getRow(currentIndex).addClass("Selected");

            toggleMoveButton();

            autoCenter.center();
        },

        open = function (index) {
            new reports.ColumnEditor(list, options.columns, index,
                function (row) {
                    matematici.canClose = false;
                    if (index == null)
                        select(insert(row, currentIndex));
                    else {
                        list[index] = row;
                        update(index);
                    }
                }, remove);
        },

        prettify = function (name) {
            return name.split("\"").join("").replace(/([a-z0-9])([A-Z])/g, "$1 $2");
        };

        generate = function () {
            var
                currentColumns = list.map(function (column) { return column.column; }),
                createColumns = function () {
                    for (var index = 0; index < options.columns.length; ++index) {
                        var queryColumn = options.columns[index];
                        if (!currentColumns.includes(queryColumn))
                            insert({
                                label: prettify(queryColumn),
                                column: queryColumn,
                                visible: true
                            });
                    }
                },
                deleteColumns = function () {
                    for (var index = options.columns.length - 1; index >= 0; --index) {
                        var queryColumn = options.columns[index];
                        remove(index);
                    }
                };

            var buttons = [
                { text: "cancel", icon: "times", cancelButton: true },
                {
                    text: "delete columns", icon: "trash-alt", handler: function () {
                        deleteColumns();
                        autoCenter.center();
                    }
                }, {
                    text: "replace columns", icon: "sync", handler: function () {
                        deleteColumns();
                        createColumns();
                        autoCenter.center();
                    }
                }, {
                    text: "add columns", icon: "plus", handler: function () {
                        createColumns();
                        autoCenter.center();
                    }
                }
            ];
            matematici.dialog.popupMenu(buttons);
        },

        removeItem = function (index) {
            list.splice(index, 1);
        },

        remove = function (index) {
            matematici.canClose = false;
            getRow(index == null ? currentIndex : index).remove();
            removeItem(currentIndex);
            toggleNoRecord();
            colorRows();
            select();
        },

        insertItem = function (row, index) {
            if (index == null) {
                list.push(row);
                return list.length - 1;
            } else {
                list.splice(index + 1, 0, row);
                return index + 1;
            }
        },

        insert = function (row, index) {
            var
                result,
                tr = $("<tr />")
                    .addClass("Handler")
                    .append($("<td />").append($("<i />").addClass("fa fa-caret-right")).attr("title", row.serializedValue))
                    .append($("<td />").addClass("Label"))
                    .append($("<td />").addClass("Column"))
                    .append($("<td />").addClass("Centered")
                        .append($("<i />").attr("title", "Visible").addClass("Visible fal fa-eye"))
                        .append($("<i />").attr("title", "Order").addClass("Order fal fa-list-ol"))
                        .append($("<i />").attr("title", "Group").addClass("Group fal fa-object-group"))
                    );

            noRecord.remove();
            insertItem(row, index);

            if (index == null) {
                tr.appendTo(tableBody)
                update(list.length - 1);
                result = list.length - 1;
            } else {
                tr.insertAfter(getRow(index));
                update(index + 1);
                result = index + 1;
            }

            colorRows();
            fixedHeader && fixedHeader.refresh();

            return result;
        },

        update = function (index) {
            var
                row = list[index],
                cells = $("td", getRow(index)),
                commands = cells.last();

            cells.filter(".Label").text(row.label).attr("title", row.label);
            cells.filter(".Column").text(row.column).attr("title", row.column);
            commands.find(".Visible").toggleClass("Off", !row.visible);
            commands.find(".Order").toggleClass("Off", !row.orderColumn);
            commands.find(".Group").toggleClass("Off", !row.groupColumn);
        },

        updateReport = function () {
            model.columns = list;
        },

        verify = function (handler) {
            updateReport();
            reports.reportEditor.test();
        },

        close = function () {
            element.remove();
            matematici.elements.body.removeClass("CheckListGrid");
            autoCenter.destroy();
            matematici.columnsList = null;
        },

        colorRows = function () {
            var
                filtering = filtersBar.isFiltering(),
                even = false;
            $("tr:visible", tableBody).each(function () {
                $(this).toggleClass("Even", filtering && even).toggleClass("Odd", filtering && !even);
                even = !even;
            });
        },

        filter = function (filtering, keyword, flags) {
            var
                rows = $("tr", tableBody),
                even = false;
            for (var index in list) {
                var
                    column = list[index],
                    visible = keyword == null;
                if (!visible && flags[0]) visible = !!column.label && column.label.toLowerCase().indexOf(keyword.toLowerCase()) >= 0;
                if (!visible && flags[1]) visible = !!column.column && column.column.toLowerCase().indexOf(keyword.toLowerCase()) >= 0;
                rows.eq(index).toggle(visible).toggleClass("Even", filtering && even).toggleClass("Odd", filtering && !even);
                if (visible) even = !even;
            }
            toggleNoRecord();
            toggleMoveButton();
            fixedHeader && fixedHeader.refresh();
            select();
        },

        setup = function () {
            matematici.elements.body.addClass("CheckListGrid");
            matematici.columnsList = columnsList;

            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            var container = $("<div />").appendTo($("<div />").addClass("Container ColumnsList").appendTo(element));

            autoCenter = new matematici.AutoCenter(container.parent());
            autoCenter.initialize();

            container.append($("<h2 />").html("Columns list"));
            filtersBar = new matematici.GridFiltersBar(container, filter);
            filtersBar.addFlag("label", true);
            filtersBar.addFlag("column", true);

            container
                .append($("<a />").attr("href", "#").addClass("CloseButton CancelButton")
                    .append($("<i />").addClass("fas fa-times-circle"))
                    .on("click", function () {
                        updateReport();
                        close();
                    }));

            orderBar = $("<div />")
                .appendTo(container)
                .addClass("OrderBar")
                .append($("<a />").addClass("Icon Up").append($("<i />").addClass("fa fa-caret-up")))
                .append($("<a />").addClass("Icon Down").append($("<i />").addClass("fa fa-caret-down")))
                .on("click", "a.Up:not(.Off)", function (e) {
                    e.preventDefault();
                    up();
                }).on("click", "a.Down:not(.Off)", function (e) {
                    e.preventDefault();
                    down();
                });

            toggleMoveButton();

            var scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container);

            table = $("<table />").addClass("Grid").appendTo(scrollablePanel)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />"))
                        .append($("<th />").text("label"))
                        .append($("<th />").text("column"))
                        .append($("<th />"))
                    )
                ).append(tableBody = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").addClass("NoRecord").attr("colspan", "4").text("(no column)"))
                    )
                ).on("click", "tr.Handler td:not(.Inactive)", function () {
                    select($(this).parent().prevAll().size());
                });

            var
                buttons = $("<div />").addClass("Buttons").appendTo(container)
                    .append($("<a />").addClass("Button DefaultButton").text("close").prepend($("<i />").addClass("Icon fa fa-check")).on("click", function () {
                        updateReport();
                        close();
                    }))
                    .append(openButton = $("<a />").addClass("Button").text("open").hide().prepend($("<i />").addClass("Icon fa fa-edit")).on("click", function () {
                        open(currentIndex);
                    }))
                    .append($("<a />").addClass("Button").text("add").prepend($("<i />").addClass("Icon fa fa-plus")).on("click", function () {
                        open();
                    }))
                    .append($("<a />").addClass("Button").text("generate").prepend($("<i />").addClass("Icon fa fa-magic")).on("click", function () {
                        generate();
                    }));

            for (var index in model.columns)
                insert(model.columns[index]);

            fixedHeader = new matematici.GridFixedHeader(table);
        };

    this.select = function (code) {
        for (var item in list)
            if (list[item].code == code) {
                select(item);
                return;
            }
    };

    setup();
};