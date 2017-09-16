prototypes.Signature = function (item, model) {
    var
        user,
        date,
        time,
        button,
        label,
        element = item.getElement(),

        setup = function () {
            label = $("<label />").text(model.Description ? model.Description : "(no description)");
            element.append(label);
            user = $("<input />").addClass("User").attr({ type: "text", readonly: true }).appendTo(element);
            $("<i />").addClass("fa fa-user").insertAfter(user);
            date = $("<input />").addClass("Date").attr({ type: "text", readonly: true }).appendTo(element);
            $("<i />").addClass("fa fa-calendar").insertAfter(date);
            time = $("<input />").addClass("Time").attr({ type: "text", readonly: true }).appendTo(element);
            $("<i />").addClass("fa fa-clock-o").insertAfter(time);
            button = $("<a />").addClass("Button").attr({ href: "#" }).appendTo(element);
        };

    this.update = function (model) {
        button.text(model.ButtonLabel ? model.ButtonLabel : "Signature");
        this.setColor(model.Color);
        this.setFontSize(model.FontSize);
    };

    this.setColor = function (value) {
        label.css({ color: model.Color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.FontSize == 0 ? "" : model.FontSize + "px" });
    };

    setup();
}