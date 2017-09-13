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
                .addClass("Element Item " + model.TypeName)
                .on("click", function (event) { prototypes.prototype.select(event, item) });

            if (model.NewLine) element.addClass("NewLine");

            if (specificItem = (
                model.TypeName == typeNames.text ? new prototypes.Text(item, model) :
                model.TypeName == typeNames.field ? new prototypes.Field(item, model) :
                model.TypeName == typeNames.signature ? new prototypes.Signature(item, model) :
                null)) {
                parent.getElement().append(element);
                specificItem.update(model);
            } else {
                console.log(model);
                alert("Invalid type name for item " + model.Id + ": " + model.TypeName);
            }
        };

    this.update = function (value) {
        model = value;
        element.attr("data-code", model.Code).attr("data-id", model.Id);
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