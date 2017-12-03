prototypes.Signature = function (item, model) {
    var
        user,
        date,
        time,
        button,
        label,
        element = item.getElement(),

        setup = function () {
            label = $("<label />").text(model.description ? model.description : "(no description)");
            element.append(label);
			user = $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(label);
			$("<i />").addClass("fi flaticon-user-o").insertAfter(user);
			date = $("<input />").addClass("Date").attr({ type: "text", readonly: true }).appendTo(label);
			$("<i />").addClass("fi flaticon-calendar").insertAfter(date);
			time = $("<input />").addClass("Time").attr({ type: "text", readonly: true }).appendTo(label);
			$("<i />").addClass("fi flaticon-clock-o").insertAfter(time);
			button = $("<a />").addClass("Button").attr({ href: "#" }).appendTo(label);
        };

    this.update = function (model) {
        button.text(model.buttonLabel ? model.buttonLabel : "Signature");
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