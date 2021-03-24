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
            for (var property in source)
                destination[property] = source[property];
        },

        confirm = function () {
            if (model.typeName == "Image") {
                if (oldModel.key && !oldModel.cloned && !model.key)
                    prototypes.prototype.deleteImage(oldModel.key);
                if (!oldModel.key && model.key) //in case the image was previously enqueued to be deleted...
                    prototypes.prototype.ensureImageWillNotBeDeleted(model.key);
            }
            $("input:not(.ReadOnly)", propertyValue).change();
            options && options.okHandler && options.okHandler();

            prototypes.prototype.verify();
        };

    propertyPanel.confirm = confirm;

    propertyPanel.select = function (value, o) {
        if (!!value) item = value;

        var state = prototypes.prototype.getModel().state;
        model = item.getModel();
        oldModel = {};
        options = o;

        copyModel(model, oldModel);

        var name = model.typeName;

        matematici.popup.show({
            title: "Properties - " + (name == "Prototype" ? "Checklist" : name) + (model.index != null && " - Index: " + (parseInt(model.index) + 1) || model.id != null && " - ID: " + model.id || "") + (model.code && " - Code: " + model.code || ""),
            content: element,
            buttons: [{
                icon: "check",
                text: "apply",
                handler: confirm
            }, {
                cancelButton: true,
                hidden: true,
                handler: function () {
                    if (model.typeName == "Image" && !oldModel.key && model.key)
                        prototypes.prototype.deleteImage(model.key);
                    copyModel(oldModel, model);
                    prototypes.prototype.update();
                }
            }]
        });

        matematici.popup.setWidth("760px");

        prototypes.prototype.setItemHandler(null);
        propertyList.empty();
        propertyValue.empty();
        information.empty();
        properties = [];

        if (model.typeName == "Prototype") {
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
                function () { $("h2 span", item.getElement()).text(model.description); }
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
                readOnly: state != prototypes.Prototype.states.draft || prototypes.prototype.getPrototypeKey().version > 0 || prototypes.prototype.getPrototypeKey().id > 0
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
                this, new prototypes.BooleanProperty(model, "ftpExport", state != prototypes.Prototype.states.draft),
                "FTP Export", "When checked, the instances are exported to the FTP server, once confirmed."
            ));

        } else {
            var
                parent = item.getParent(),
                siblings = !!parent && parent.getChildren(),
                itemIndex = item.getIndex(),
                isHeader = model.typeName == "Group" && parent.getModel().header === model;

            if (!isHeader)
                properties.push(new prototypes.Property(
                    this, new prototypes.BooleanProperty(model, "pageBreak", state != prototypes.Prototype.states.draft, model.typeName == "Page"), "Page break",
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
                "Uniquely identifies an element. Can be left empty. Use only if a reference to the element is needed is some expression.",
                function (value) {
                    item.getElement().data("code", model.code);
                    prototypes.prototype.completeModel();
                }
            ));

            if (model.typeName != "Text" && model.typeName != "Link" && model.typeName != "Image")
                properties.push(new prototypes.Property(
                    this, new prototypes.ExpressionProperty(model, "readOnly", "read-only", true, true, 0), "Read-only",
                    "Determines if the element is editable by the operator or not."
                ));
            properties.push(new prototypes.Property(
                this, new prototypes.ExpressionProperty(model, "hidden", "hidden", false, false, 2), "Hidden",
                "Determines if the element is visible or not."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.CountDownToProperty(model, "countDownToCode"), "Countdown to",
                "Shows the countdown until the date and time are reached."
            ));

            if (model.typeName != "Link" && model.typeName != "Text" && model.typeName != "Image") {
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
                    "Determines which signature locks the element."
                ));
            }

            if (model.typeName != "Page") {
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
                    this, new prototypes.ListProperty(model, "color", [
                        { label: "(Default)", code: "" },
                        { label: "Black", code: "#000" },
                        { label: "Red", code: "#cc0000" },
                        { label: "Cyan", code: "#e8f7fc" },
                        { label: "Green", code: "#009688" },
                        { label: "Grey", code: "#999" },
                        { label: "Yellow", code: "#ffba00" }
                    ], { readOnly: state != prototypes.Prototype.states.draft }),
                    "Color",
                    "The color of the text.",
                    function (value) { item.setColor(model.color); }
                ));
            }

            switch (model.typeName) {
                case "Page":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Caption",
                        "The caption of the page.",
                        function () { $("div.TabLabels a.Selected", prototypes.prototype.getTab().getElement()).text(model.description); }
                    ));
                    break;
                case "Group":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                        "Caption",
                        "The caption of the group.",
                        function () { item.getElement().children("h3").text(model.description); }
                    ));
                    var tableContent = false;
                    for (var node = parent; node.getModel().typeName == "Group"; node = node.getParent())
                        if (node.getModel().showAsTable) {
                            tableContent = true;
                            break;
                        }

                    if (!tableContent && !isHeader) {
                        properties.push(new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "iterator", state != prototypes.Prototype.states.draft), "Iterator",
                            "Determines if the group is an iterator.",
                            function () {
                                var itaratorCommandsEnabled = prototypes.prototype.getModel().state == prototypes.Prototype.states.draft && model.iterator;
                                showAsTable.getType().setReadOnly(!itaratorCommandsEnabled);
                                instanceIterationsNumber.getType().setReadOnly(!itaratorCommandsEnabled);
                                prototypeIterationsNumber.getType().setReadOnly(!itaratorCommandsEnabled);
                                addIteration.getType().setReadOnly(!itaratorCommandsEnabled);
                                removeIteration.getType().setReadOnly(!itaratorCommandsEnabled);
                                moveIteration.getType().setReadOnly(!itaratorCommandsEnabled);

                                if (!model.iterator) {
                                    model.instanceIterationsNumber = 0;
                                    model.prototypeIterationsNumber = 0;

                                    model.showAsTable = false;
                                    model.addIterationCondition = 1;
                                    model.removeIterationCondition = 1;
                                    model.moveIterationCondition = 1
                                }
                            }
                        ));

                        var showAsTable = new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "showAsTable", state != prototypes.Prototype.states.draft || !model.iterator), "Show as table",
                            "Displays the group as a table."
                        );
                        properties.push(showAsTable);

                        var instanceIterationsNumber = new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "instanceIterations", { number: true, readOnly: state != prototypes.Prototype.states.draft || !model.iterator }), "Instance iterations",
                            "Creates the specified number of iterations automatically for new instances."
                        );
                        properties.push(instanceIterationsNumber);

                        var prototypeIterationsNumber = new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "prototypeIterations", { number: true, readOnly: state != prototypes.Prototype.states.draft || !model.iterator }), "Prototype iterations",
                            "Creates the specified number of iterations automatically int the PDF version."
                        );
                        properties.push(prototypeIterationsNumber);

                        var addIteration = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "addIteration", "add-iteration", false, false, 1), "Add iteration",
                            "Defines if the add button of the iterator is present."
                        );
                        properties.push(addIteration);

                        var removeIteration = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "removeIteration", "remove-iteration", false, false, 1), "Remove iteration",
                            "Defines if the remove button of the iterator is present."
                        );
                        properties.push(removeIteration);

                        var moveIteration = new prototypes.Property(
                            this, new prototypes.ExpressionProperty(model, "moveIteration", "move-iteration", false, false, 1), "Move iteration",
                            "Defines if the mover button of the iterator is present."
                        );
                        properties.push(moveIteration);
                    }

                    break;
                case "Text":
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
                        function () { $("label", item.getElement()).children("span").text(model.description); }
                    ));

                    let
                        linesCountProperty,
                        linesCountItems,
                        prepareLinesCount = function () {
                            linesCountItems = [
                                { label: "Standard width", code: "0" },
                                { label: "Full width", code: "1" }
                            ];

                            if (!model.valueTypeName || model.valueTypeName == "Text")
                                for (var index = 2; index < 100; ++index)
                                    linesCountItems.push({ label: index + " rows", code: index });
                        };

                    prepareLinesCount();

                    if (!model.linesCount) model.linesCount = 0;
                    properties.push(new prototypes.Property(
                        this, linesCountProperty = new prototypes.ListProperty(model, "linesCount", linesCountItems, { readOnly: state != prototypes.Prototype.states.draft }),
                        "Size",
                        "The size of the field.",
                        function () {
                            var element = item.getElement();
                            element.toggleClass("MultiLine", model.linesCount > 0);
                            $("div.Input", element).outerHeight(model.linesCount < 2 ? 36 : 20 * model.linesCount + 12);
                        }
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "newLine", state != prototypes.Prototype.states.draft),
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
                        function () { $("label", item.getElement()).children("span").text(model.description); }
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
                        this, new prototypes.ListProperty(model, "signatureLevel", matematici.profiles,
                            { readOnly: state != prototypes.Prototype.states.draft, nullable: true }), "Signature level",
                        "If has a value only users with that level can sign."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "cancelable", state != prototypes.Prototype.states.draft), "Cancelable",
                        "If checked, the signature can be reverted by a signature manager."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.EmailProperty(model, state != prototypes.Prototype.states.draft), "Send email",
                        "If the option 'distribution lists' is selected, an atuomatic email will be automatically sent to the specified distribution lists. If the option 'instance editors' is selected or the distribution list is left empty, all the possible instance editors will be prompted to the signer. The signer will be then able to send an automatic email to any of those users."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "acceptsPin", state != prototypes.Prototype.states.draft), "Accepts PIN",
                        "If checked, the signature users can issue this signature using their PIN."
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
                    break;
                case "Attachment":
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
                            ["images", "pdf"],
                            { readOnly: state != prototypes.Prototype.states.draft }
                        ), "File type",
                        "The file type of the attachment."
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

    propertyPanel.setPrototypeTypesList = function (data) {
        prototypeTypesList = data;
    };

    propertyPanel.getPrototypeTypesList = function () {
        return prototypeTypesList;
    };

    propertyPanel.checkDataType = function (expression, typeName) {
        return expression.innerDataType && expression.innerDataType.typeName == typeName || expression.dataType && expression.dataType.typeName == typeName;
    };

    prototypes.propertyPanel = propertyPanel;
}