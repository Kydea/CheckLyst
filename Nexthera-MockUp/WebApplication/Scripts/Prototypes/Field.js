prototypes.Field = function (item, model) {
    var
        field,
        label,
        span,
        element = item.getElement(),

        setSize = function () {
            label.toggleClass("Controls2", model.linesCount == -1);
            element.toggleClass("MultiLine", model.linesCount > 0);
            field.outerHeight(model.linesCount <= 1 ? 36 : 20 * model.linesCount + 12);
        },

        setup = function () {
            label = $("<label />").appendTo(element);
            span = $("<span />").appendTo(label);
            field = $("<div />").addClass("Input").appendTo(label);
            setSize();
        };

    this.update = function (model) {
        span.html(model.description ? model.description : "(no description)")
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
        field.attr("title", field.text());
    };

    this.setColor = function (value) {
        label.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    this.setSize = setSize;

    setup();
}