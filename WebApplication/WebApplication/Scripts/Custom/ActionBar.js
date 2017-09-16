matematici.ActionBar = function (selector) {
    var
        element = $(selector),
        buttons = [],

        setup = function () {
            element.append($("<div />").addClass("Clearing"));
        };

    this.add = function (label, action, cssClass) {
        $("<a />").addClass("Button " + (cssClass || "")).html(label).prependTo(element).on("click", action);
        return this;
    };

    this.getElement = function () {
        return element;
    };

    setup();
}