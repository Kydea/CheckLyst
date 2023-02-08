matematici.AutoScroller = function () {
    var
        setup = function () {
            $(document).on("click", function (e) {
                if (e.pageX < 30 && e.pageX > 0)
                    matematici.elements.body.animate({ scrollTop: e.pageY - matematici.elements.window.height() * .3 }, 500);
            });
        };

    setup();
}