instances.Link = function (item, model) {
    var
        i, a,

        setup = function () {
            var element = item.getElement();

            i = document.createElement("i");
            i.className = "Icon Link fa fa-caret-right";
            element.appendChild(i);

            a = document.createElement("a");
            a.className = "Link";
            element.appendChild(a);
        };

    this.update = function (model) {
        a.innerHTML = model.descriptionValue || "(no label)";
        if (!!model.url) {
            a.setAttribute("target", "_blank");
            a.setAttribute("href", model.url);
        } else {
            a.removeAttribute("target");
            a.setAttribute("href", "#");
        }

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function () {
        i.style.color = model.color;
        a.style.color = model.color;
    };

    this.setFontSize = function () {
        a.style.fontSize = model.fontSize && model.fontSize + "px";
    };

    setup();
}