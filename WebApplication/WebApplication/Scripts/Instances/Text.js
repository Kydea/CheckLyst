instances.Text = function (item, model) {
    var
        element,
        p,
        ellipsis,

        showMore = function () {
            matematici.dialog.show("Full content", model.descriptionValue);
            matematici.dialog.setWidth("900px");
        };

        setup = function () {
            element = item.getElement();
            element.classList.toggle("Wide", model.width == 0);
            element.classList.toggle("SingleRow", model.height == 1);

            p = document.createElement("p");
            p.classList.toggle("Label", model.width > 0)
            p.classList.toggle("Controls2", model.width == 2);
            element.appendChild(p);

            ellipsis = document.createElement("a");
            ellipsis.classList.add("Hidden");
            ellipsis.classList.add("Ellipsis");
            element.appendChild(ellipsis);

            var i = document.createElement("i");
            i.classList.add("fa");
            i.classList.add("fa-ellipsis-h");
            i.addEventListener("click", showMore);
            ellipsis.appendChild(i);
        };

    this.update = function (model) {
		p.innerHTML = model.descriptionValue ? model.descriptionValue.split('\n').join('<br />') : "(no description)";
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
        ellipsis.classList.toggle("Hidden", model.height == 0 || p.offsetHeight >= p.scrollHeight);
    };

    this.setColor = function () {
        p.style.color = model.color;
    };

    this.setFontSize = function () {
        p.style.fontSize = model.fontSize && model.fontSize + "px";
    };

    setup();
}