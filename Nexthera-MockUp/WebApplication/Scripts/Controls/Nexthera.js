matematici.Nexthera = function (data) {
    var
        content = matematici.elements.content,

        setupFilters = function (panel, filters) {
            var
                columnsSelect = $("select#Columns", panel);
            columnsSelect.append($("<option />").text("(add a filter)").attr("value", "-")).on("change", function () {
                panel.addClass("Added")
                panel.parent().addClass("Filtered");
                $("select", content).each(function () {
                    matematici.AutoComplete.get(this).refresh();
                });
            });
            for (var index in filters) {
                if (filters[index].label && filters[index].label != "")
                    columnsSelect.append($("<option />").attr("value", filters[index].label).text(filters[index].label));
            }

            panel.on("click", "i.Remove", function () {
                panel.removeClass("Added")
                panel.parent().removeClass("Filtered");
                matematici.AutoComplete.get(columnsSelect[0]).setValue("-");
            });
        },

        setupBlocks = function () {
            var tables = $("div.Tables", content);
            for (var blockIndex in data.blocks) {
                var
                    block = data.blocks[blockIndex],
                    form = $("<form />").insertBefore(tables).toggleClass("Emphasy", !!block.emphasy);

                for (var fieldIndex in block.fields) {
                    var
                        field = block.fields[fieldIndex],
                        label;

                    if (!field.handler) {
                        label = $("<label />").addClass(field.cssClass || "").appendTo(form);
                        var span = $("<span />").html(field.label).appendTo(label);
                        if (field.um)
                            $("<span />").html(" " + field.um).addClass("UM").appendTo(span);
                    }

                    var input;
                    if (field.handler) {
                        input = $("<a />").attr("href", "#").addClass("Button GreyYellowButton").text(field.label).on("click", field.handler).appendTo(form);
                        if (field.icon)
                            $("<i />").addClass("Icon fa fa-" + field.icon).appendTo(input);
                    } else if (field.value === true || field.value === false)
                        input = $("<input />").attr("type", "checkbox").prop("checked", field.value).appendTo(label);
                    else if (field.values) {
                        input = $("<select />").appendTo(label);
                        if (field.selectedIndexes)
                            input.attr("multiple", "multiple")
                        for (var valueIndex = 0; valueIndex < field.values.length; ++valueIndex) {
                            var option = $("<option />").text(field.values[valueIndex]).attr("value", field.values[valueIndex]).appendTo(input);
                            if (valueIndex == (field.selectedIndex || 0) || field.selectedIndexes && field.selectedIndexes.includes(valueIndex))
                                option.attr("selected", "selected");
                        }
                    } else if (field.area)
                        input = $("<textarea />").appendTo(label);
                    else {
                        if (field.icon)
                            $("<i />").addClass("Icon fal fa-" + field.icon).appendTo(label);
                        input = $("<input />").attr("type", "text").val(field.value || "").appendTo(label);
                    }
                    if (field.editable === false)
                        matematici.controls.setEditable(input, false);

                    if (field.values)
                        new matematici.AutoComplete(input[0]).initialize();
                }
            }
        },

        openTableMenu = function () {
            matematici.dialog.popupMenu([
                { text: "(cancel)", cancelButton: true },
                { text: "reload settings", icon: "fal fa-sync-alt" },
                { text: "save settings", icon: "fal fa-save" },
                { text: "toggle columns", icon: "fal fa-toggle-on" }
            ])
        },

        setupGrid = function (table, tableData) {
            var
                head = $("thead tr", table),
                body = $("tbody", table)

            $(".TableMenu", table).on("click", openTableMenu)

            for (var columnIndex in tableData.columns) {
                var
                    column = tableData.columns[columnIndex],
                    th = $("<th />").html(column.label).addClass(column.cssClass || "").append($("<span />").addClass("UM").html(column.um ? " (" + column.um + ")" : ""));
                if (column.width != null)
                    th.css("min-width", column.width + "px");
                head.append(th);
            }
            if (tableData.log != false) {
                head.append($("<th />").text("created by").css("min-width", "200px"));
                head.append($("<th />").text("created on").css("min-width", "200px"));
                head.append($("<th />").text("modified by").css("min-width", "200px"));
                head.append($("<th />").text("modified on").css("min-width", "200px"));
            }

            for (var rowIndex = 0; rowIndex < tableData.rows.length; ++rowIndex) {
                var
                    row = tableData.rows[rowIndex],
                    tr = $("<tr />").appendTo(body);
                if (rowIndex == (tableData.selectedIndex || 0)) {
                    tr.addClass("Selected");
                    $("<td />").append($("<i />").addClass("fa fa-caret-right")).appendTo(tr);
                } else
                    $("<td />").appendTo(tr);
                for (var cellIndex = 0; cellIndex < tableData.columns.length + (tableData.log == false ? 0 : 4); ++cellIndex) {
                    var
                        value = row[cellIndex],
                        td = $("<td />").appendTo(tr);

                    if (value === true)
                        td.append($("<i />").addClass("fa fa-check"));
                    else
                        td.html(value || "");
                    if (tableData.columns[cellIndex] && tableData.columns[cellIndex].centered)
                        td.addClass("Centered");
                    if (tableData.columns[cellIndex] && tableData.columns[cellIndex].cellCssClass)
                        td.addClass(tableData.columns[cellIndex].cellCssClass);
                }
            }
        },

        refreshContent = function () {
            matematici.elements.content.css("margin-bottom", $("div.Tables").outerHeight());
        },

        maximize = function (table) {
            table.css("margin-top", matematici.breadcrumbs.getElement().outerHeight() + $("div.Toolbar").outerHeight() + 1);
        },

        setup = function () {
            $(".fa-window-minimize", content).on("click", function () {
                $(this).closest("div.Table").addClass("Compact").removeClass("Extended").removeClass("Filters")
                    .css("margin-top", 0);
                refreshContent();
            });

            $(".fa-window-restore", content).on("click", function () {
                $(this).closest("div.Table").removeClass("Compact").removeClass("Extended").removeClass("Filters")
                    .css("margin-top", 0);
                refreshContent();
            });

            $(".fa-window-maximize", content).on("click", function () {
                maximize($(this).closest("div.Table").removeClass("Compact").addClass("Extended").removeClass("Filters"));
                refreshContent();
            });

            $(".fa-filter", content).on("click", function () {
                maximize($(this).closest("div.Table").removeClass("Compact").addClass("Extended").toggleClass("Filters"));
                $("select", content).each(function () {
                    matematici.AutoComplete.get(this).refresh();
                });
                refreshContent();
            });

            setupBlocks();
            $("div.Tables div.Table", content).each(function () {
                var
                    panel = $(this),
                    tableData = data.tables[panel.attr("id")];

                panel
                    .toggleClass("Extended", !!tableData.extended)
                    .toggleClass("Compact", !!tableData.compact)
                    .toggleClass("Filters", !!tableData.filters);

                if (tableData.extended) maximize(panel);
                $("span.Title", panel).html(tableData.label + " <span class=\"RowsCount\">" + tableData.rows.length + " rows<span>");
                setupFilters($("form.Filters", panel), tableData.columns);
                setupGrid($("div.Data table", panel), tableData);
            });

            refreshContent();
        };

    this.initialize = function () {
        setup();
    }
}

matematici.data = {

    therapies: function (compact) {
        return {
            filters: !compact,
            extended: !compact,
            compact: compact,
            label: "therapies",
            columns: [
                { label: "id", cssClass: "Down", width: 170 },
                { label: "therapy type", width: 300 },
                { label: "donor", width: 170 },
                { label: "patient", width: 170 },
                { label: "manufacturing type", width: 170 },
                { label: "donation type" },
                { label: "state", width: 120 },
                { label: "current phase", width: 160 },
                { label: "price" },
                { label: "amount paid" },
                { label: "started by", width: 200 },
                { label: "started on", width: 200 },
                { label: "aborted by", width: 200 },
                { label: "aborted on", width: 200 },
            ],
            rows: [
                ["CHA0001-23-00123", "cart.1 - Car-T", "A0001-23-00AA01", "A0001-23-00AA01", "made-to-order", "autologous", "in progress", "cell manufacturing", "350.000,00", "", "", false, "2023-01-01 15:10:08", "John Smith", "Ann Clark", "2023-02-01 10:50:18", "John Smith", "2023-01-01 15:10:08"],
                ["CHA0001-23-00124", "carnk-1 - Car-NK", "A0001-23-00AA01", "A0001-23-000N04", "made-to-stock", "allogeneic", "completetd", "", "250.000,00", "", "", true, "2023-02-01 10:50:18", "Ann Clark", "Giovanni Bianchi", "2023-02-01 10:50:18"],
                ["CHA0001-23-00125", "cart.1 - Car-T", "A0001-23-00CK36", "A0001-23-00CK36", "made-to-order", "autologous", "proposed", "", "350.000,00", "", "", false, "2023-01-01 15:10:08", "John Smith", "Ann Clark", "2023-02-01 10:50:18", "John Smith", "2023-01-01 15:10:08"],
                ["CHA0001-23-00126", "aav-8a1.1 - adeno-associated virus 8a1", "A0001-23-00JJ86", "", "made-to-stock", "n/a", "proposed", "", "50.000,00", "", "", true, "2022-12-01 15:10:18", "Mario Rossi", "Juan Lopez", "2023-02-01 10:50:18"],
                ["CHA0001-23-00127", "pcv.1 - personalized cancer vaccin", "A0001-23-00CX43", "A0001-23-00CX43", "made-to-order", "autologous", "proposed", "", "315.000,00", "", "", false, "2023-02-10 14:41:50", "Giovanni Bianchi", "Juan Lopez", "2023-02-01 10:50:18"],
                ["CHA0001-23-00128", "mrna-7z4.1 - mRNA therapy 7z4", "A0001-23-00CX63", "", "assembled-to-order", "n/a", "in progress", "cell manufacturing", "155.000,00", "", "", true, "2022-12-15 11:40:08", "Juan Lopez", "Giovanni Bianchi", "2023-02-01 10:50:18"]
            ]
        };
    },

    therapyTypes: function (compact) {
        return {
            filters: !compact,
            extended: !compact,
            compact: compact,
            label: "therapy types",
            columns: [
                { label: "id", cssClass: "Down" },
                { label: "description" },
                { label: "manutacturing type" },
                { label: "donation type" },
                { label: "price", um: "&euro;" },
                { label: "validity start" },
                { label: "validity end" },
                { label: "active", cssClass: "Tight", centered: true }
            ],
            rows: [
                ["carnk.1", "Car-NK", "made-to-stock", "allogeneic", "250.000,00", "", "", true, "2023-02-01 10:50:18", "Ann Clark"],
                ["cart.1", "Car-T", "made-to-order", "autologous", "350.000,00", "", "", false, "2023-01-01 15:10:08", "John Smith"],
                ["aav-8a1.1", "adeno-associated virus 8a1", "made-to-stock", "n/a", "50.000,00", "", "", true, "2022-12-01 15:10:18", "Mario Rossi"],
                ["mrna-7z4.1", "mRNA therapy 7z4", "assembled-to-order", "n/a", "155.000,00", "", "", true, "2022-12-15 11:40:08", "Juan Lopez"],
                ["pcv.1", "personalized cancer vaccin", "made-to-order", "autologous", "315.000,00", "", "", false, "2023-02-10 14:41:50", "Giovanni Bianchi"],
            ],
            selectedIndex: 1
        }

    }


};