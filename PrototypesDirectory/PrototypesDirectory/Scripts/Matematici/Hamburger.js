matematici.Hamburger = function (selector) {
    var 
        element = $(selector),
        
        setup = function () {
            element.on("click", function () {
                element.toggleClass("is-active");
            })
        };

    this.toggle = function (value) {
        element.toggleClass("is-active", value);
    };

    setup();
};