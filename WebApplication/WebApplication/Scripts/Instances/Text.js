instances.Text = function (item, model) {
    var
        p,

        setup = function () {
            var element = item.getElement();
            p = document.createElement("p");
            if (model.width == 1)
                element.classList.add("Label SingleWidth")
            else if (model.width == 2)
                element.classList.add("Label DoubleWidth")

            if (model.heigth == 1)
                element.classList.add("SingleRow")

            item.getElement().appendChild(p);
        };

    this.update = function (model) {
		p.innerHTML = model.descriptionValue ? model.descriptionValue.split('\n').join('<br />') : "(no description)";
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function () {
        p.style.color = model.color;
    };

    this.setFontSize = function () {
        p.style.fontSize = model.fontSize && model.fontSize + "px";
    };

    setup();
}