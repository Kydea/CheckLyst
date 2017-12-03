prototypes.Item = function (model, parent) {
    var
        item = this,
        specificItem,
        element,
        model,
        index,

        typeNames = {
            text: "Text",
            field: "Field",
            signature: "Signature"
        },

        create = function () {
            element = $("<div />")
                .addClass("Element Item " + model.typeName)
                .on("click", function (event) { prototypes.prototype.select(event, item) });

            if (model.newLine) element.addClass("NewLine");

            if (specificItem = (
                model.typeName == typeNames.text ? new prototypes.Text(item, model) :
                model.typeName == typeNames.field ? new prototypes.Field(item, model) :
                model.typeName == typeNames.signature ? new prototypes.Signature(item, model) :
                null)) {
                parent.getElement().append(element);
                specificItem.update(model);
            } else {
                console.log(model);
                alert("Invalid type name for item " + model.id + ": " + model.typeName);
            }
        };

    this.update = function (value) {
		model = value;
		if (!model.itemId)
			model.itemId = prototypes.prototype.getNewItemId();
		element.attr("data-code", model.code).attr("id", "ItemId" + model.itemId);
        specificItem.update(model)
    };

    this.setColor = function (value) {
        specificItem.setColor(value);
    };

    this.setFontSize = function (value) {
        specificItem.setFontSize(value);
    };

    this.setIndex = function (value) {
        index = value;
    }

    this.getIndex = function () {
        return index;
    }

    this.getElement = function () {
        return element;
    };

    this.getParent = function () {
        return parent;
    };

    this.getModel = function () {
        return model;
    };

    create();
}