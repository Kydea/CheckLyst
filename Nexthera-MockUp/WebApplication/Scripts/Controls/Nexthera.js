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
            var top = matematici.breadcrumbs.getElement().outerHeight() + $("div.Toolbar").outerHeight() + 1;
            table.css({
                "margin-top": top,
                "height": "calc(100% - " + top + "px)"
            });
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