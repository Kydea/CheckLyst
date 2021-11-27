prototypes.Text = function (item, model) {
    var
        p,

        setSize = function () {
            var element = item.getElement();
            element.toggleClass("NewLine", model.newLine);
            element.toggleClass("Wide", model.width == 0);
            element.toggleClass("SingleRow", model.height == 1);
            p.toggleClass("Label", model.width > 0).toggleClass("Controls2", model.width == 2);
            var ellipsis = model.height == 1 && p[0].offsetHeight < p[0].scrollHeight;
            p.toggleClass("Ellipsis", ellipsis);
            p.attr("title", ellipsis ? model.description : "");
        },

        setup = function () {
            p = $("<p />").appendTo(item.getElement());
            setSize();
        };

    this.update = function (model) {
        p.html(model.description ? model.description.split('\n').join('<br />') : "(no description)");
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
        setSize();
    };

    this.setColor = function (value) {
        p.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        p.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    this.setSize = setSize;

    setup();
}