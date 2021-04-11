instances.ExternalSources = function (itemId, name, multiSelection) {
    var
        element,
        page = 0,
        parameters,
        columns,
        noRecord,
        next,
        search,
        header,
        container,
        tableBody,
        autoCenter,
        fixedHeader,
        selectedRows = 0,

        filterChanged = function () {
            next.hide();
            page = 0;
        },

        addFilter = function (parameter) {
            parameters
                .append($("<label />")
                    .append($("<span />").text(parameter.label))
                    .append($("<input />").attr({ type: "text", id: parameter.name }).on("change, keydown", filterChanged))
                );
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
            console.log(selectedRows);
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

            var
                scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container).hide(),
                table = $("<table />").addClass("Grid").appendTo(scrollablePanel)
                    .append($("<thead />")
                        .append(header = $("<tr />"))
                    ).append(
                        tableBody = $("<tbody />")
                    ).on("click", "tr.Handler", rowClicked);

            $("<div />").addClass("Buttons").appendTo(container)
                .append(search = $("<a />").addClass("DefaultButton Button").text("search").prepend($("<i />").addClass("Icon fa fa-search")).on("click", function () {
                    load();
                }));

            $.ajax({
                type: "post",
                url: matematici.urls.dataSources.api + "/Data/GetDataSource",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    code: instances.instance.getExternalSources()[name]
                })
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
                fixedHeader = new matematici.GridFixedHeader(table);

                autoCenter.center();
            });
        },

        getValues = function () {
            var values = {};
            $("input", parameters).each(function (index) {
                var input = $(this);
                values[index] = {
                    key: input.attr("id"),
                    value: input.val()
                }
            });
            return values;
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
                next = $("<tr />").addClass("Next").append($("<td />").attr("colspan", header.children().size()).append($("<i />").addClass("Icon fa fa-angle-double-down"))).on("click", function () {
                    ++page;
                    load();
                });

            if (value)
                next.appendTo(tableBody);
            else
                next.remove();
        },

        fill = function (rows) {
            if (page = 0)
                tableBody.empty();

            if (rows && rows.length)
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

            toggleNoRecord(page == 0 && (!rows || !rows.length));
            toggleNext(rows.length >= 20);
            fixedHeader.refresh();
            autoCenter.center();
        },

        load = function () {
            $.ajax(matematici.urls.instances.api + "/GetExternalSourceRows", {
                method: "post",
                data: { data: { instanceCode: instances.instance.getCode(), parameters: getValues(), externalSourceCode: name, page: page } }
            }).done(function (result) {
                fill(JSON.parse(result).rows);
            });
        };

    setup();
}