instances.Attachment = function (item, model) {
    var
        button,

        setup = function () {
            var input = $("<input />").addClass("Hidden").attr({ type: "file" }).appendTo(item.getElement())
            button =
                $("<a />")
                    .addClass("Button Attachment").attr({ href: "#" }).appendTo(item.getElement())
                    .on("click", function (e) {
                        input.click();
                    });
        };

    this.update = function (model) {
        button
            .html(model.description || "upload")
            .append($("<i />").addClass("Icon fa fa-paperclip"));

        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        button.css({ backgroundColor: model.color });
    };

    this.setFontSize = function (value) {
        button.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}