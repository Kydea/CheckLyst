instances.Item = function (model, parent) {
    var
        item = this,
        element,
        elementInformation,

        typeNames = {
            text: "Text",
            field: "Field",
            signature: "Signature",
            link: "Link",
            image: "Image",
            attachment: "Attachment"
        },

        typedItem,

        create = function () {
            element = document.createElement(item.isTableContent() ? "td" : "div");
            element.className = "Element Item " + model.typeName;
            if (model.newLine) element.classList.add("NewLine");
            element.setAttribute("data-code", model.code);
            element.setAttribute("data-id", model.id);

            if (typedItem = (
                model.typeName == typeNames.text ? new instances.Text(item, model) :
                model.typeName == typeNames.field ? new instances.Field(item, model) :
                model.typeName == typeNames.signature ? new instances.Signature(item, model) :
                model.typeName == typeNames.link ? new instances.Link(item, model) :
                model.typeName == typeNames.image ? new instances.Image(item, model) :
                model.typeName == typeNames.attachment ? new instances.Attachment(item, model) :
                null)
            ) {
                parent.getElement().appendChild(element);
                typedItem.update(model);
            } else {
                console.log(model);
                alert("Invalid type name for item " + model.id + ": " + model.typeName);
            }

            elementInformation = new instances.ElementInformation(element);
        };

    this.update = function (model) {
        instances.instance.getItems()[model.id] = item;
        element.classList.toggle("Valid", model.valid);

        if (item.isTableContent())
            element.classList.toggle("Invisible", !!model.hidden);
        else
            element.classList.toggle("Hidden", !!model.hidden);

        if (model.countDownToValue)
			new instances.CountDown(element, model.countDownToValue, model.id);
        typedItem.update(model)

        var errorSpans = element.querySelectorAll("span.Error, span.Null");
        for (var index = 0; index < errorSpans.length; ++index) {
            var
                errorSpan = errorSpans[index],
                i = document.createElement("i");

            i.className = "fas fa-exclamation-triangle";
            i.setAttribute("title", errorSpan.textContent || "no value");
            errorSpan.innerHTML = "";
            errorSpan.appendChild(i);
        }

        elementInformation.update(model);
    };

    this.getElement = function () {
        return element;
    };

    this.getParent = function () {
        return parent;
    };

    this.isTableContent = function () {
        return parent.isTableContent() || parent.isTable();
    };

    this.getTypedItem = function () {
        return typedItem;
    };

    create();
}