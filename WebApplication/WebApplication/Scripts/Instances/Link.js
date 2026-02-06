instances.Link = function (item, model) {
    var
        i, a, linkedItem,

        click = function (e) {
            e.preventDefault();
            if (!linkedItem.trigger()) matematici.dialog.show("warning", "The " + model.elementType + " " + model.elementCode + " is not enabled");
        },

        setup = function () {
            var element = item.getElement();

            i = document.createElement("i");
            i.className = "Icon Link fa fa-caret-right";
            element.appendChild(i);

            a = document.createElement("a");
            a.className = "Link";
            element.appendChild(a);

            if (model.elementType == "Attachment" || model.elementType == "Image") {
                linkedItem = instances.instance.getItems()[model.elementId].getTypedItem();
                a.addEventListener("click", click);
                a.setAttribute("href", "#");
                a.setAttribute("title", model.elementType + " " + model.elementCode);
            }
        };

    this.update = function (model) {
        a.innerHTML = model.descriptionValue || "(no label)";

        if (!linkedItem) {
            if (!!model.url) {
                a.setAttribute("target", "_blank");
                a.setAttribute("href", model.url);
            } else {
                a.removeAttribute("target");
                a.setAttribute("href", "#");
            }
        }

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        i.style.color = value;
        a.style.color = value;
    };

    this.setFontSize = function (value) {
        a.style.fontSize = value && value + "px";
    };

    setup();
}