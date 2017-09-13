instances.Text = function (item, model) {
    var
        p,

        setup = function () {
            p = $("<p />").addClass("Description").appendTo(item.getElement());
        };

    this.update = function (model) {
        p.html(model.Description ? model.Description.split('\n').join('<br />') : "(no description)");
        this.setColor(model.Color);
        this.setFontSize(model.FontSize);
    };

    this.setColor = function (value) {
        p.css({ color: model.Color });
    };

    this.setFontSize = function (value) {
        p.css({ fontSize: model.FontSize == 0 ? "" : model.FontSize + "px" });
    };

    setup();
}