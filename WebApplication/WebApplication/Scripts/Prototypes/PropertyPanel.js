prototypes.PropertyPanel = function (selector) {
    var
        propertyPanel = this,
        element = $(selector),
        border = element.children("div.Border"),
        noItem = element.children("div.NoItem"),
        propertyList = element.children("div.PropertyList"),
        propertyValue = element.children("div.PropertyValue"),
        information = element.children("div.Information"),
        item,
        dragY,
        currentProperty,

        drag = function (e) {
            e.preventDefault();
            var height = element.height() + dragY - e.pageY;
            if (height > 100) {
                element.height(Math.max(height, 100));
                dragY = e.pageY;
            }
        };

    propertyPanel.resize = function () {
        prototypes.prototype.getElement().css("padding-bottom", element.height());
    };

    propertyPanel.windowScrolled = function () {
        element.css("bottom", Math.max(0, matematici.elements.window.scrollTop() + matematici.elements.window.height() + matematici.elements.footer.height() - matematici.elements.body.height()));
    };

    propertyPanel.select = function (value) {
        prototypes.prototype.setItemHandler(null);
        item = value;
        element.toggleClass("NoItem", !value)
        if (item) {
            propertyList.empty();
            propertyValue.empty();
            information.empty();

            var
                model = item.getModel(),
                state = prototypes.prototype.getModel().State;

            if (model.typeName == "Prototype") {
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "Code", {
                        readOnly: state != prototypes.Prototype.states.draft || prototypes.prototype.getModel().Version > 0
                    }).setCheck([{
                        action: function (value) {
                            return matematici.codeRegularExpression.test(value);
                        },
                        message: "Invalid code"
                    }]), "Code",
                    "The code that uniquely identifies the prototype, can be set only for new prototypes."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "Version", { readOnly: true }), "Version",
                    "The version number."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "Description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () { $("h2", item.getElement()).text(model.description); }),
                    "Description", "A short description of the prototype."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "InstanceDescription", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                    "Instance description", "A formula to use for the description of the instances of this prototype."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "InstanceFileName", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }),
                    "Instance file name", "A formula to use for the file name of the pdf generated for confirmed instances of this prototype."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "State", { readOnly: true }), "State",
                    "The state of the prototype. This property is not editable. " +
                    "Use the button <b>start the simulation</b>, <b>cancel the simulation</b>, <b>start the simulation</b>, <b>submit for approval</b>, <b>approve</b> or <b>reject</b> to change the state of the version."
                );
                new prototypes.Property(this, new prototypes.PrototypeTypeProperty(model, {
                    readOnly: state != prototypes.Prototype.states.draft || prototypes.prototype.getModel().Version > 0
                }), "Prototype Type", "Company, Department and Prototype type of the version.");
                new prototypes.Property(
                    this, new prototypes.DateProperty(model, "ValidFrom", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([{
                        action: function (value) {
                            return !value || value > new Date();
                        },
                        message: "Specify a future date or leave empty"
                    }]), "Validity start",
                    "If specified, in this date, the version gets the state <b>active</b>, if the state of the version is <b>approved</b>. Otherwise the version get the state <b>active</b> immediately when approved."
                );
                new prototypes.Property(
                    this, new prototypes.DateProperty(model, "ValidUntil", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([{
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
                );
                new prototypes.Property(
                    this, new prototypes.BooleanProperty(model, "FTPExport", state != prototypes.Prototype.states.draft),
                    "FTP Export", "When checked, the instances are exported to the FTP server, once confirmed."
                );

            } else {
                var
                    parent = item.getParent(),
                    siblings = !!parent && parent.getChildren(),
                    itemIndex = item.getIndex();

                if (parent && state == prototypes.Prototype.states.draft && (!parent.getModel || parent.getModel().Header !== item.getModel()))
                    new prototypes.Property(
                        this, new prototypes.ActionsProperty([
                            {
                                text: "remove", icon: "remove", handler: function () {
                                    parent.getChildren().splice(itemIndex, 1);
                                    prototypes.prototype.update();
                                    matematici.canClose = false;
                                }
                            },
                            {
                                text: "move up", icon: "up", handler: function () {
                                    var items = [];
                                    for (var index in siblings)
                                        items.push(siblings[index == itemIndex - 1 ? itemIndex : index == itemIndex ? index - 1 : index]);
                                    parent.setChildren(items);
                                    prototypes.prototype.update();
                                    matematici.canClose = false;
                                }, visible: itemIndex > 0
                            },
                            {
                                text: "move down", icon: "down", handler: function () {
                                    var items = [];
                                    for (var index in siblings)
                                        items.push(siblings[index == itemIndex ? itemIndex + 1 : index == itemIndex + 1 ? itemIndex : index]);
                                    parent.setChildren(items);
                                    prototypes.prototype.update();
                                    matematici.canClose = false;
                                }, visible: itemIndex < siblings.length - 1
                            }
                        ]), "Actions",
                        ""
                    );

                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "Code", {
                        readOnly: state != prototypes.Prototype.states.draft
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
                );
                new prototypes.Property(
                    this, new prototypes.ReadOnlyProperty(model), "Read-only",
                    "Determines if the element is editable by the operator or not."
                );
                new prototypes.Property(
                    this, new prototypes.HiddenProperty(model, "Hidden", state != prototypes.Prototype.states.draft), "Hidden",
                    "Determines if the element is visible or not."
                );
                new prototypes.Property(
                    this, new prototypes.ExpressionProperty(model, "CountDownToCode", { readOnly: state != prototypes.Prototype.states.draft }).setCheck([
                        {
                            action: function (expression) {
                                return !expression || expression.DataType.TypeName == "DateTime";
                            },
                            message: "the expression must result must be a date/time"
                        }
                    ]), "Countdown to",
                    "Shows the countdown until the date and time are reached."
                );
                new prototypes.Property(
                    this, new prototypes.ValidProperty(model), "Validity",
                    "Sets the condition under which the element is considered valid. By default pages and groups are valid if alle the contained elements are valid, fields are valid if contains a value, signature are valid if signed. Text are always valid."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "ValidityMessage", { readOnly: state != prototypes.Prototype.states.draft }), "Validity message",
                    "The error message to display to the user when the item is not valid to help him solve the problem."
                );
                new prototypes.Property(
                    this, new prototypes.SimpleValueProperty(model, "FontSize", { number: true, readOnly: state != prototypes.Prototype.states.draft }).setCheck([
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
                    "The size in pixel of the text (leave empty for the default font size)."
                );
                new prototypes.Property(
                    this, new prototypes.ListProperty(model, "Color", [
                        { label: "Black", code: "" },
                        { label: "Red", code: "#f00" },
                        { label: "Blue", code: "#086386" },
                        { label: "Green", code: "#0a0" },
                        { label: "Grey", code: "#9e9e9e" },
                        { label: "Orange", code: "#f90" },
                    ], { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function (value) {
                        item.setColor(model.color);
                    }), "Color",
                    "The color the text."
                );
                new prototypes.Property(
                    this, new prototypes.SignatureProperty(model, { readOnly: state != prototypes.Prototype.states.draft }), "Signature",
                    "Determines which signature locks the element. Select the signature in the list or by the command 'set by click'."
                );

                switch (model.typeName) {
                    case "Page":
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "Description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                                $("div.TabLabels a.Selected", prototypes.prototype.getTab().getElement()).text(model.description);
                            }), "Caption",
                            "The caption of the page."
                        );
                        break;
                    case "Group":
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "Description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                                item.getElement().children("h3").text(model.description);
                            }), "Caption",
                            "The caption of the group."
                        );
                        var tableContent = false;
                        for (var node = parent; node.getModel().TypeName == "Group"; node = node.getParent())
                            if (node.getModel().ShowAsTable) {
                                tableContent = true;
                                break;
                            }
                        if (!tableContent && parent.getModel().Header !== model) {
                            new prototypes.Property(
                                this, new prototypes.BooleanProperty(model, "Iterator", state != prototypes.Prototype.states.draft).setHandler(function () {
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
                            );

                            var showAsTable = null;
                            if (parent.getModel().Header !== model)
                                showAsTable = new prototypes.Property(
                                    this, new prototypes.BooleanProperty(model, "ShowAsTable", state != prototypes.Prototype.states.draft || !model.iterator), "Show as table",
                                    "Displays the group as a table."
                                );
                            var instanceIterationsNumber = new prototypes.Property(
                                this, new prototypes.SimpleValueProperty(model, "InstanceIterations", { number: true, readOnly: state != prototypes.Prototype.states.draft || !model.iterator }), "Instance iterations",
                                "Creates the specified number of iterations automatically for new instances."
                            );
                            var prototypeIterationsNumber = new prototypes.Property(
                                this, new prototypes.SimpleValueProperty(model, "PrototypeIterations", { number: true, readOnly: state != prototypes.Prototype.states.draft || !model.iterator }), "Prototype iterations",
                                "Creates the specified number of iterations automatically int the PDF version."
                            );
                            var disableAdd = new prototypes.Property(
                                this, new prototypes.BooleanProperty(model, "DisableAdd", state != prototypes.Prototype.states.draft || !model.iterator), "Disable add",
                                "Disable the add button of the iterator."
                            );
                            var disableRemove = new prototypes.Property(
                                this, new prototypes.BooleanProperty(model, "DisableRemove", state != prototypes.Prototype.states.draft || !model.iterator), "Disable remove",
                                "Disable the remove button of the iterator."
                            );
                            var disableMove = new prototypes.Property(
                                this, new prototypes.BooleanProperty(model, "DisableMove", state != prototypes.Prototype.states.draft || !model.iterator), "Disable move",
                                "Disable the move up and move down buttons of the iterator."
                            );
                        }

                        new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "PrototypesDirectory", state != prototypes.Prototype.states.draft), "Prototypes Directory",
                            "Saves the prototype in the application Checklist Prototypes Directory."
                        );
                        break;
                    case "Text":
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "Description", { fullHeight: true, readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                                $("p", item.getElement()).html((model.description || '').split('\n').join('<br />'));
                            }), "Content",
                            "The content of the text."
                        );
                        break;
                    case "Field":
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "Description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                                $("label", item.getElement()).text(model.description);
                            }), "Label",
                            "The label of the item."
                        );
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "LinesCount", { number: true, readOnly: state != prototypes.Prototype.states.draft }).setCheck([
                                {
                                    action: function (value) {
                                        var intValue = parseInt(value);
                                        return intValue >= 0 && value == intValue;
                                    },
                                    message: "invalid value"
                                }
                            ]).setHandler(function () {
                                var element = item.getElement();
                                element.toggleClass("MultiLine", model.linesCount > 0);
                                $("div.Input", element).height(24 * model.linesCount - 10);
                            }), "Lines count",
                            "The number of text lines shown."
                        );
                        new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "NewLine", state != prototypes.Prototype.states.draft).setHandler(function () {
                                item.getElement().toggleClass("NewLine", model.newLine);
                            }), "New line",
                            "If checked, the field breaks the line."
                        );
                        new prototypes.Property(
                            this, new prototypes.ValueProperty(model), "Value",
                            "The value of the field."
                        );
                        new prototypes.Property(
                            this, new prototypes.ListProperty(model, "ValueTypeName", [
                                { label: "Text", code: "Text" },
                                { label: "Boolean", code: "Boolean" },
                                { label: "Number", code: "Number" },
                                { label: "Date Time", code: "DateTime" },
                                { label: "Date", code: "Date" },
                                { label: "Time", code: "Time" },
                            ], { readOnly: state != prototypes.Prototype.states.draft }), "Value type",
                            "The data type returned by field. Data type affects the behaviour of the field and restricts the allowed inputs."
                        );
                        break;
                    case "Signature":
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "Description", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                                $("label", item.getElement()).text(model.description);
                            }), "Label",
                            "The label (cause) of the signature."
                        );
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "ButtonLabel", { readOnly: state != prototypes.Prototype.states.draft }).setHandler(function () {
                                $("a.Button", item.getElement()).text(model.buttonLabel);
                            }), "Button label",
                            "The label of the button."
                        );
                        new prototypes.Property(
                            this, new prototypes.SimpleValueProperty(model, "SignatureLevel", { number: true, readOnly: state != prototypes.Prototype.states.draft }).setCheck([
                                {
                                    action: function (value) {
                                        var intValue = parseInt(value);
                                        return (value || "").trim() == "" || intValue >= 0 && value == intValue;
                                    },
                                    message: "invalid value"
                                }
                            ]), "Signature level",
                            "If has a value only users with that level can sign."
                        );
                        new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "Cancelable", state != prototypes.Prototype.states.draft), "Cancelable",
                            "If checked, the signature can be reverted by a signature manager."
                        );
                        new prototypes.Property(
                            this, new prototypes.BooleanProperty(model, "AcceptsPIN", state != prototypes.Prototype.states.draft), "Accepts PIN",
                            "If checked, the signature users can issue this signature using their PIN."
                        );
                        break;
                    default:
                        alert("Unexpected type name " + model.typeName);
                        console.log(model);
                }
            }
        }
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

    propertyPanel.initialize = function () {
        matematici.elements.window.on("resize", propertyPanel.windowScrolled);
        matematici.elements.window.on("scroll", propertyPanel.windowScrolled);
        prototypes.propertyPanel = propertyPanel;
        border
            .on("mousedown", function (e) {
                dragY = e.pageY;
                matematici.elements.body
                    .on("mousemove", drag)
                    .on("mouseup", function () {
                        matematici.elements.body.off("mousemove", drag);
                        propertyPanel.resize();
                    });
            });
        element.on("click", function (e) { e.stopPropagation(); });
    };

    propertyPanel.destroy = function () {
        matematici.elements.window.off("resize", propertyPanel.windowScrolled);
        matematici.elements.window.off("scroll", propertyPanel.windowScrolled);
        prototypes.propertyPanel = null;
        border.off("mousedown, mousemove, mouseup");
    };
}