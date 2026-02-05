prototypes.PropertyPanel = function (selector) {
    var
        propertyPanel = this,
        element = $(selector),
        border = element.children("div.Border"),
        noItem = element.children("div.NoItem"),
        propertyList = $("tbody", element.children("div.PropertyList")),
        propertyValue = element.children("div.PropertyValue"),
        information = element.children("div.Information"),
        currentProperty,
        prototypeTypesList,
        properties,
        item,
        model,
        oldModel,
        options,

        copyModel = function (source, destination) {
            Object.keys(destination).forEach(k => delete destination[k]);
            Object.assign(destination, source);
            //for (var property in source)
            //    destination[property] = source[property];
            if (source.errors) {
                destination.errors = [];
                for (var error in source.errors)
                    destination.errors.push(source.errors[error]);
            }
        },

        confirm = function () {
            if (model.typeName == "Image") {
                if (oldModel.key && !oldModel.cloned && !model.key)
                    prototypes.prototype.deleteImage(oldModel.key);
                if (!oldModel.key && model.key) //in case the image was previously enqueued to be deleted...
                    prototypes.prototype.ensureImageWillNotBeDeleted(model.key);
            }
            $("input:not(.ReadOnly)", propertyValue).trigger("change");
            currentProperty && currentProperty.getType().destroy && currentProperty.getType().destroy();

            if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft)
                prototypes.prototype.verify(options && options.okHandler);
            else
                prototypes.prototype.setupActionBar();
        };

    propertyPanel.confirm = confirm;

    propertyPanel.select = function (value, o) {
        if (!!value) item = value;

        var state = prototypes.prototype.getModel().state;
        model = item.getModel();
        oldModel = {};
        options = o;

        copyModel(model, oldModel);

        var
            name = model.typeName,
            buttons = [];

        if (prototypes.prototype.getModel().state == prototypes.Prototype.states.draft || model.typeName == "Prototype")
            buttons.push({
                icon: "check",
                text: "apply",
                handler: confirm
            });
        
        buttons.push({
            cancelButton: true,
            hidden: true,
            handler: function () {
                if (model.typeName == "Image" && !oldModel.key && model.key)
                    prototypes.prototype.deleteImage(model.key);
                copyModel(oldModel, model);
                prototypes.prototype.update();
            }
        });

        matematici.popup.show({
            title: "Properties - " + name + (model.index != null && " - Index: " + (parseInt(model.index) + 1) || model.id != null && " - ID: " + model.id || "") + (model.code && " - Code: " + model.code || ""),
            content: element,
            buttons: buttons
        });

        matematici.popup.setWidth("760px");

        prototypes.prototype.setItemHandler(null);
        propertyList.empty();
        propertyValue.empty();
        information.empty();
        properties = [];

        if (model.typeName == "Prototype") {
            var editRole = model.version == null || matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "##") || matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "#" + model.prototypeType + "#");
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "name", {
                    readOnly: state != prototypes.Prototype.states.draft || prototypes.prototype.getPrototypeKey().version > 0 || prototypes.prototype.getPrototypeKey().id > 0,
                    code: true
                }).setCheck([{
                    action: function (value) {
                        return value == "" || matematici.codeRegularExpression.test(value);
                    },
                    message: "Invalid name"
                }]), "Name", "The name of the prototype"
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "version", { readOnly: true }), "Version",
                "The version number."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                "Description", "A short description of the prototype.",
                function () { $("h2 span", item.getElement()).html(model.description); }
            ));
            var lifeCycleProperty;
            properties.push(lifeCycleProperty = new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "lifeCycle", { number: true, maxLength: 4, readOnly: !editRole }).setCheck([
                    {
                        action: function (value) {
                            var intValue = parseInt(value);
                            return (value || "").trim() == "" || intValue > 0 && value == intValue;
                        },
                        message: "invalid value"
                    }
                ]), "Life cycle",
                "The number of days of the life cycle."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "descriptionFormulaString", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                "Instance description", "A formula to use for the description of the instances of this prototype."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "fileNameFormulaString", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                "Instance file name", "A formula to use for the file name of the pdf generated for confirmed instances of this prototype."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.WorkFlowProperty(model, "instanceConfirmationWorkFlow", { readOnly: state != prototypes.Prototype.states.draft }),
                "Instance confirmation workflow", "The confirmation workflow of the instance of this prototype. Leave empty to have the instances confirmed without a confirmation workflow."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "state", { readOnly: true, defaultValue: prototypes.Prototype.states.draft }), "State",
                "The state of the prototype. This property is not editable. Use the action <strong>buttons</strong> to change the state of the version."
            ));
            properties.push(new prototypes.Property(this, new prototypes.PrototypeTypeProperty(model, {
                readOnly: state != prototypes.Prototype.states.draft || prototypes.prototype.getPrototypeKey().version > 0 || prototypes.prototype.getPrototypeKey().id > 0,
                lifeCycleProperty: lifeCycleProperty
            }), "Prototype Type", "Department and Prototype type of the prototype."));
            properties.push(new prototypes.Property(
                this, new prototypes.DateProperty(model, "validFrom", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([{
                    action: function (value) {
                        return !value || value > matematici.dateFormatter.date(new moment());
                    },
                    message: "Specify a future date or leave empty"
                }, {
                    action: function (value) {
                        return !model.validUntil || !value || model.validUntil.substring(0, 10) > value;
                    },
                    message: "Validity end must be greater than validity start"
                }]), "Validity start",
                "If specified, in this date, the version gets the state <b>active</b>, if the state of the version is <b>approved</b>. Otherwise the version get the state <b>active</b> immediately when approved."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.DateProperty(model, "validUntil", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([{
                    action: function (value) {
                        return !value || value > matematici.dateFormatter.date(new moment());
                    },
                    message: "Specify a future date or leave empty"
                }, {
                    action: function (value) {
                        return !model.validFrom || !value || model.validFrom.substring(0, 10) < value;
                    },
                    message: "Validity end must be greater than validity start"
                }]), "Validity end",
                "If specified, in this date, the version gets the state <b>obsolete</b>, if the state of the version is <b>active</b>."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.BooleanProperty(model, "ftpExport"),
                "FTP Export", "When checked, the instances are exported to the FTP server, once confirmed."
            ));

        } else {
            var
                parent = item.getParent(),
                siblings = !!parent && parent.getChildren(),
                itemIndex = item.getIndex(),
                isHeader = model.typeName == "Group" && parent.getModel().header === model;

            if (!isHeader && model.typeName != "Note")
                properties.push(new prototypes.Property(
                    this, new prototypes.BooleanProperty(model, "pageBreak", false, model.typeName == "Page"), "Page break",
                    "Forces a page break in the pdf version of the checklist before the element."
                ));

            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "code", {
                    readOnly: state != prototypes.Prototype.states.draft,
                    code: true
                }).setCheck([{
                    action: function (value) {
                        return value == "" || matematici.codeRegularExpression.test(value);
                    },
                    message: "Invalid code"
                }]), "Code",
                "Uniquely identifies an element. Can be left empty. Use only if a reference to the element is needed.",
                function (value) {
                    item.getElement().data("code", model.code);
                    prototypes.prototype.completeModel();
                    if (!model.code && model.dumpExternalSourceName) model.dumpExternalSourceName = null;
                }
            ));

            if (model.typeName != "Text" && model.typeName != "Link" && model.typeName != "Image" && model.typeName != "Note") {
                properties.push(new prototypes.Property(
                    this, new prototypes.ExpressionProperty(model, "readOnly", "read-only", true, true, 0), "Read-only",
                    "Determines if the element is editable by the operator or not."
                ));
                properties.push(new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "userGroup", { readOnly: state != prototypes.Prototype.states.draft }), "User group",
                    "lockes the edit only to users logged to this group."
                ));
            }

            if (model.typeName != "Note") {
                properties.push(new prototypes.Property(
                    this, new prototypes.ExpressionProperty(model, "hidden", "hidden", false, false, 2), "Hidden",
                    "Determines if the element is visible or not."
                ));
                properties.push(new prototypes.Property(
                    this, new prototypes.CountDownToProperty(model, "countDownToCode"), "Countdown to",
                    "Shows the countdown until the date and time are reached."
                ));
            }

            if (model.typeName != "Link" && model.typeName != "Text" && model.typeName != "Image" && model.typeName != "Note") {
                properties.push(new prototypes.Property(
                    this, new prototypes.ExpressionProperty(model, "valid", "validity", true, true, 0), "Validity",
                    "Sets the condition under which the element is considered valid. By default pages and groups are valid if all the contained elements are valid, fields and attachments are valid if not empty, signature are valid if signed. Text are always valid. Attachments are valid if contains a file."
                ));
                properties.push(new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "validityMessage", { readOnly: state != prototypes.Prototype.states.draft }), "Validity message",
                    "The error message to display to the user when the item is not valid to help him solve the problem."
                ));
                properties.push(new prototypes.Property(
                    this, new prototypes.SignatureProperty(model, { readOnly: state != prototypes.Prototype.states.draft }), "Signature",
                    "Determines which signature locks the element. For iterators with a code, it is present alse a Source on which a data export is performed when the signature is issued."
                ));
            }

            if (model.typeName != "Page" && model.typeName != "Note") {
                properties.push(new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "fontSize", { number: true, readOnly: state != prototypes.Prototype.states.draft }).setCheck([
                        {
                            action: function (value) {
                                var intValue = parseInt(value);
                                return (value || "").trim() == "" || intValue >= 0 && value == intValue;
                            },
                            message: "invalid value"
                        }
                    ]), "Font size",
                    "The size in pixel of the text (empty or 0 for the default font size).",
                    function () { item.setFontSize(model.fontSize); }
                ));
                properties.push(new prototypes.Property(
                    this, new prototypes.ColorProperty(model, "color"),
                    "Color",
                    "The color of the text.",
                    function () { item.setColor(model.color); }
                ));
            }

            switch (model.typeName) {
                case "Page":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Caption",
                        "The caption of the page.",
                        function () { $("div.TabLabels a.Selected", prototypes.prototype.getTab().getElement()).html(model.description); }
                    ));
                    break;
                case "Group":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                        "Caption",
                        "The caption of the group.",
                        function () { item.getElement().children("h3").html(model.description); }
                    ));
                    var tableContent = false;
                    for (var node = parent; node.getModel().typeName == "Group"; node = node.getParent())
                        if (node.getModel().showAsTable) {
                            tableContent = true;
                            break;
                        }

                    if (!tableContent && !isHeader) {
                        var
                            instanceIterationsNumber, prototypeIterationsNumber, showAsTable, addIteration, removeIteration, moveIteration, directImport,

                            toggleIteratorControls = function () {
                                var itaratorCommandsEnabled = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft && model.iterator;
                                showAsTable.getType().setReadOnly(!itaratorCommandsEnabled);
                                instanceIterationsNumber.getType().setReadOnly(!itaratorCommandsEnabled);
                                prototypeIterationsNumber.getType().setReadOnly(!itaratorCommandsEnabled);
                                addIteration.getType().setReadOnly(!itaratorCommandsEnabled);
                                removeIteration.getType().setReadOnly(!itaratorCommandsEnabled);
                                moveIteration.getType().setReadOnly(!itaratorCommandsEnabled);
                                directImport.getType().setReadOnly(!itaratorCommandsEnabled);

                                if (!model.iterator) {
                                    model.instanceIterationsNumber = 0;
                                    model.prototypeIterationsNumber = 0;

                                    model.showAsTable = false;
                                    model.addIterationCondition = 1;
                                    model.removeIterationCondition = 1;
                                    model.moveIterationCondition = 1
                                    model.directImportCondition = 2;
                                    model.dumpExternalSourceName = null;

                                    instanceIterationsNumber.setValue();
                                    prototypeIterationsNumber.setValue();
                                    showAsTable.setValue();
                                }

                                addIteration.setValue();
                                removeIteration.setValue();
                                moveIteration.setValue();
                                directImport.setValue();
                            };

                        properties.push(new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "iterator"), "Iterator",
                            "Determines if the group is an iterator.", toggleIteratorControls
                        ));

                        showAsTable = new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "showAsTable", !model.iterator), "Show as table",
                            "Displays the group as a table."
                        );
                        properties.push(showAsTable);

                        instanceIterationsNumber = new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "instanceIterations", { number: true, readOnly: state != prototypes.Prototype.states.draft || !model.iterator }), "Instance iterations",
                            "Creates the specified number of iterations automatically for new instances."
                        );
                        properties.push(instanceIterationsNumber);

                        prototypeIterationsNumber = new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "prototypeIterations", { number: true, readOnly: state != prototypes.Prototype.states.draft || !model.iterator }), "Prototype iterations",
                            "Creates the specified number of iterations automatically int the PDF version."
                        );
                        properties.push(prototypeIterationsNumber);

                        addIteration = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "addIteration", "add-iteration", false, false, 1), "Add iteration",
                            "Defines if the buttons add and import of the iterator are present."
                        ).setValueParser(function (node) {
                            if (!model.iterator) {
                                node.text("");
                                return true;
                            }
                        });
                        properties.push(addIteration);

                        removeIteration = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "removeIteration", "remove-iteration", false, false, 1), "Remove iteration",
                            "Defines if the remove button of the iterator is present."
                        ).setValueParser(function (node) {
                            if (!model.iterator) {
                                node.text("");
                                return true;
                            }
                        });
                        properties.push(removeIteration);

                        moveIteration = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "moveIteration", "move-iteration", false, false, 1), "Move iteration",
                            "Defines if the mover button of the iterator is present."
                        ).setValueParser(function (node) {
                            if (!model.iterator) {
                                node.text("");
                                return true;
                            }
                        });
                        properties.push(moveIteration);

                        directImport = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "directImport", "direct-import", false, false, 2), "Direct import",
                            "Defines if import is made directly without prompting the results to choose."
                        ).setValueParser(function (node) {
                            if (!model.iterator) {
                                node.text("");
                                return true;
                            }
                        });
                        properties.push(directImport);

                        setTimeout(toggleIteratorControls, 0);
                    }

                    break;
                case "Text":
                    var heightBaseProperty, heightProperty, newLineBaseProperty, newLineProperty;
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                        "Content",
                        "The content of the text.",
                        function () { $("p", item.getElement()).html((model.description || '').split('\n').join('<br />')); }
                    ));
                    properties.push(newLineBaseProperty = new prototypes.Property(
                        this, newLineProperty = new prototypes.BooleanProperty(model, "newLine", model.width == 0, true),
                        "New line",
                        "If checked, the field breaks the line. Available only if the width is not set on auto.",
                        function () { item.getElement().toggleClass("NewLine", model.newLine); }
                    ));
                    properties.push(heightBaseProperty = new prototypes.Property(
                        this, heightProperty = new prototypes.ListProperty(model, "height", [
                            { label: "Auto", code: "0" },
                            { label: "Fixed standard height", code: "1" },
                        ], { readOnly: model.width == 0 }),
                        "Height",
                        "The height of the text. Available only if the width is not set on auto.",
                        function () {
                            if (model.height == 0) {
                                model.newLine = true;
                                newLineBaseProperty.setValue();
                            }
                            newLineProperty.setReadOnly(state != prototypes.Prototype.states.draft || model.height == 0)
                            item.getSpecificItem().setSize();
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ListProperty(model, "width", [
                            { label: "Auto", code: "0" },
                            { label: "Single width", code: "1" },
                            { label: "Double width", code: "2" }
                        ]),
                        "Width",
                        "The width of the text.",
                        function () {
                            if (model.width == 0) {
                                model.height = 0;
                                model.newLine = true;
                                heightBaseProperty.setValue();
                                newLineBaseProperty.setValue();
                            }
                            newLineProperty.setReadOnly(state != prototypes.Prototype.states.draft || model.height == 0);
                            heightProperty.setReadOnly(state != prototypes.Prototype.states.draft || model.width == 0);
                            item.getSpecificItem().setSize();
                        }
                    ));

                    break;
                case "Note":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                        "Content",
                        "The content of the text.",
                        function () { $("p", item.getElement()).html((model.description || '').split('\n').join('<br />')); }
                    ));

                    break;
                case "Field":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Label",
                        "The label of the item.",
                        function () { $("label", item.getElement()).children("span").html(model.description); }
                    ));

                    var
                        linesCountProperty,
                        linesCountItems,
                        prepareLinesCount = function () {
                            linesCountItems = [
                                { label: "Single width", code: "0" },
                                { label: "Double width", code: "-1" },
                                { label: "Full width", code: "1" }
                            ];

                            if (!model.valueTypeName || model.valueTypeName == "Text")
                                for (var index = 2; index < 100; ++index)
                                    linesCountItems.push({ label: index + " rows", code: index });
                        };

                    prepareLinesCount();

                    if (!model.linesCount) model.linesCount = 0;
                    properties.push(new prototypes.Property(
                        this, linesCountProperty = new prototypes.ListProperty(model, "linesCount", linesCountItems),
                        "Size",
                        "The size of the field.",
                        function () {
                            item.getSpecificItem().setSize();
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "newLine"),
                        "New line",
                        "If checked, the field breaks the line.",
                        function () { item.getElement().toggleClass("NewLine", model.newLine); }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ValueProperty(model), "Value",
                        "The value of the field."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ValueTypeProperty(model, "valueTypeName", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Value type",
                        "The data type returned by field. Data type affects the behaviour of the field and restricts the allowed inputs.",
                        function () {
                            prepareLinesCount();
                            if (model.valueTypeName != "Text" && model.linesCount > 1) {
                                var element = item.getElement();
                                model.linesCount = 0;
                                element.removeClass("MultiLine");
                                $("div.Input", element).outerHeight(36);
                            }
                            linesCountProperty.refresh(linesCountItems);
                        }
                    ));
                    break;
                case "Signature":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Label",
                        "The label (cause) of the signature.",
                        function () { $("label", item.getElement()).children("span").html(model.description); }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "buttonLabel", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Button label",
                        "The label of the button.",
                        function () {
                            var
                                button = $("a.SignatureButton", item.getElement()),
                                icon = button.children().clone();
                            button.text(model.buttonLabel).prepend(icon);
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ListProperty(model, "signatureLevel", matematici.profiles, { nullable: true }), "Signature level",
                        "If has a value only users with that level can sign."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "cancelable"), "Cancelable",
                        "If checked, the signature can be reverted by a signature manager."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.EmailProperty(model, state != prototypes.Prototype.states.draft), "Send email",
                        "If the option 'distribution lists' is selected, an atuomatic email will be automatically sent to the specified distribution lists. If the option 'instance editors' is selected or the distribution list is left empty, all the possible instance editors will be prompted to the signer. The signer will be then able to send an automatic email to any of those users."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "acceptsPin"), "Accepts PIN",
                        "If checked, the signature users can issue this signature using their PIN."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.DataSourceProperty(model, "commandExternalSourceName"), "Command source",
                        "A data source to call once signed."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "closeInstance"), "Close instance",
                        "If checked, tries to close the instance once issued."
                    ));
                    break;
                case "InstanceLink":
                    var parametersProperty;
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Label",
                        "The label of the instance link.",
                        function () { $("label", item.getElement()).children("span").html(model.description); }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.PrototypeProperty(model),
                        "Prototype",
                        "The prototype of the linked checklist.",
                        function () {
                            if (!model.prototypeName) {
                                model.parameters = null;
                                parametersProperty.setValue();
                            }
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ListProperty(model, "actions", [
                            { label: "Link and create instances", code: "0" },
                            { label: "Create instances", code: "1" },
                            { label: "Link instances", code: "2" }
                        ]),
                        "Actions",
                        "The actions available.",
                        function () {
                            if (model.actions == 2) {
                                model.parameters = null;
                                parametersProperty.setValue();
                            }
                        }
                    ));
                    properties.push(parametersProperty = new prototypes.Property(
                        this, new prototypes.ParametersProperty(model),
                        "Parameters",
                        "The parameters to pass to the created instances."
                    ));
                    break;
                case "Link":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Label",
                        "The label of the link.",
                        function () { $("a.Link", item.getElement()).html((model.description || '(no label)')); }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "url", { readOnly: state != prototypes.Prototype.states.draft, fullHeight: true }),
                        "Url",
                        "The url to open.",
                        function () { $("a.Link", item.getElement()).attr("href", model.url); }
                    ));
                    break;
                case "Image":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Label",
                        "The label of the button.",
                        function () { $("a.Button span", item.getElement()).html((model.description || 'open image')); }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.UploadProperty(model, "key", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Image",
                        "Upload the image to show."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.TargetProperty(model, "Image"),
                        "Target",
                        "If the target is specified, the button will show the image of the specified item."
                    ));
                    break;
                case "Attachment":
                    var restrictToCameraProperty;
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Label",
                        "The label of the button.",
                        function () {
                            $("a.Attachment", item.getElement())
                                .html(model.description || "upload")
                                .append($("<i />").addClass("Icon fa fa-paperclip"));
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ListProperty(
                            model,
                            "fileType",
                            /* The first element is the default one. To change it, change also in CheckListManager.Prototypes.Logic.Models.CheckLists.Attachment */
                            ["images", "pdf"]
                        ), "File type",
                        "The file type of the attachment.",
                        function () {
                            if (model.fileType != "images") {
                                model.restrictToCamera = false;
                                restrictToCameraProperty.setValue();
                            }
                            restrictToCameraProperty.getType().setReadOnly(model.fileType == "pdf");
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.TargetProperty(model, "Attachment"),
                        "Target",
                        "If the target is specified, the attachment will be linked to the specified item."
                    ));
                    properties.push(restrictToCameraProperty = new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "restrictToCamera", model.fileType == "pdf"),
                        "Restrict to camera",
                        "If the flag is set, the attachment will be taken exclusively by camera of the device, otherwise it will be possible to browse in the saved images."
                    ));
                    break;
                default:
                    alert("Unexpected type name " + model.typeName);
                    console.log(model);
            }
        }

        properties.sort(function (a, b) { return a.getCaption().localeCompare(b.getCaption()); });
        for (var property in properties)
            properties[property].setup();

        if (!value)
            propertyPanel.setCurrentProperty()
    };

    propertyPanel.getPropertyList = function () {
        return propertyList;
    };

    propertyPanel.getPropertyValue = function () {
        return propertyValue;
    };

    propertyPanel.getInformation = function () {
        return information;
    };

    propertyPanel.getCurrentProperty = function () {
        return currentProperty;
    };

    propertyPanel.setCurrentProperty = function (value) {
        if (currentProperty) currentProperty.unselect();
        currentProperty = currentProperty == value ? null : value;
        prototypes.prototype.setItemHandler(null);
        if (currentProperty) currentProperty.select();
    };

    propertyPanel.checkDataType = function (expression, typeName) {
        return expression.innerDataType && expression.innerDataType.typeName == typeName || expression.dataType && expression.dataType.typeName == typeName;
    };

    prototypes.propertyPanel = propertyPanel;
}