prototypes.Field = function (item, model) {
    var
        field,
        label,
        element = item.getElement(),

        setup = function () {
            label = $("<label />").appendTo(element);
            field = $("<div />").addClass("Input").appendTo(element);
            if (model.LinesCount > 0) {
                element.addClass("MultiLine");
                field.height(24 * model.LinesCount - 10);
            }
        };

    this.update = function (model) {
        label.html(model.Description ? model.Description : "(no description)")
        this.setColor(model.Color);
        this.setFontSize(model.FontSize);
        field.attr("title", field.text());
    };

    this.setColor = function (value) {
        label.css({ color: model.Color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.FontSize == 0 ? "" : model.FontSize + "px" });
    };

    setup();
}