matematici.ActionBar = function (selector) {
    var
        element = $(selector),
        buttons = [],

        setup = function () {
            element.append($("<div />").addClass("Clearing"));
        };

    this.add = function (label, action, cssClass, icon) {
		var button = $("<a />").addClass("Button " + (cssClass || "")).html(label).prependTo(element).on("click", action);
		if (icon) button.append($("<i />").addClass("fi flaticon-" + icon));
        return this;
    };

    this.getElement = function () {
        return element;
    };

    setup();
}