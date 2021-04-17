instances.ExternalSources = function (itemId, name, multiSelection) {
    var
        element,
        page = 0,
        parameters,
        parameterValues = {},
        columns,
        noRecord,
        next,
        searchButton,
        confirmButton,
        header,
        container,
        table,
        tableBody,
        autoCenter,
        fixedHeader,
        selectionCheckBox,
        totalRows = 0,
        selectedRows = 0,
        inputs = [],

        toggleSelection = function () {
            var value = selectionCheckBox.prop("indeterminate") ? null : selectionCheckBox.prop("checked");
            $("tr td:first-child input", tableBody).each(function () {
                this.checked = value == true;
                this.indeterminate = value == null;
            });
            selectedRows = value ? totalRows : 0;
            selectionCheckBox.indeterminate = false;
            selectionCheckBox.value = value != true;
            confirmButton.toggle(selectedRows > 0);
        },

        refreshFixedHeader = function () {
            if (fixedHeader)
                fixedHeader.refresh();
            else
                fixedHeader = new matematici.GridFixedHeader(table);
            if (multiSelection) $("th:first-child", fixedHeader.getTableHeader()).append(selectionCheckBox = $("<input />").attr("type", "checkbox").on("click", toggleSelection))
        },

        addFilter = function (parameter) {
            if (!parameter.hidden) {
                var label = $("<label />").append($("<span />").text(parameter.label));
                inputs.push(new matematici.TypedInput(label[0], parameter.parameterType, { name: parameter.name, value: parameter.value, readOnly: parameter.readOnly }))
                parameters.append(label);
            }
        },

        close = function () {
            autoCenter.destroy();
            matematici.elements.body.removeClass("CheckListGrid");
            matematici.checkListGrid = null;
            element.remove();
        },

        toggleRow = function (tr) {
            var value = checkBox = $("input", tr).prop("checked");
            selectedRows += value ? 1 : -1;
            selectionCheckBox.prop("indeterminate", totalRows > selectedRows && selectedRows > 0);
            selectionCheckBox.prop("checked", totalRows == selectedRows);
            confirmButton.toggle(selectedRows > 0);
        },

        selectRow = function (tr) {
            var
                row = tr.data("row"),
                data = {};

            for (var index = 0; index < columns.length; ++index)
                data[index] = { key: columns[index], value: row[index] };

            new instances.Event(instances.Events.externalSourceRowChange, itemId, { rowIndex: tr.data("index"), row: data }).trigger();
            close();
        },

        rowClicked = function () {
            var tr = $(this);

            if (multiSelection)
                $("input", tr).click();
            else
                selectRow(tr);
        },

        setup = function () {
            matematici.elements.body.addClass("CheckListGrid");
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            var container = $("<div />").appendTo($("<div />").addClass("Container ExternalSources").appendTo(element));
            if (multiSelection) container.parent().addClass("MultiSelection");

            autoCenter = new matematici.AutoCenter(container.parent());
            autoCenter.initialize();

            container.append($("<a />").attr("href", "#").addClass("CloseButton CancelButton").append($("<i />").addClass("fas fa-times-circle")).on("click", close));

            $("<h2 />").text(name).appendTo(container);
            parameters = $("<div />").addClass("Parameters Module").appendTo(container);

            var scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container).hide();

            table = $("<table />")
                .addClass("Grid")
                .appendTo(scrollablePanel)
                .append($("<thead />").append(header = $("<tr />")))
                .append(tableBody = $("<tbody />"))
                .on("click", "tr.Handler", rowClicked)
                .on("click", "tr.Next", function () {
                    ++page;
                    load();
                });

            $("<div />").addClass("Buttons").appendTo(container)
                .append(searchButton = $("<a />").addClass("DefaultButton Button").text("search").prepend($("<i />").addClass("Icon fa fa-search")).on("click", function () {
                    page = 0;
                    if (setParameterValues()) load();
                }))
                .append(confirmButton = $("<a />").addClass("Button").text("confirm").prepend($("<i />").addClass("Icon fa fa-file-import")).on("click", confirm).hide());

            $.ajax(matematici.urls.instances.api + "/GetExternalSource", {
                method: "post",
                data: { instanceCode: instances.instance.getCode(), externalSourceCode: name, itemId: itemId }
            }).done(function (result) {
                for (var index in result.parameters)
                    addFilter(result.parameters[index]);

                columns = result.columns;

                if (multiSelection) header.append($("<th />"));
                for (var column in columns)
                    if (columns[column][0] != "_")
                        header.append($("<th />").text(columns[column].split("_").join(" ")));

                toggleNoRecord(true);
                scrollablePanel.show();
                refreshFixedHeader();

                autoCenter.center();
            });
        },

        setParameterValues = function () {
            parameterValues = {};
            var valid = true;
            for (var index in inputs) {
                var input = inputs[index];
                if (!input.validate()) valid = false;
                parameterValues[index] = {
                    key: input.getOption("name"),
                    value: input.getValue()
                }
            }
            if (!valid) parameterValues = {};
            return valid;
        },

        toggleNoRecord = function (value) {
            if (!noRecord)
                noRecord = $("<tr />").addClass("NoRecord").append($("<td />").attr("colspan", header.children().size()).text("(no record)"));
            if (value)
                tableBody.append(noRecord);
            else
                noRecord.remove();
        },

        toggleNext = function (value) {
            if (!next)
                next = $("<tr />").addClass("Next").append($("<td />").attr("colspan", header.children().size()).append($("<i />").addClass("Icon fa fa-angle-double-down")));

            if (value)
                next.appendTo(tableBody);
            else
                next.remove();
        },

        confirm = function () {
            var
                data = {
                    value: name,
                    columns: [],
                    rows: []
                };

            for (var index = 0; index < columns.length; ++index)
                data.columns.push(columns[index]);

            $("tr td:first-child input:checked", tableBody).each(function () {
                data.rows.push($(this).parent().parent().data("row"));
            });
            new instances.Event(instances.Events.import, itemId, data).done(function (result) {
                instances.instance.update(result);
                close();
            }).trigger();
        },

        load = function () {
            $.ajax(matematici.urls.instances.api + "/GetExternalSourceRows", {
                method: "post",
                data: { data: { instanceCode: instances.instance.getCode(), parameters: parameterValues, externalSourceCode: name, page: page } }
            }).done(function (result) {
                if (page == 0) {
                    tableBody.empty();
                    totalRows = 0;
                }

                var rows = JSON.parse(result).rows;
                if (rows && rows.length) {
                    for (var row in rows) {
                        var tr = $("<tr />").appendTo(tableBody).addClass("Handler");
                        if (multiSelection) tr.append($("<td />").append($("<input />").attr({ type: "checkbox" }).on("click", function (e) {
                            e.stopPropagation();
                            toggleRow($(this).parent());
                        })));
                        for (var column in rows[row])
                            if (columns[column][0] != "_")
                                tr.append($("<td />").html(rows[row][column])).data({ index: parseInt(row) + 20 * page, row: rows[row] });
                    }
                    totalRows += rows.length;
                }

                toggleNoRecord(page == 0 && (!rows || !rows.length));
                toggleNext(rows && rows.length >= 20 && page < 4);
                refreshFixedHeader();
                autoCenter.center();

            });
        };

    setup();
}