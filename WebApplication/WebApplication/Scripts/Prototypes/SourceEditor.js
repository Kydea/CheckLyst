prototypes.SourceEditor = function (sources, sourceIndex, confirmHandler, deleteHandler, cancelHandler) {
    var
        source = sources[sourceIndex],
        newRecord = !source,
        sourceEditor = this,
        edited = false,
        editor,
        form,
        tableBody,
        dataSourceSelect,
        parameterPrototype,
        dataSourceParameters = [],
        noRecord,

        getCode = function () {
            return $("input[name=code]", form).val()
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

            matematici.dialog.show("warning", "Warning: the changes to the source will be lost.", [
                { text: "discard the changes", icon: "exclamation", handler: cancelHandler || matematici.popup.close },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        map = function () {
            var
                source = {
                    dependingRelations: [],
                    dependentRelations: [],
                    parameters: {}
                },
                code = getCode();

            if (!matematici.codeRegularExpression.test(code)) {
                matematici.dialog.error("Invalid code", "The code " + code + " is not valid");
                return null;
            }

            if (!$("select[name=dataSource]", form).val()) {
                matematici.dialog.error("error", "no source selected");
                return null;
            }

            for (var index in sources)
                if (index != sourceIndex && code == sources[index].code) {
                    matematici.dialog.error("Duplicated code", "The code " + code + " is duplicated");
                    return null;
                }

            $("div.Fields input[type=text]", form).each(function () {
                source[this.name] = this.value;
            });

            $("div.Fields input[type=checkbox]", form).each(function () {
                source[this.name] = this.checked;
            });

            $("div.Fields select", form).each(function () {
                source[this.name] = this.value;
            });

            $("tr.Parameter", tableBody).each(function () {
                var
                    tr = $(this),
                    parameterName = tr.data("name"),
                    bindingType = $("select[name=binding]", tr).val(),
                    bindingCode = $("select[name=code]", tr).val(),
                    hidden = $("input[name=hidden]", tr).prop("checked"),
                    readOnly = $("input[name=readOnly]", tr).prop("checked"),
                    parameter = bindingType == null && !hidden && !readOnly ? null : {
                        name: parameterName,
                        expressionCode: bindingType == "expression" ? bindingCode : null,
                        elementCode: bindingType == "field" ? bindingCode : null,
                        hidden: hidden,
                        readOnly: readOnly
                    };

                if (parameter) source.parameters[parameterName] = parameter;
            });

            return source;
        },

        confirm = function () {
            if (!edited) return false;

            var source = map();

            if (source == null) return true;

            if (!newRecord && sources[sourceIndex].dependentRelations && sources[sourceIndex].dependentRelations.length > 0 && getCode() != sources[sourceIndex].code) {
                var message = "WARNING: the source is referenced by " + sources[sourceIndex].dependentRelations.length + " element(s). If you change the code from '" + sources[sourceIndex].code + "', the referencing elements will have to be fixed."
                matematici.dialog.show("warning", message, [
                    {
                        text: "confirm", icon: "check", handler: function () {
                            confirmHandler(source);
                            matematici.popup.close();
                        }
                    },
                    { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
                ]);
                return true;
            } else
                confirmHandler(source);
        },

        remove = function () {
            var message = newRecord || source.dependentRelations && source.dependentRelations.length == 0 ?
                "Warning: the source will be deleted." :
                "WARNING: the source is referenced by " + source.dependentRelations.length + " element(s). After the deletion of this source, the referencing elements will have to be fixed to save the proptotype.";
            matematici.dialog.show("warning", message, [
                {
                    text: "confirm", icon: "check", handler: function () {
                        if (!newRecord) deleteHandler();
                        matematici.popup.close();
                    }
                },
                { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
            ]);
            return true;
        },

        setupParameters = function () {
            if (dataSourceSelect.val())
                $.ajax({
                    type: "post",
                    url: matematici.urls.dataSources.api + "/Data/GetDataSource",
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        code: dataSourceSelect.val()
                    })
                }).done(function (result) {
                    tableBody.empty();
                    dataSourceParameters = result.parameters;
                    var handledParameters = new Set();
                    if (dataSourceParameters && dataSourceParameters.length)
                        for (var parameterIndex in dataSourceParameters) {
                            var
                                dataSourceParameter = dataSourceParameters[parameterIndex],
                                parameterRow = parameterPrototype.clone().appendTo(tableBody).data("name", dataSourceParameter.name),
                                cells = $("td", parameterRow),
                                bindingSelect = cells.eq(1).children("select")[0],
                                bindingAutoComplete = new matematici.AutoComplete(bindingSelect),
                                codeSelect = cells.eq(2).children("select")[0],
                                codeAutoComplete = new matematici.AutoComplete(codeSelect),
                                parameter = source && source.parameters && source.parameters[dataSourceParameter.name] && source.parameters[dataSourceParameter.name];

                            cells.eq(0).text(dataSourceParameter.name);
                            bindingAutoComplete.initialize();
                            bindingAutoComplete.setValue(parameter && parameter.expressionCode ? "expression" : parameter && parameter.elementCode ? "field" : null);
                            codeAutoComplete.initialize();
                            bindingChanged.call(bindingSelect)

                            if (parameter) {
                                handledParameters.add(dataSourceParameter.name);
                                var code = parameter.expressionCode || parameter.elementCode;
                                codeAutoComplete.setValue(code);
                                if (!codeAutoComplete.getValue()) codeAutoComplete.setError(code);
                                $("input", cells.eq(3)).prop("checked", parameter.hidden);
                                $("input", cells.eq(4)).prop("checked", parameter.readOnly);
                            } else
                                codeAutoComplete.setValue(null);
                        }
                    else
                        tableBody.append(noRecord);

                    if (source && source.parameters)
                        for (var parameterIndex in source.parameters) {
                            var parameter = source.parameters[parameterIndex];
                            if (!handledParameters.has(parameter.name)) {
                                var
                                    parameterRow = parameterPrototype.clone().appendTo(tableBody),
                                    cells = $("td", parameterRow);
                                cells.eq(0).text(parameter.name).addClass("Red");
                                cells.eq(1).empty();
                                cells.eq(2).empty();
                                cells.eq(3).empty();
                                cells.eq(4).empty();
                            }
                        }
                });
            else
                tableBody.empty().append(noRecord);
        },

        bindingChanged = function () {
            var
                value = this.value,
                codes = [],
                codeSelect = $("select", this.parentElement.nextElementSibling),
                autoComplete = matematici.AutoComplete.get(codeSelect[0]);

            matematici.controls.setEditable(codeSelect, !!value);

            codeSelect.empty();
            if (value == "expression") {
                var expressions = prototypes.prototype.getModel().expressions;
                for (var expressionIndex in expressions)
                    codes.push(expressions[expressionIndex].code);
            } else if (value == "field") {
                var fields = prototypes.prototype.getElements("Field");
                for (var fieldIndex in fields)
                    codes.push(fieldIndex);
            }

            if (value)
                for (var index in codes) {
                    var option = document.createElement("option");
                    option.setAttribute("value", codes[index]);
                    option.textContent = codes[index];
                    codeSelect.append(option);
                }

            autoComplete.refreshValues();
            autoComplete.checkReadOnly();
        },

        fill = function (f) {
            form = f;

            dataSourceSelect = $("select[name=dataSource]", form).on("change", setupParameters);
            var dataSources = prototypes.prototype.getDataSources();
            for (index in dataSources)
                $("<option />").appendTo(dataSourceSelect).text(dataSources[index].code).val(dataSources[index].code);

            tableBody = $("table tbody", form);

            if (!newRecord) {
                $("div.Fields input[type=text]", form).each(function () {
                    this.value = source[this.name];
                });

                $("div.Fields input[type=checkbox]", form).each(function () {
                    this.checked = this.value = source[this.name];
                });

                $("div.Fields select", form).each(function () {
                    this.value = source[this.name];
                });
            }

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft) {
                $("input[name=code]", form).keyfilter(/^[$A-Z_0-9]*$/i);
                form
                    .on("change", "input, select", function () { edited = true; })
                    .on("change", "select[name=binding]", bindingChanged);
            } else
                $("input, select", form).each(function () {
                    matematici.controls.setEditable($(this), false);
                });

            var rows = $("tr", tableBody);
            parameterPrototype = rows.eq(0).remove();
            noRecord = rows.eq(1).remove();

            if (!source)
                tableBody.append(noRecord);

            setTimeout(function () {
                var autoComplete = matematici.AutoComplete.get(dataSourceSelect[0]);
                if (source) {
                    autoComplete.setValue(source.dataSource);
                    setupParameters();
                } else
                    autoComplete.setValue(null);
            }, 0);
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft) {
                buttons.push({ text: "delete", icon: "trash-alt", handler: remove });
                buttons.push({ text: "confirm", icon: "save", handler: confirm });
            }

            matematici.popup.show({
                url: matematici.urls.prototypes.sourceEditor,
                data: sourceEditor,
                sender: sourceEditor,
                buttons: buttons,
                width: 770,
            });
        };

    this.fill = fill;

    setup();
};
