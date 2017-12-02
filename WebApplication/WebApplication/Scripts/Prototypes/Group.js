prototypes.Group = function (element, typeName, parent, previous) {
    var
        group = this,
        title,
        model,
        addButton,
        index,

        create = function () {
            element = $("<div />");

            if (parent.isPage())
                element.appendTo(parent.getElement());
            else
                element.insertAfter(previous ? previous.getElement() : $("h3", parent.getElement()));
        },

        addElement = function (typeName) {
            matematici.canClose = false;

            if (!model.items) model.items = [];
            model.items.push({ typeName: typeName });
            prototypes.prototype.update();
        },

        addGroup = function () {
            matematici.canClose = false;

            if (!model.groups) model.groups = [];
            model.groups.push({ typeName: "Group" });
            prototypes.prototype.update();
        },

        setup = function () {
            if (!element) create();

            element
                .addClass("Element " + typeName)
                .on("click", function (event) { prototypes.prototype.select(event, group) });

            if (group.isPage()) 
                addButton = $("<div />").addClass("Add AddGroup").appendTo(element)
                    .append($("<a />").attr("href", "#").addClass("Button").text("Add group").on("click", function (e) {
                    	if (prototypes.prototype.getItemHandler())
                            return;

                        e.preventDefault();
                        e.stopPropagation();
                        if (prototypes.prototype.getModel().state != prototypes.Prototype.states.draft) return;

                        addGroup();
                    }));
            else {

                title = $("<h3 />").appendTo(element);

                addButton = $("<div />").addClass("Add").appendTo(element)
                    .append($("<a />").attr("href", "#").addClass("Button").text("Add").on("click", function (e) {
                    	if (prototypes.prototype.getItemHandler())
                            return;

                        e.preventDefault();
                        e.stopPropagation();
                        if (prototypes.prototype.getModel().state != prototypes.Prototype.states.draft) return;

                        matematici.dialog.popupMenu([
                            { text: "cancel", icon: "remove", cancelButton: true },
                            { text: "signature", icon: "check-square-o", handler: addElement, data: "Signature" },
                            { text: "field", icon: "i-cursor", handler: addElement, data: "Field" },
                            { text: "text", icon: "align-left", handler: addElement, data: "Text" },
                            { text: "group", icon: "list-ul", handler: addElement, data: "Group" }
                        ]);
                    }));
            }
        },

        update = function (model) {
            if (group.isPage())
                model.typeName = "Page";
            if (title) 
                title.text(model.description ? model.description : "(no title)")
            group.setColor(model.color);
            group.setFontSize(model.fontSize);
            addButton.children("a").toggleClass("ReadOnly", prototypes.prototype.getModel().state != prototypes.Prototype.states.draft);
        };

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

        element.append(addButton);
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

    setup();
}