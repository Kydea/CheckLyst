prototypes.PropertyPanel = function (selector) {
    var
        propertyPanel = this,
        element = $(selector),
        border = element.children("div.Border"),
        noItem = element.children("div.NoItem"),
        propertyList = element.children("div.PropertyList"),
        propertyValue = element.children("div.PropertyValue"),
        information = element.children("div.Information"),
        dragY,
        currentProperty,
        prototypeTypesList,
        properties,

        drag = function (e) {
            e.preventDefault();
            var height = element.height() + dragY - e.pageY;
            if (height > 100) {
                element.height(Math.max(height, 100));
                dragY = e.pageY;
            }
        },

        copyModel = function (source, destination) {
            for (var property in source)
                destination[property] = source[property];
        };

    propertyPanel.select = function (value) {
        var
            item = value,
            model = item.getModel(),
            state = prototypes.prototype.getModel().state,
            oldModel = {};

        copyModel(model, oldModel);

        matematici.popup.show({
            title: "Properties - " + model.typeName,
            content: element,
            buttons: [{
                icon: "check",
                text: "apply",
                handler: function () {
                    $("input:not(.ReadOnly)", propertyValue).change();
                }
            }, {
                cancelButton: true,
                hidden: true,
                handler: function () {
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
                this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () { $("h2 span", item.getElement()).text(model.description); }),
                "Description", "A short description of the prototype."
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
                "The state of the prototype. This property is not editable. " +
                "Use the action <strong>buttons</strong> to change the state of the version."
            ));
            properties.push(new prototypes.Property(this, new prototypes.PrototypeTypeProperty(model, {
                readOnly: state != prototypes.Prototype.states.draft || prototypes.prototype.getPrototypeKey().version > 0 || prototypes.prototype.getPrototypeKey().id > 0
            }), "Prototype Type", "Department and Prototype type of the prototype."));
            properties.push(new prototypes.Property(
                this, new prototypes.DateProperty(model, "validFrom", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([{
                    action: function (value) {
                        return !value || value > new Date();
                    },
                    message: "Specify a future date or leave empty"
                }]), "Validity start",
                "If specified, in this date, the version gets the state <b>active</b>, if the state of the version is <b>approved</b>. Otherwise the version get the state <b>active</b> immediately when approved."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.DateProperty(model, "validUntil", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([{
                    action: function (value) {
                        return !value || value > new Date();
                    },
                    message: "Specify a future date or leave empty"
                }, {
                    action: function (value) {
                        return !model.validFrom || !value || new Date(model.validFrom) < value;
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
                itemIndex = item.getIndex();

            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "code", {
                    readOnly: state != prototypes.Prototype.states.draft,
                    code: true
                }).setHandler(function (value) {
                    item.getElement().data("code", model.code);
                    prototypes.prototype.completeModel();
                }).setCheck([{
                    action: function (value) {
                        return value == "" || matematici.codeRegularExpression.test(value);
                    },
                    message: "Invalid code"
                }]), "Code",
                "Uniquely identifies an element. Can be left empty. Use only if a reference to the element is needed is some expression."
            ));
            if (model.typeName != "Text" && model.typeName != "Link" && model.typeName != "Image")
                properties.push(new prototypes.Property(
                    this, new prototypes.ReadOnlyProperty(model), "Read-only",
                    "Determines if the element is editable by the operator or not."
                ));
            properties.push(new prototypes.Property(
                this, new prototypes.HiddenProperty(model, "hidden", state != prototypes.Prototype.states.draft), "Hidden",
                "Determines if the element is visible or not."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.CountDownToProperty(model, "countDownToCode"), "Countdown to",
                "Shows the countdown until the date and time are reached."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.ValidProperty(model), "Validity",
                "Sets the condition under which the element is considered valid. By default pages and groups are valid if all the contained elements are valid, fields are valid if contains a value, signature are valid if signed. Text are always valid. Attachments are valid if contains a file."
            ));
            properties.push(new prototypes.Property(
                this, new prototypes.SimpleValueProperty(model, "validityMessage", { readOnly: state != prototypes.Prototype.states.draft }), "Validity message",
                "The error message to display to the user when the item is not valid to help him solve the problem."
            ));


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
                    ]).setHandler(function () {
                        item.setFontSize(model.fontSize);
                    }), "Font size",
                    "The size in pixel of the text (empty or 0 for the default font size)."
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
                    ], { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function (value) {
                        item.setColor(model.color);
                    }), "Color",
                    "The color the text."
                ));
            }

            properties.push(new prototypes.Property(
                this, new prototypes.SignatureProperty(model, { readOnly: state != prototypes.Prototype.states.draft }), "Signature",
                "Determines which signature locks the element."
            ));

            switch (model.typeName) {
                case "Page":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("div.TabLabels a.Selected", prototypes.prototype.getTab().getElement()).text(model.description);
                        }), "Caption",
                        "The caption of the page."
                    ));
                    break;
                case "Group":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            item.getElement().children("h3").text(model.description);
                        }), "Caption",
                        "The caption of the group."
                    ));
                    var tableContent = false;
                    for (var node = parent; node.getModel().typeName == "Group"; node = node.getParent())
                        if (node.getModel().showAsTable) {
                            tableContent = true;
                            break;
                        }

                    if (!tableContent && parent.getModel().header !== model) {
                        properties.push(new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "iterator", state != prototypes.Prototype.states.draft).setHandler(function () {
                                showAsTable.getType().setReadOnly(!model.iterator);
                                instanceIterationsNumber.getType().setReadOnly(!model.iterator);
                                prototypeIterationsNumber.getType().setReadOnly(!model.iterator);
                                disableAdd.getType().setReadOnly(!model.iterator);
                                disableRemove.getType().setReadOnly(!model.iterator);
                                disableMove.getType().setReadOnly(!model.iterator);

                                if (!model.iterator) {
                                    model.instanceIterationsNumber = 0;
                                    model.prototypeIterationsNumber = 0;

                                    model.showAsTable =
                                        model.disableAdd =
                                        model.disableRemove =
                                        model.disableMove =
                                        false;
                                }
                            }), "Iterator",
                            "Determines if the group is an iterator."
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

                        var disableAdd = new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "disableAdd", state != prototypes.Prototype.states.draft || !model.iterator), "Disable add",
                            "Disable the add button of the iterator."
                        );
                        properties.push(disableAdd);

                        var disableRemove = new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "disableRemove", state != prototypes.Prototype.states.draft || !model.iterator), "Disable remove",
                            "Disable the remove button of the iterator."
                        );
                        properties.push(disableRemove);

                        var disableMove = new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "disableMove", state != prototypes.Prototype.states.draft || !model.iterator), "Disable move",
                            "Disable the move up and move down buttons of the iterator."
                        );
                        properties.push(disableMove);
                    }

                    break;
                case "Text":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("p", item.getElement()).html((model.description || '').split('\n').join('<br />'));
                        }), "Content",
                        "The content of the text."
                    ));
                    break;
                case "Field":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("label", item.getElement()).children("span").text(model.description);
                        }), "Label",
                        "The label of the item."
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
                        this, linesCountProperty = new prototypes.ListProperty(model, "linesCount", linesCountItems, { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            var element = item.getElement();
                            element.toggleClass("MultiLine", model.linesCount > 0);
                            $("div.Input", element).outerHeight(model.linesCount < 2 ? 36 : 20 * model.linesCount + 12);
                        }), "Size",
                        "The size of the field."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "newLine", state != prototypes.Prototype.states.draft).setHandler(function () {
                            item.getElement().toggleClass("NewLine", model.newLine);
                        }), "New line",
                        "If checked, the field breaks the line."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ValueProperty(model), "Value",
                        "The value of the field."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.ListProperty(model, "valueTypeName", [
                            { label: "Text", code: "Text" },
                            { label: "Boolean", code: "Boolean" },
                            { label: "Number", code: "Number" },
                            { label: "Date Time", code: "DateTime" },
                            { label: "Date", code: "Date" },
                            { label: "Time", code: "Time" },
                        ], { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            prepareLinesCount();
                            if (model.valueTypeName != "Text" && model.linesCount > 1) {
                                var element = item.getElement();
                                model.linesCount = 0;
                                element.removeClass("MultiLine");
                                $("div.Input", element).outerHeight(36);
                            }
                            linesCountProperty.refresh(linesCountItems);
                        }), "Value type",
                        "The data type returned by field. Data type affects the behaviour of the field and restricts the allowed inputs."
                    ));
                    break;
                case "Signature":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("label", item.getElement()).children("span").text(model.description);
                        }), "Label",
                        "The label (cause) of the signature."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "buttonLabel", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            var
                                button = $("a.SignatureButton", item.getElement()),
                                icon = button.children().clone();
                            button.text(model.buttonLabel).prepend(icon);
                        }), "Button label",
                        "The label of the button."
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
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("a.Link", item.getElement()).html((model.description || '(no label)'));
                        }), "Label",
                        "The label of the link."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "url", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("a.Link", item.getElement()).attr("href", model.url);
                        }), "Url",
                        "The url to open."
                    ));
                    break;
                case "Image":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("a.Button span", item.getElement()).html((model.description || 'open image'));
                        }), "Label",
                        "The label of the button."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.UploadProperty(model, "key", { readOnly: state != prototypes.Prototype.states.draft }),
                        "Image",
                        "Upload the image to show."
                    ));
                    properties.push(new prototypes.Property(
                        this, new prototypes.BooleanProperty(model, "newLine", state != prototypes.Prototype.states.draft).setHandler(function () {
                            item.getElement().toggleClass("NewLine", model.newLine);
                        }), "New line",
                        "If checked, the field breaks the line."
                    ));
                    break;
                case "Attachment":
                    properties.push(new prototypes.Property(
                        this, new prototypes.SimpleValueProperty(model, "description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                            $("a.Attachment", item.getElement())
                                .html(model.description || "upload")
                                .append($("<i />").addClass("Icon fa fa-paperclip"));
                        }), "Label",
                        "The label of the button."
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
        return expression.dataType.typeName == typeName || expression.innerDataType && expression.innerDataType.typeName == typeName;
    };

    prototypes.propertyPanel = propertyPanel;
}