reports.ColumnEditor = function (columns, queryColumns, columnIndex, confirmHandler, deleteHandler, cancelHandler) {
    var
        column = columns[columnIndex],
        newRecord = !column,
        columnEditor = this,
        edited = false,
        editor,
        form,

        toggleNotification = function () {
            var
                notificationCheck = $("input[name=notification]", form),
                notifictatonOn = notificationCheck.prop("checked");

            matematici.controls.setEditable($("input[name=notificationEmail]", form), notifictatonOn);
            matematici.controls.setEditable($("input[name=notificationNote]", form), notifictatonOn);
        },

        cancel = function () {
            if (!edited) {
                if (cancelHandler) {
                    cancelHandler();
                    return true;
                }
                else
                    return false;
            }

            matematici.dialog.show("warning", "Warning: the changes to the column will be lost.", [
                { text: "discard the changes", icon: "exclamation", handler: cancelHandler || matematici.popup.close },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        map = function () {
            var
                column = {};

            $("input[type=text]", form).each(function () {
                column[this.name] = this.value;
            });

            $("input[type=checkbox]", form).each(function () {
                column[this.name] = this.checked;
            });

            $("select", form).each(function () {
                column[this.name] = this.value;
            });

            column.column = editor.getValue();

            return column;
        },

        confirm = function () {
            if (!edited) return false;
            var column = map();
            if (column == null) return true;
            confirmHandler(column);
        },

        remove = function () {
            if (!newRecord) deleteHandler();
        },

        fill = function (f) {
            form = f;
            editor = new matematici.SQLEditor("ColumnAceEditor");

            if (!newRecord) {
                editor.setValue(column.column);

                $("input[type=text]", form).each(function () {
                    this.value = column[this.name];
                });

                $("input[type=checkbox]", form).each(function () {
                    this.checked = this.value = column[this.name];
                });

                $("select", form).each(function () {
                    this.value = column[this.name];
                });
            } else
                $("select", form).each(function () {
                    this.value = null;
                });

            $("input[name=code]", form).keyfilter(/^[$A-Z_0-9]*$/i);
            form
                .on("change", "input, select", function () { edited = true; })
                .on("change", "input[name=notification]", toggleNotification);

            editor.setChangeHandler(function () { edited = true; })
            editor.addCompleterItems(queryColumns.map(function (column) {
                return { value: column, score: 1000, meta: "query column" };
            }));
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });
            buttons.push({ text: "delete", icon: "trash-alt", handler: remove });
            buttons.push({ text: "confirm", icon: "save", handler: confirm });

            matematici.popup.show({
                url: matematici.urls.reports.columnEditor,
                data: columnEditor,
                sender: columnEditor,
                buttons: buttons,
                width: 770,
            });
        };

    this.fill = fill;

    setup();
};