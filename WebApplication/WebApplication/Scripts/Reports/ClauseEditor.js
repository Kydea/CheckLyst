reports.ClauseEditor = function (clauses, columns, clauseIndex, confirmHandler, deleteHandler, verifyHandler, cancelHandler) {
    var
        clause = clauses[clauseIndex],
        newRecord = !clause,
        clauseEditor = this,
        edited = false,
        editor,
        form,

        parameterType,
        parameterTypeSelect,
        clauseTypeLabel,
        clauseTypeSelect,
        filterPanel,
        columnLabel,
        columnSelect,
        signLabel,
        signSelect,
        valueLabel,
        valueTypeInput,
        customLabel,

        toggleControls = function () {
            if (parameterType != parameterTypeSelect.val()) {
                parameterType = parameterTypeSelect.val();
                valueTypeInput && valueTypeInput.destroy();
                if (parameterType == "Prototypes")
                    valueTypeInput = null;
                else
                    valueTypeInput = new matematici.TypedInput(valueLabel[0], parameterType, { maxLength: 50 })
            }

            clauseTypeLabel.toggle(parameterType != "Prototypes");
            columnLabel.toggle(clauseTypeSelect.val() == "filter" || parameterType == "Prototypes");
            signLabel.toggle(clauseTypeSelect.val() == "filter" && parameterType != "Prototypes");
            valueLabel.toggle(clauseTypeSelect.val() != "custom" && signSelect.val() != "0" && signSelect.val() != "!0" && parameterType != "Prototypes");
            customLabel.toggle(clauseTypeSelect.val() == "custom" && parameterType != "Prototypes");

            matematici.popup.center();

            var autoComplete = matematici.AutoComplete.get(columnSelect[0]);
            autoComplete && autoComplete.refresh();
            autoComplete = matematici.AutoComplete.get(signSelect[0]);
            autoComplete && autoComplete.refresh();
        },

        cancel = function () {
            if (!edited) return false;

            matematici.dialog.show("warning", "Warning: the changes to the clause will be lost.", [
                {
                    text: "discard the changes", icon: "exclamation", handler: function () {
                        matematici.popup.close();
                        if (edited) cancelHandler && cancelHandler();
                    }
                },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        map = function () {
            var
                clause = {};

            $("input[type=text][name]:visible", form).each(function () {
                clause[this.name] = this.value;
            });

            $("input[type=checkbox][name]:visible", form).each(function () {
                clause[this.name] = this.checked;
            });

            $("label:visible select[name]", form).each(function () {
                clause[this.name] = this.value;
            });

            clause.clause = {
                column: columnLabel.is(":visible") ? columnSelect.val() : null,
                sign: signLabel.is(":visible") ? signSelect.val() : null,
                value: valueLabel.is(":visible") ? valueTypeInput.getValue() : null,
                custom: customLabel.is(":visible") ? editor.getValue() : null
            };

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

        verify = function () {
            var clause = map();
            if (clause != null) verifyHandler(clauseEditor, clause);
            return true;
        },

        setError = function () {
            var
                errors = reports.reportEditor.getModel().clauseErrors,
                errorRecord = errors && errors["item" + clauseIndex];
            if (errorRecord)
                for (var fieldIndex in errorRecord.fields) {
                    var field = errorRecord.fields[fieldIndex];

                    if (fieldIndex == "custom")
                        editor.setError(0, field.validationMessages[0].message, "error");
                    else {
                        var control = fieldIndex == "column" ? columnSelect : $("input[name=" + fieldIndex + "]", form);
                        matematici.controls.setValidationMessages(control, field.validationMessages);
                    }
                }
        },

        fill = function (f) {
            form = f;
            editor = new matematici.SQLEditor("CustomAceEditor");

            parameterTypeSelect = $("select[name=parameterType]", form).on("change", toggleControls);
            clauseTypeLabel = $("label#ClauseTypeLabel", form);
            clauseTypeSelect = $("select[name=clauseType]", form).on("change", toggleControls);
            filterPanel = $("div#FilterPanel", form);
            columnLabel = $("label#ColumnLabel", form);
            columnSelect = $("select#Column", form).on("change", toggleControls);
            signLabel = $("label#SignLabel", form);
            signSelect = $("select#Sign", form).on("change", toggleControls);
            valueLabel = $("label#ValueLabel", form);
            customLabel = $("label#CustomLabel", form);

            for (var fieldIndex in columns) {
                var column = columns[fieldIndex].split("\"").join("");
                columnSelect.append($("<option />").attr("value", column).text(column));
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

                columnSelect.val(clause.clause.column || "");
                signSelect.val(clause.clause.sign || "");
                editor.setValue(clause.clause.custom || "");
                toggleControls();
                valueTypeInput && valueTypeInput.setValue(clause.clause.value || "");
            } 

            $("input[name=parameterName]", form).keyfilter(/^[$A-Z_0-9]*$/i);
            form
                .on("change", "input, select", function () { edited = true; });

            editor.setChangeHandler(function () { edited = true; })
            editor.addCompleterItems(columns.map(function (column) {
                return { value: column, score: 1000, meta: "report column" };
            }));

            $("input, select", form).each(function () {
                matematici.controls.setValidationMessages($(this), []);
            });

            if (clause) setError();
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });
            buttons.push({ text: "delete", icon: "trash-alt", handler: remove });
            buttons.push({ text: "confirm", icon: "save", handler: confirm });
            buttons.push({ text: "verify", icon: "question", handler: verify });

            matematici.popup.show({
                url: matematici.urls.reports.clauseEditor,
                data: clauseEditor,
                sender: clauseEditor,
                buttons: buttons,
                width: 770,
            });
        };

    this.fill = fill;
    this.setError = setError;

    setup();
};