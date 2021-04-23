prototypes.Group = function (element, typeName, parent, previous) {
    var
        group = this,
        title,
        model,
        index,
        addButton,

        create = function () {
            element = $("<div />");

            if (parent.isPage())
                element.appendTo(parent.getElement());
            else
                element.insertAfter(previous ? previous.getElement() : $("h3", parent.getElement()));
        },

        addElement = function (typeName, itemModel) {
            matematici.canClose = false;

            if (!model.items) model.items = [];
            if (itemModel == null) {
                itemModel = { typeName: typeName };
                //default values
                if (typeName == "Text") {
                    itemModel.newLine = true;
                    itemModel.height = itemModel.width = 0;
                }
            }
            model.items.push(itemModel);
            prototypes.prototype.update();
        },

        addGroup = function (groupModel) {
            matematici.canClose = false;

            if (!model.groups) model.groups = [];
            var newGroup = groupModel || { typeName: "Group", id: prototypes.prototype.getNewItemsId() };
            model.groups.push(newGroup);
            prototypes.prototype.update();
        },

        setup = function () {
            if (!element)
                create();

            element
                .addClass("Element " + typeName)
                .on("click", function (event) {
                    prototypes.prototype.select(event, group)
                });

            if (group.isPage())
                addButton = $("<a />")
                    .attr("href", "#")
                    .addClass("Button Add")
                    .text("Add group")
                    .prepend($("<i >").addClass("Icon Add fa fa-plus")).on("click", function () { addGroup(); });
            else
                title = $("<h3 />").appendTo(element);
        },

        update = function (model) {
            if (group.isPage())
                model.typeName = "Page";
            if (title)
                title.text(model.description ? model.description : "(no title)")
            group.setColor(model.color);
            group.setFontSize(model.fontSize);
        };

    this.add = function () {
        matematici.dialog.popupMenu([
            { text: "cancel", icon: "times", cancelButton: true },
            { text: "attachment", icon: "paperclip", handler: addElement, data: "Attachment" },
            { text: "image", icon: "image", handler: addElement, data: "Image" },
            { text: "link", icon: "link", handler: addElement, data: "Link" },
            { text: "instance link", icon: "file-invoice", handler: addElement, data: "InstanceLink" },
            { text: "signature", icon: "check", handler: addElement, data: "Signature" },
            { text: "field", icon: "i-cursor", handler: addElement, data: "Field" },
            { text: "text", icon: "align-left", handler: addElement, data: "Text" },
            { text: "group", icon: "list-ul", handler: addElement, data: "Group" }
        ]);
    }

    this.setColor = function (value) {
        if (title)
            title.css({ color: value });
    };

    this.setFontSize = function (value) {
        if (title)
            title.css({ fontSize: value == 0 ? "" : value + "px" });
    };

    this.update = function (value) {
        model = value;
        if (!group.isPage()) {
            if (model.id > 0)
                prototypes.prototype.setNewItemsId(model.id);
            else
                model.id = prototypes.prototype.getNewItemsId();
            element.attr("data-code", model.code).attr("id", "ItemId" + model.id).data("id", model.id);
        }

        update(model);

        /* disegna il prototypo */
        var
            children = group.getChildren(),
            previous = null;

        for (var index in children) {
            var
                itemModel = children[index],
                item =
                    itemModel.typeName == "Group" ? new prototypes.Group(null, itemModel.typeName, this, previous) :
                        new prototypes.Item(itemModel, this);

            item.setIndex(parseInt(index));
            item.update(itemModel);
            previous = item;
        }

        if (addButton && prototypes.prototype.getModel().state == prototypes.Prototype.states.draft)
            addButton.appendTo(element);
    };

    this.clear = function () {
        element.empty().off("click");
        setup();
    };

    this.delete = function () {
        element.remove();
        delete this;
    };

    this.getElement = function () {
        return element;
    };

    this.getModel = function () {
        return model;
    };

    this.isPage = function () {
        return typeName == "Page";
    };

    this.isGroup = function () {
        return typeName == "Group";
    };

    this.isIteration = function () {
        return typeName == "Iteration";
    };

    this.setIndex = function (value) {
        index = value;
    };

    this.getIndex = function () {
        return index;
    };

    this.getParent = function () {
        return parent;
    };

    this.getChildren = function () {
        return model.groups || model.items;
    };

    this.setChildren = function (value) {
        if (group.isPage())
            model.groups = value;
        else
            model.items = value;
    };

    this.addGroup = addGroup;

    this.addElement = addElement;

    setup();
}