prototypes.Link = function (item, model) {
    var
        i, a,

        setup = function () {
            i = $("<i >").addClass("Link fi flaticon-arrow-right").appendTo(item.getElement());
            a = $("<a />").addClass("Link").appendTo(item.getElement());
        };

    this.update = function (model) {
        a
            .html(model.description ? model.description : "(no label)")
            .attr("href", model.url);

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        i.css({ color: model.color });
        a.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        a.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}