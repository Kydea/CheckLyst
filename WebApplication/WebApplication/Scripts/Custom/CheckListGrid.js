matematici.CheckListGrid = function (title, values, handler, renderHandler, columns, rowRenderHandler) {
    var
        checkListGrid = this,
        element,
        container,
        scrollablePanel,
        titleBar,
        parameters,
        tableHeader,
        tableBody,
        autoCenter,
        fixedHeader,
        next,
        nextHandler,

        toggleNext = function (value) {
            if (!next)
                next = $("<tr />").addClass("Next").append($("<td />").attr("colspan", tableHeader.children().size()).append($("<i />").addClass("Icon fa fa-angle-double-down")));

            if (value)
                next.appendTo(tableBody);
            else
                next.remove();
        },

        setup = function () {
            matematici.checkListGrid = checkListGrid;
            matematici.elements.body.addClass("CheckListGrid");
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            container = $("<div />").appendTo($("<div />").addClass("Container").appendTo(element));
            titleBar = $("<h2 />").html(title).appendTo(container);
            scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container);
            autoCenter = new matematici.AutoCenter(container.parent());
            autoCenter.initialize();

            renderValues();
        },

        addRow = function (row, cssClass, rowIndex) {
            var tr = $("<tr />").data("row", row).appendTo(tableBody).toggleClass("Handler", !!handler);
            if (cssClass) tr.addClass(cssClass);
            for (var columnIndex in columns) {
                var column = columns[columnIndex];
                if (!renderHandler || !renderHandler(tr, row, rowIndex, columnIndex, row[column.name])) {
                    var
                        td = $("<td />"),
                        value = column.name ? row[column.name] : rowIndex;
                    if (column.align == "center")
                        td.addClass("Centered");
                    if (column.type == "DateTimeOffset") {
                        if (value) {
                            var formattedDate = matematici.dateFormatter.format(moment.parseZone(value));
                            td.append($("<span />").html(formattedDate.date + " " + formattedDate.time + ":" + formattedDate.seconds).attr("title", formattedDate.offset));
                        }
                    } else if (value === true || column.type == "Boolean" && value && value.toLowerCase() == "true")
                        td.append($("<i />").addClass("fa fa-check")).addClass("Centered");
                    else if (value === false || column.type == "Boolean" && value && value.toLowerCase() == "false")
                        td.append($("<i />").addClass("fa fa-times")).addClass("Centered");
                    else
                        td.html(value)
                    tr.append(td);
                }
            }

            return tr;
        },

        renderValues = function () {
            scrollablePanel.empty();

            var table = $("<table />").addClass("Grid").appendTo(scrollablePanel)
                .append($("<thead />")
                    .append(tableHeader = $("<tr />"))
                )
                .append(tableBody = $("<tbody />"))
                .on("click", "tr.Handler", function () {
                    if (!handler || !handler.call(this, checkListGrid, $(this).data("row")))
                        checkListGrid.close();
                }).on("click", "tr.Next", function () {
                    nextHandler();
                });

            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container)
                        .append($("<a />").addClass("Button CancelButton").text("close").prepend($("<i />").addClass("Icon fa fa-times")).on("click", function () {
                            checkListGrid.close();
                        }));

            if (values.length == 0) {
                tableHeader.append($("<th />").text("-"));
                tableBody.append($("<tr />").addClass("NoRecord").append($("<td />").attr("colspan", 1).text("(no record)")))
            } else {
                if (columns) {
                    for (var column in columns)
                        tableHeader.append($("<th />").text(columns[column].label));
                    for (var rowIndex in values) {
                        var row = values[rowIndex];
                        if (!rowRenderHandler || !rowRenderHandler(checkListGrid, rowIndex, row))
                            addRow(row, null, rowIndex);
                    }
                } else {
                    var row = values[0];
                    var isArray = Array.isArray(row);

                    for (var column in row) {
                        var label = isArray ? row[column] : column;
                        label = (label || "").replace("_", " ");
                        if (!renderHandler || !renderHandler.call(this, row, column, label))
                            tableHeader.append($("<th />").text(label));
                    }

                    var rows = isArray ? values.slice(1) : values;
                    for (var row in rows) {
                        var tr = $("<tr />").data("row", rows[row]).appendTo(tableBody).toggleClass("Handler", !!handler);
                        for (var column in rows[row]) {
                            if (!renderHandler || !renderHandler.call(this, row, column, rows[row][column])) {
                                tr.append($("<td />").html((rows[row][column] == null || rows[row][column] == undefined ? "" : rows[row][column]).toString()))
                            }
                        }
                    }
                }
            }
            fixedHeader = new matematici.GridFixedHeader(table);
        };

    checkListGrid.setValues = function (v) {
        values = v;
        renderValues();
    };

    checkListGrid.getParameters = function () {
        parameters = $("<div />").addClass("Parameters Module").insertAfter(titleBar);
        return parameters;
    }

    checkListGrid.close = function () {
        autoCenter.destroy();
        element.remove();
        matematici.elements.body.removeClass("CheckListGrid");
        matematici.checkListGrid = null;
    }

    checkListGrid.getTitle = function () {
        return title;
    };

    checkListGrid.refresh = function () {
        if (fixedHeader)
            fixedHeader.refresh();
    };

    checkListGrid.setNextHandler = function (handler) {
        nextHandler = handler;
        toggleNext(!!handler);
    };

    checkListGrid.center = function () {
        autoCenter.center();
    };

    checkListGrid.addRow = addRow;

    setup();
}