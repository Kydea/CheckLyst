matematici.AutoCenter = function (selector) {
    var
        autoCenter = this,
        element = $(selector).data("autoCenter", autoCenter),
        scrollablePanel,

        center = function () {
            if (scrollablePanel) {
                var maxHeight =
                    80 //padding + margin
                    + scrollablePanel.siblings("h2").outerHeight()
                    + scrollablePanel.siblings(".GridFiltersBar").outerHeight()
                    + scrollablePanel.parent().siblings(".Buttons").outerHeight()
                scrollablePanel.css("max-height", "calc(100vh - " + maxHeight + "px)");
            }
            element.css("top", (window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) < 760 ? 0 : Math.max(parseInt((element.parent().height() - element.outerHeight()) / 2), 0));
        },

        initialize = function () {
            matematici.elements.window.on("resize", center);
            scrollablePanel = element.hasClass("Container") && element.parent().hasClass("CheckListGrid") ? element.children().first().children().filter(".ScrollablePanel") : null;
            if (scrollablePanel && scrollablePanel.length == 0) scrollablePanel = null;
            center();
            element.show();
        },

        destroy = function () {
            matematici.elements.window.off("resize", center);
        };

    this.initialize = function () {
        setTimeout(initialize, 0);
    };

    this.destroy = destroy;
    this.center = center;
}

matematici.initializers.push(function (context) {
    $(".AutoCenter", context).each(function () {
        new matematici.AutoCenter(this).initialize();
    });
});

matematici.destroyers.push(function (context) {
    $(".AutoCenter", context).each(function () {
        $(this).data("autoCenter").destroy();
    });
});
