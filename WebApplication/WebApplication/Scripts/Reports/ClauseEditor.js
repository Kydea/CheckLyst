reports.ClauseEditor = function (clauses, columns, clauseIndex, confirmHandler, deleteHandler, cancelHandler) {
    var
        clause = clauses[clauseIndex],
        newRecord = !clause,
        clauseEditor = this,
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

            matematici.dialog.show("warning", "Warning: the changes to the clause will be lost.", [
                { text: "discard the changes", icon: "exclamation", handler: cancelHandler || matematici.popup.close },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        map = function () {
            var
                clause = {};

            $("input[type=text]", form).each(function () {
                clause[this.name] = this.value;
            });

            $("input[type=checkbox]", form).each(function () {
                clause[this.name] = this.checked;
            });

            $("select", form).each(function () {
                clause[this.name] = this.value;
            });

            clause.clause = editor.getValue();

            return clause;
        },

        confirm = function () {
            if (!edited) return false;
            var clause = map();
            if (clause == null) return true;
            confirmHandler(clause);
        },

        remove = function () {
            if (!newRecord) deleteHandler();
        },

        fill = function (f) {
            form = f;
            editor = new matematici.SQLEditor("ClauseAceEditor");

            if (!newRecord) {
                editor.setValue(clause.clause);

                $("input[type=text]", form).each(function () {
                    this.value = clause[this.name];
                });

                $("input[type=checkbox]", form).each(function () {
                    this.checked = this.value = clause[this.name];
                });

                $("select", form).each(function () {
                    this.value = clause[this.name];
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
            editor.addCompleterItems(columns.map(function (column) {
                return { value: column, score: 1000, meta: "report column" };
            }));
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });
            buttons.push({ text: "delete", icon: "trash-alt", handler: remove });
            buttons.push({ text: "confirm", icon: "save", handler: confirm });

            matematici.popup.show({
                url: matematici.urls.reports.clauseEditor,
                data: clauseEditor,
                sender: clauseEditor,
                buttons: buttons,
                width: 770,
            });
        };

    this.fill = fill;

    setup();
};