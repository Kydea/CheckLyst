matematici.Tab = function(selector, options) {

	var
		tab = this,
        element = $(selector),
        currentItem,
        handlers = [],

		getCurrentItemHeight = function () {
			return currentItem.label.height() + currentItem.tab.height() + 3;
		},

		resize = function () {
			element.show();
			if (options.fixedHeight) {
				var
					height = 0,
					currentIndex = currentItem && currentItem.index || 0;

				$(".TabLabels a", element).each(function (index) {
					select(index);
					height = Math.max(height, getCurrentItemHeight());
				});

				select (currentIndex);
				element.height(height);
			} else
				element.height(getItemHeight());
        },

        getItem = function(index) {
            return {
                index: index,
                label: $(".TabLabels a", element).removeClass("Selected").eq(index).addClass("Selected"),
                tab: $(".Tab", element).removeClass("Selected").eq(index).addClass("Selected")
            }
        }

        select = function (index) {
            currentItem = getItem(index);
			if (!options.fixedHeight)
				resize();
        },

		setup = function () {
			select($(".TabLabels a.Selected", element).prevAll().size());

			element
				.data("tab", tab)
                .on("click", ".TabLabels a", function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    select($(this).prevAll().size());

                    for (var index in handlers)
                        handlers[index].call(this, currentItem.index);
                });

            $(".TabLabels a", element).each(function() {
                var label = $(this);
                label.attr("title", label.text());
            });
        };

    this.getElement = function () {
        return element;
    };

    this.getCurrentItem = function () {
        return currentItem;
    };

    this.getItem = function(index) {
        return getItem(index);
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

	this.resize = function () {
		resize();
	};

    setup();
};