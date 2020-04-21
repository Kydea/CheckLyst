instances.Text = function (item, model) {
    var
        p,

        setup = function () {
            p = document.createElement("p");
            p.className = "Description";
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