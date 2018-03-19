prototypes.Field = function (item, model) {
    var
        field,
		label,
		span,
        element = item.getElement(),

        setup = function () {
			label = $("<label />").appendTo(element);
			span = $("<span />").appendTo(label);
			field = $("<div />").addClass("Input").appendTo(label);
			if (model.linesCount > 0) 
				element.addClass("MultiLine");
            if (model.linesCount > 1) {
				field.outerHeight(20 * model.linesCount + 12);
            }
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

    setup();
}