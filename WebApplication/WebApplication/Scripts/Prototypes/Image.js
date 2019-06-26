prototypes.Image = function (item, model) {
    var
        span,
        a,

        setup = function () {
            span = $("<span />");
            a = $("<a />").addClass("Image Button GreyYellowButton").append(span).append($("<i />").addClass("fi flaticon-arrow-right")).appendTo(item.getElement());
        };

    this.update = function (model) {
        span.html(model.description ? model.description : "show image");

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        a.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        a.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}