matematici.FixedHeader = function (selector) {
    var
        grid = $(selector),
        header = $("thead tr", grid),
        headerCells = $("thead tr th", grid),
        fixedHeader = $("<div />").addClass("FixedHeader"),
        fixedHeaderCells,
        initialized,

        scroll = function () {
            fixedHeader.css("display", matematici.elements.window.scrollTop() > grid.offset().top ? "table" : "none");
        },

        resize = function () {
            fixedHeader.css({
                height: header.css("height"),
                left: header.offset().left,
                width: header.css("width"),
            });
            headerCells.each(function () {
                var
                    cell = $(this),
                    fixedCell = cell.data("fixedCell");

                fixedCell.css({
                    width: cell.css("width"),
                    height: cell.css("height")
                });
            });

            scroll();
        };

    this.initialize = function () {
        if (grid.size() == 0 || !grid.is(":visible")) return;
        fixedHeader.insertBefore(grid);
        headerCells.each(function () {
            var cell = $(this);
            cell.data("fixedCell", $("<div />").appendTo(fixedHeader).html(cell.html()))
        });
        fixedHeaderCells = $("div", fixedHeader);
        matematici.elements.window.on("resize", resize);
        matematici.elements.window.on("scroll", scroll);
        resize();
        initialized = true;
    };

    this.destroy = function () {
        if (initialized) {
            matematici.elements.window.off("resize", resize);
            matematici.elements.window.off("scroll", scroll);
        }
    };

    this.resume = function () {
    };

    matematici.fixedHeader = this;
}