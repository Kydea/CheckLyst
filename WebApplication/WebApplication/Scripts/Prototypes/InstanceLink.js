prototypes.InstanceLink = function (item, model) {
    var
        label,
        element = item.getElement(),

        setup = function () {
            label = $("<label />").addClass("Controls3").append($("<span />").html(model.description || "(no description)"));
            element.append(label);
            $("<i />").addClass("Icon fal fa-key").appendTo(label);
            $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(label);
            $("<i />").addClass("Icon fal fa-layer-group").appendTo(label);
            $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(label);
            $("<i />").addClass("Icon fal fa-file-invoice").appendTo(label);
            $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(label);
            $("<a />")
                .addClass("Button GreyYellowButton  Placeholder")
                .attr({ href: "#" })
                .appendTo(element)
                .text("Actions")
                .prepend($("<i >").addClass("Icon fal fa-cog"));
        };

    this.update = function (model) {
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function () {
        label.css({ color: model.color });
    };

    this.setFontSize = function () {
        label.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}