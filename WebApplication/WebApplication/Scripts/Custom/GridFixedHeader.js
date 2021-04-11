matematici.GridFixedHeader = function (table) {
    var
        gridFixedHeader = this,
        scrollablePanel,
        container,
        tableHeader,

        resize = function () {
            container.height($("> table > thead > tr", container).height());
            container.css("width", "calc(100% - " + (scrollablePanel[0].offsetWidth - scrollablePanel[0].clientWidth) + "px");
            tableHeader.width(table.width());
        },

        refresh = function () {
            if (tableHeader) tableHeader.remove();
            container.append(tableHeader = table.clone());
            resize();
        },

        scroll = function () {
            container.scrollLeft(scrollablePanel.scrollLeft());
        },

        setup = function () {
            scrollablePanel = table.parent();
            scrollablePanel.data("GridFixedHeader", gridFixedHeader);
            scrollablePanel.prev().filter("div.GridFixedHeader").remove();
            container = $("<div />").addClass("GridFixedHeader").insertBefore(scrollablePanel);
            refresh();
            scrollablePanel.on("scroll", scroll);
        };

    gridFixedHeader.resize = resize;
    gridFixedHeader.refresh = refresh;

    setup();
}