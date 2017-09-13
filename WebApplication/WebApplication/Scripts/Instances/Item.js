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
            element = (item.isTableContent() ? $("<td />") : $("<div />")).addClass("Element Item " + model.TypeName);
            if (model.NewLine) element.addClass("NewLine");

            if (item = (
                model.TypeName == typeNames.text ? new instances.Text(item, model) :
                model.TypeName == typeNames.field ? new instances.Field(item, model) :
                model.TypeName == typeNames.signature ? new instances.Signature(item, model) :
                null)) {
                parent.getElement().append(element);
                item.update(model);
            } else {
                console.log(model);
                alert("Invalid type name for item " + model.Id + ": " + model.TypeName);
            }

            elementInformation = new instances.ElementInformation(element);
        };

    this.update = function (model) {
        element
            .attr("data-code", model.Code)
            .attr("data-id", model.Id)
            .toggleClass("Valid", model.Valid)
            .toggleClass("Hidden", model.Hidden);
        if (model.CountDownTo)
            new instances.CountDown(element, model.CountDownTo, model.Id);
        item.update(model)
        $("span.Error, span.Null", element).each(function () {
            var
                span = $(this),
                content = span.text();
            if (!content) content = "no value";
            span.empty().append($("<i />").addClass("fa fa-warning").attr({ title: content }));
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