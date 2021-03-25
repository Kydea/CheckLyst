instances.Text = function (item, model) {
    var
        p,

        setup = function () {
            var element = item.getElement();
            p = document.createElement("p");

            element.classList.toggle("Wide", model.width == 0);
            element.classList.toggle("SingleRow", model.height == 1);
            p.classList.toggle("Label", model.width > 0)
            p.classList.toggle("Controls2", model.width == 2);
            var ellipsis = model.height == 1 && p.offsetHeight < p.scrollHeight;
            p.classList.toggle("Ellipsis", ellipsis);
            p.setAttribute("title", ellipsis ? model.description : null);
            element.appendChild(p);
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