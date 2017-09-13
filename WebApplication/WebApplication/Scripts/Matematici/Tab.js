matematici.Tab = function(selector) {

    var
        element = $(selector),
        currentIndex,
        currentLabel,
        currentTab,
        handlers = [],

        resize = function () {
            element.height(currentLabel.height() + currentTab.height());
        },

        select = function (index) {
            currentIndex = index;
            currentLabel = $(".TabLabels a", element).removeClass("Selected").eq(currentIndex).addClass("Selected"),
            currentTab = $(".Tab", element).removeClass("Selected").eq(index).addClass("Selected");

            resize();
        },

        setup = function () {
            element
                .each(function () {
                    var current = $(".TabLabels a.Selected", this);
                    select(current.size() == 0 ? 0 : current.prevAll().size());
                })
                .on("click", ".TabLabels a", function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    select($(this).prevAll().size());

                    for (var index in handlers)
                        handlers[index].call(this, currentIndex);
                });
        };

    this.getElement = function () {
        return element;
    };

    this.getCurrentIndex = function () {
        return currentIndex;
    };

    this.addHandler = function (handler) {
        handlers.push(handler);
        return this;
    };

    this.select = function (index) {
        select(index);
    };

    this.add = function (label) {
        $(".TabLabels", element).append($("<a />").attr("href", "#").append(label));
    };

    this.clear = function () {
        $(".TabLabels", element).empty();
    };

    setup();
};