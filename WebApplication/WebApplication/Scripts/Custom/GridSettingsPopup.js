matematici.GridSettingsPopup = function (grid) {
    var
        canClose = true,
        content,

        close = function () {
            if (!canClose)
                matematici.dialog.show("warning", "The updates will be lost.",
                    [{
                        text: "confirm", icon: "check", handler: function () {
                            matematici.popup.close();
                        }
                    },
                    {
                        text: "cancel", icon: "times", cancelButton: true
                    }]
                );
            return !canClose;
        },

        save = function () {
            var
                model = { columns: [] },
                defaultModel = grid.getDefaultSettings(),
                columns = $("div.Column", ".Columns"),
                asDefault = columns.lenght === defaultModel.lenght;

            $("div.Column", ".Columns").each(function (index) {
                var
                    column = $(this),
                    defaultModelColumn = defaultModel.columns[index],
                    modelColumn = { name: column.data("column"), visible: $("input", column).prop("checked") };

                model.columns.push(modelColumn);
                if (asDefault && (defaultModelColumn.name !== modelColumn.name || defaultModelColumn.visible !== modelColumn.visible)) asDefault = false;
            });

            matematici.userSettingsClient.save({ setting: "GridSetup", code: grid.getName(), value: asDefault ? null : JSON.stringify(model) });
            grid.applySettings(model);
            return false;
        },

        reset = function () {
            content.empty();
            applySettings(grid.getDefaultSettings());
            return true;
        },

        applySettings = function (settings) {
            for (var column of settings.columns) {
                var
                    columnDomElements = grid.getColumnDomElements(column.name),
                    div = $("<div />").text(columnDomElements.label).data("column", column.name).attr("draggable", true).addClass("Column");
                $("<input />").attr("type", "checkbox").prependTo(div).prop("checked", column.visible);
                if (columnDomElements.advancedSearch) div.append($("<span />").addClass("Note").text("(advanced search)"));
                content.append(div);
            }
        },

        show = function () {
            var
                buttons = [{
                    text: "save",
                    icon: "save",
                    handler: save
                }, {
                    text: "reset",
                    icon: "times",
                    handler: reset
                }, {
                    hidden: true,
                    cancelButton: true,
                    handler: close
                }];

            content = $("<div />").addClass("Columns")

            applySettings(grid.getSettings());

            matematici.popup.show({
                title: "grid settings",
                content: content,
                cssClass: "GridSettings",
                buttons: buttons
            });

            new matematici.DragContainer(content[0], {
                itemSelector: ".Column",
                groupSelector: ".Columns",
                onDrop: function (context) {
                    var
                        item = context.item,
                        target = context.target;

                    if (item && target) {
                        $(item).insertBefore(target);
                        canClose = false;
                    }
                }
            });
        };

    this.show = show;
}