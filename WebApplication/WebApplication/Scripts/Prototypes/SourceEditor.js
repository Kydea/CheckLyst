prototypes.SourceEditor = function (sources, sourceIndex, confirmHandler, deleteHandler, verifyHandler, cancelHandler) {
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
                    dependentRelations: []
                },
                code = getCode();

            if (!matematici.codeRegularExpression.test(code)) {
                matematici.dialog.error("Invalid code", "The code " + code + " is not valid");
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

        verify = function () {
            var source = map();
            if (source != null) verifyHandler(sourceEditor, source);
            return true;
        },

        setError = function () {
            //TODO
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
                    console.log(source);
                    tableBody.empty();
                    dataSourceParameters = result.parameters;
                    var handlerdParameters = new Map();
                    if (dataSourceParameters && dataSourceParameters.length)
                        for (var parameterIndex in dataSourceParameters) {
                            var dataSourceParameter = dataSourceParameters[parameterIndex];
                            var parameterRow = parameterPrototype.clone();
                            var cells = $("td", parameterRow);
                            cells[0].textContent = dataSourceParameter.label;

                            if (source.parameters && source.parameters[dataSourceParameter.code]) {
                                var parameterRow = source.parameters[dataSourceParameter.code];
                                handlerdParameters.push(dataSourceParameter.code);
                            }
                            tableBody.append(parameterRow);
                        }
                    else
                        tableBody.append(noRecord);

                    if (source.parameters)
                        for (var parameterIndex in source.parameters) {
                            var parameter = source.parameters[paramter];
                            if (!handlerdParameters.has(parameter.code)) {
                                var parameter = parameterPrototype.clone();
                                var cells = $("td", parameterRow);
                                cells.eq(0).text(parameter.code).addClass("Error");
                                cells.eq(1).empty();
                                cells.eq(2).empty();
                                cells.eq(3).empty();
                                cells.eq(4).empty();
                            }
                        }

                    /*
                    if (column)
                        sourceColumn.val(column);
                    else
                        sourceColumn[0].selectedIndex = 0;


                    for (index in columns)
                        $("<option />").appendTo(sourceColumn).text(columns[index]).attr("value", columns[index]);
                    var autoComplete = new matematici.AutoComplete(select);
                    autoComplete.initialize();
                    */
                });
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
                form.on("change", "input, select", function () { edited = true; });
            } else
                $("input, select", form).each(function () {
                    matematici.controls.setEditable($(this), false);
                });

            var rows = $("tr", tableBody);
            parameterPrototype = rows.eq(0).remove();
            noRecord = rows.eq(1).remove();

            setupParameters();

            if (source) setError();
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true, handler: cancel });

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft) {
                buttons.push({ text: "verify", icon: "question", handler: verify });
                if (!!deleteHandler)
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
    this.setError = setError;

    setup();
};