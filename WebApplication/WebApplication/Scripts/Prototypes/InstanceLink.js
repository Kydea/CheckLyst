prototypes.InstanceLink = function (item, model) {
    var
        label,
        element = item.getElement(),

        setup = function () {
            label = $("<label />").append($("<span />").html(model.description ? model.description : "(no description)"));
            element.append(label);
            $("<i />").addClass("Icon fal fa-file-invoice").appendTo(label);
            $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(label);
            $("<a />").addClass("Button InstanceLinkButton Placeholder").attr({ href: "#" }).appendTo(element).text("Create");
            $("<a />").addClass("Button InstanceLinkButton Placeholder").attr({ href: "#" }).appendTo(element).text("Select");
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