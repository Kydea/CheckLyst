instances.Text = function (item, model) {
    var
        p,

        setup = function () {
            p = $("<p />").addClass("Description").appendTo(item.getElement());
        };

    this.update = function (model) {
		p.html(model.descriptionValue ? model.descriptionValue.split('\n').join('<br />') : "(no description)");
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        p.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        p.css({ fontSize: model.fontSize && model.fontSize + "px" });
    };

    setup();
}