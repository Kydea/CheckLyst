prototypes.Signature = function (item, model) {
    var
        button,
        label,
        element = item.getElement(),

		setup = function () {
			label = $("<label />").addClass("Controls3").append($("<span />").html(model.description ? model.description : "(no description)"));
			element.append(label);
			$("<i />").addClass("Icon fal fa-user").appendTo(label);
			$("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(label);
			$("<i />").addClass("Icon fal fa-calendar-alt").appendTo(label);
			$("<input />").addClass("Date").attr({ type: "text", readonly: true }).appendTo(label);
			$("<i />").addClass("Icon fal fa-clock").appendTo(label);
			$("<input />").addClass("Time").attr({ type: "text", readonly: true }).appendTo(label);
            button = $("<a />").addClass("Button SignatureButton Placeholder").attr({ href: "#" }).appendTo(element);
        };

	this.update = function (model) {
		button.text(model.buttonLabel ? model.buttonLabel : "Signature").prepend($("<i >").addClass("Icon fa fa-check"));
        this.setColor(model.color);
        this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        label.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}