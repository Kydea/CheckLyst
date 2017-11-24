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

        select = function (index) {
			currentItem = {
				index: index,
				label: $(".TabLabels a", element).removeClass("Selected").eq(index).addClass("Selected"),
				tab: $(".Tab", element).removeClass("Selected").eq(index).addClass("Selected")
			}

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
        };

    this.getElement = function () {
        return element;
    };

    this.getCurrentItem = function () {
        return currentItem;
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