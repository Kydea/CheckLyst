instances.Item = function (model, parent) {
    var
        item = this,
        element,
        elementInformation,

        typeNames = {
            text: "Text",
            field: "Field",
            signature: "Signature"
        },

        item,

        create = function () {
            element = (item.isTableContent() ? $("<td />") : $("<div />")).addClass("Element Item " + model.typeName);
            if (model.newLine) element.addClass("NewLine");

            if (item = (
                model.typeName == typeNames.text ? new instances.Text(item, model) :
                model.typeName == typeNames.field ? new instances.Field(item, model) :
                model.typeName == typeNames.signature ? new instances.Signature(item, model) :
                null)) {
                parent.getElement().append(element);
                item.update(model);
            } else {
                console.log(model);
                alert("Invalid type name for item " + model.id + ": " + model.typeName);
            }

            elementInformation = new instances.ElementInformation(element);
        };

    this.update = function (model) {
        element
            .attr("data-code", model.code)
            .attr("data-id", model.id)
            .toggleClass("Valid", model.valid)
            .toggleClass("Hidden", model.hidden);
        if (model.countDownTo)
            new instances.CountDown(element, model.countDownTo, model.id);
        item.update(model)
        $("span.Error, span.Null", element).each(function () {
            var
                span = $(this),
                content = span.text();
            if (!content) content = "no value";
			span.empty().append($("<i />").addClass("fi flaticon-warning").attr({ title: content }));
        });
        elementInformation.update(model);
    };

    this.getElement = function () {
        return element;
    };

    this.getParent = function () {
        return parent;
    }

    this.isTableContent = function () {
        return parent.isTableContent() || parent.isTable();
    }

    create();
}