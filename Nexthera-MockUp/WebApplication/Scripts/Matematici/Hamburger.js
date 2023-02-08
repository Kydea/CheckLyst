matematici.Hamburger = function (selector, handler) {
    var 
        element = $(selector),
        
        setup = function () {
            element.on("click", function () {
                element.toggleClass("is-active");
                handler && handler();
            })
        };

    this.toggle = function (value) {
        element.toggleClass("is-active", value);
    };

    setup();
};