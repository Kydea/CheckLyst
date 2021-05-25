reports.ClauseEditor = function (clauses, columns, clauseIndex, confirmHandler, deleteHandler, cancelHandler) {
    var
        clause = clauses[clauseIndex],
        newRecord = !clause,
        clauseEditor = this,
        edited = false,
        editor,
        form,

        parameterType,
        parameterTypeSelect,
        clauseTypeSelect,
        filterPanel,
        columnLabel,
        columnSelect,
        signLabel,
        signSelect,
        valueLabel,
        valueTypeInput,
        clauseLabel,

        toggleNotification = function () {
            var
                notificationCheck = $("input[name=notification]", form),
                notifictatonOn = notificationCheck.prop("checked");

            matematici.controls.setEditable($("input[name=notificationEmail]", form), notifictatonOn);
            matematici.controls.setEditable($("input[name=notificationNote]", form), notifictatonOn);
        },

        toggleControls = function () {
            if (parameterType != parameterTypeSelect.val()) {
                parameterType = parameterTypeSelect.val();
                valueTypeInput && valueTypeInput.destroy();
                valueTypeInput = new matematici.TypedInput(valueLabel[0], parameterType, { maxLength: 50 })
            }

            columnLabel.toggle(clauseTypeSelect.val() == "filter");
            signLabel.toggle(clauseTypeSelect.val() == "filter");
            valueLabel.toggle(clauseTypeSelect.val() != "custom" && signSelect.val() != "0" && signSelect.val() != "!0");
            clauseLabel.toggle(clauseTypeSelect.val() == "custom");
            matematici.popup.center();
            var autoComplete = matematici.AutoComplete.get(columnSelect[0]);
            autoComplete && autoComplete.refresh();
            autoComplete = matematici.AutoComplete.get(signSelect[0]);
            autoComplete && autoComplete.refresh();
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

            clause.clause = JSON.stringify({
                column: columnSelect.val(),
                sign: signSelect.val(),
                value: valueTypeInput.getValue(),
                content: editor.getValue()
            });

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

            parameterTypeSelect = $("select[name=parameterType]", form).on("change", toggleControls);
            clauseTypeSelect = $("select[name=clauseType]", form).on("change", toggleControls);
            filterPanel = $("div#FilterPanel", form);
            columnLabel = $("label#ColumnLabel", form);
            columnSelect = $("select#Column", form).on("change", toggleControls);
            signLabel = $("label#SignLabel", form);
            signSelect = $("select#Sign", form).on("change", toggleControls);
            valueLabel = $("label#ValueLabel", form);
            clauseLabel = $("label#ClauseLabel", form);

            for (var index in columns) {
                var column = columns[index];
                columnSelect.append($("<option />").attr("value", column).text(column.split("\"").join("")));
            }

            if (newRecord)
                toggleControls();
            else {
                $("input[type=text]", form).each(function () {
                    this.value = clause[this.name];
                });

                $("input[type=checkbox]", form).each(function () {
                    this.checked = this.value = clause[this.name];
                });

                $("select", form).each(function () {
                    this.value = clause[this.name];
                });

                var json = JSON.parse(clause.clause);
                columnSelect.val(json.column || "");
                signSelect.val(json.sign || "");
                editor.setValue(json.content || "");
                toggleControls();
                valueTypeInput.setValue(json.value || "");
            } 

            $("input[name=parameterName]", form).keyfilter(/^[$A-Z_0-9]*$/i);
            form
                .on("change", "input, select", function () { edited = true; });

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