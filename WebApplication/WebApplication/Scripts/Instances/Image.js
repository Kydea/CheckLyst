instances.Image = function (item, model) {
    var
        span,
        a,
        title,

        clickHandler = function () {
            matematici.imageViewer.show(title, "http://www.matematicires.com/Content/Media/7/Home/Slide.Landscape/1.jpg");
        },

        setup = function () {
            span = $("<span />");
            a = $("<a />").addClass("Image Button GreyYellowButton").append(span).append($("<i />").addClass("fa fa-caret-right")).appendTo(item.getElement()).on("click", clickHandler);
        };

    this.update = function (model) {
        span.html(title = model.description ? model.description : "show image");

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