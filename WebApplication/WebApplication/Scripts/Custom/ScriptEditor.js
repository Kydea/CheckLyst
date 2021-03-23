matematici.ScriptEditor = function (form) {
    var
        options = {},
        scriptEditor = this,
        editor,
        editable = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft,
        exclusions = ["dependentRelations", "dependingRelations", "error", "errors", "removed", "pageIndex", "elements"],
        flags,

        getFlagValues = function () {
            return flags.map(function () {
                return this.checked;
            });
        },

        mapPrototype = function (source, destination) {
            destination.description = source.description;
            destination.descriptionFormulaString = source.descriptionFormulaString;
            destination.fileNameFormulaString = source.fileNameFormulaString;
            destination.ftpExport = source.ftpExport;
            destination.instanceConfirmationWorkFlow = source.instanceConfirmationWorkFlow;
            destination.itemIDSequence = source.itemIDSequence;
            destination.typeName = source.typeName;
        },

        map = function (source, destination, customExclusions) {
            var orderedProperties = Object.keys(source);
            orderedProperties.sort();
            for (var index in orderedProperties) {
                var property = orderedProperties[index];
                if (!!property && !customExclusions.includes(property) && !exclusions.includes(property))
                    destination[property] = source[property];
            }
        },

        traverseForImport = function (element) {
            element.id = prototypes.prototype.getNewItemsId();
            if (element.typeName == "Page")
                for (var index in element.groups)
                    traverseForImport(element.groups[index]);
            else if (element.typeName == "Group")
                for (var index in element.items)
                    traverseForImport(element.items[index]);
            else if (element.typeName == "Image")
                element.cloned = true;
        },

        traverseForExport = function (source, destination) {
            if (source.typeName == "Prototype") {
                var flagValues = getFlagValues();
                if (flagValues[1]) {
                    destination.expressions = {};
                    for (var index in source.expressions) {
                        destination.expressions[index] = {};
                        map(source.expressions[index], destination.expressions[index], ["value", "dataType", "innerDataType", "errorPosition"]);
                    }
                }

                if (flagValues[2]) {
                    destination.externalSources = {};
                    for (var index in source.externalSources)
                        destination.externalSources[index] = source.externalSources[index];
                }

                if (flagValues[0]) {
                    if (!source.header) throw new Error("key header not found");
                    if (!source.pages) throw new Error("key pages not found");
                    destination.header = {};
                    traverseForExport(source.header, destination.header);
                    destination.pages = [];
                    for (var index in source.pages) {
                        destination.pages[index] = {};
                        traverseForExport(source.pages[index], destination.pages[index]);
                    }

                    mapPrototype(source, destination);
                }
            } else if (source.typeName == "Page") {
                destination.groups = [];
                for (var index in source.groups) {
                    destination.groups[index] = {};
                    traverseForExport(source.groups[index], destination.groups[index]);
                }
                map(source, destination, ["groups"]);
            } else if (source.typeName == "Group") {
                destination.items = [];
                for (var index in source.items) {
                    destination.items[index] = {};
                    traverseForExport(source.items[index], destination.items[index]);
                }
                map(source, destination, ["items", "iterations"]);
            } else
                map(source, destination, []);
        },

        cancel = function () {
            options.cancelHandler && options.cancelHandler();
        },

        prepare = function () {
            flags = $("input[type=checkbox]", form);

            editor = ace.edit("AceExpressionEditor");
            editor.session.setMode("ace/mode/json");
            editor.setTheme("ace/theme/custom")
            editor.setShowPrintMargin(false);
            editor.setReadOnly(!editable);
            editor.on("input", function () {
                if (editable) $("a.Button.Save", matematici.popup.getButtonsBar()).toggle(!editor.session.getUndoManager().isClean());
            });
        },

        load = function () {
            var handler = function () {
                var model = {};
                traverseForExport(prototypes.prototype.getModel(), model);
                editor.setValue(JSON.stringify(model, null, '\t'), -1);
                editor.session.getUndoManager().markClean();
                setTimeout(function () {
                    editor.focus();
                }, 0);
            };

            if (editor.session.getValue() == "")
                handler();
            else
                matematici.dialog.show({
                    title: "Script editor",
                    content: "Attention, the json will be replaced with the model of the prototype",
                    buttons: [{
                        icon: "check",
                        text: "ok",
                        handler: handler
                    }]
                });
            return true;
        },

        save = function () {
            if (editor.getSession().getAnnotations().length > 0)
                matematici.dialog.error({
                    title: "Script editor",
                    content: "The model has errors",
                });
            else {
                var
                    flagValues = getFlagValues(),
                    messages = [];
                if (flagValues[0]) messages.push("the items and properties");
                if (flagValues[1]) messages.push("the expressions");
                if (flagValues[2]) messages.push("the datasources");

                matematici.dialog.show({
                    title: "Script editor",
                    content: "Attention, " + messages.join(", ") + " of the current prototype will be replaced with the json model provided",
                    buttons: [{
                        icon: "check",
                        text: "ok",
                        handler: function () {
                            try {
                                var
                                    oldModel = prototypes.prototype.getModel(),
                                    newModel = JSON.parse(editor.getValue());

                                if (flagValues[1])
                                    for (var code in newModel.expressions) {
                                        //for prototypes from versions < 2.2
                                        var expression = newModel.expressions[code];
                                        expression.code = code;
                                        expression.unique = expression.unique >= "2" ? expression.unique : "0";
                                    }
                                else
                                    newModel.expressions = oldModel.expressions;

                                if (!flagValues[2])
                                    newModel.externalSources = oldModel.externalSources;

                                if (flagValues[0]) {
                                    traverseForImport(newModel.header);
                                    for (var page in newModel.pages) {
                                        newModel.pages[page].typeName = "Page";
                                        traverseForImport(newModel.pages[page]);
                                    }
                                } else {
                                    newModel.header = oldModel.header;
                                    newModel.pages = oldModel.pages;
                                    mapPrototype(oldModel, newModel);
                                }

                                prototypes.prototype.setModel(newModel);
                                matematici.popup.close();
                            } catch (error) {
                                console.log(error);
                                setTimeout(function () {
                                    matematici.dialog.error("Error", error);
                                }, 0);
                            }
                        }
                    }, {
                        icon: "times",
                        text: "cancel",
                        cancelButton: true
                    }]
                });
            }

            options.okHandler && options.okHandler();
            return true;
        },

        copy = function () {
            var selection = editor.selection.toJSON();
            editor.selectAll();
            editor.focus();
            document.execCommand('copy');
            editor.selection.fromJSON(selection);
            matematici.blip.ok();
            return true;
        },

        setup = function () {
            var buttons = [
                { cancelButton: true, hidden: true, handler: cancel },
                { text: "copy", icon: "copy", handler: copy }
            ];

            if (editable)
                buttons.push(
                    { text: "update prototype", icon: "sign-in", handler: save, hidden: true, cssClass: "Save" },
                    { text: "generate json", icon: "sign-out", handler: load, cssClass: "Load" }
                );

            matematici.popup.show({
                url: matematici.urls.prototypes.scriptEditor,
                data: scriptEditor,
                sender: scriptEditor,
                buttons: buttons,
                width: "100%",
            });
        };

    this.prepare = prepare;

    setup();
}
