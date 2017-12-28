instances.Text = function (item, model) {
    var
        p,

        setup = function () {
            p = $("<p />").addClass("Description").appendTo(item.getElement());
        };

    this.update = function (model) {
        p.html(model.description ? model.description.split('\n').join('<br />') : "(no description)");
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        p.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        p.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}