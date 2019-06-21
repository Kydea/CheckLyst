matematici.Tab = function(selector, options) {
    var
        tab = this,
        element = $(selector),
        currentItem,
        handlers = [],

        getLabelsHeight = function () {
            return $("> .TabLabels", element).outerHeight();
        },

		getCurrentItemHeight = function () {
            return getLabelsHeight() + currentItem.tab.height() + 3;
		},

		resize = function () {
			element.show();
			if (options.fixedHeight) {
                var
                    labelsHeight = getLabelsHeight();
					tagHeight = 0,
					currentIndex = currentItem && currentItem.index || 0;

                $("> .Tab", element).css("margin-top", labelsHeight);

                $("> .TabLabels a", element).each(function (index) {
                    var item = select(index);
                    tagHeight = Math.max(tagHeight, getCurrentItemHeight());
                    item.tab.hide();
                });

				select(currentIndex);
                element.height(labelsHeight);
			} else
				element.height(getCurrentItemHeight());
		},

		getItem = function (index) {
			return {
				index: index,
				label: $("> .TabLabels a", element).eq(index),
				tab: $("> .Tab", element).eq(index)
			}
		},

        setItem = function (index) {
			var item = {
				index: index,
                label: $("> .TabLabels a", element).removeClass("Selected").eq(index).addClass("Selected"),
                tab: $("> .Tab", element).removeClass("Selected").eq(index).addClass("Selected").show()
			}
            setTimeout(function () { $("> .Tab:not(.Selected)", element).hide(); }, 500);
            return item;
		},

		select = function (index) {
			currentItem = setItem(index);
			if (!options.fixedHeight)
				resize();

			$("select.AutoComplete").each(function (index) {
				var
					select = $(this),
					autoComplete = select.data("autoComplete");
				if (autoComplete) autoComplete.refresh();
            });

            return currentItem;
		},

		setup = function () {
			if (!options) options = {};
			select($("> .TabLabels a.Selected", element).prevAll("a").size());

			element
				.data("tab", tab)
                .on("click", "> .TabLabels a", function (e) {
                    e.preventDefault();
                    //e.stopPropagation();

                    select($(this).prevAll("a").size());

                    for (var index in handlers)
                        handlers[index].call(this, currentItem);
                });

            $("> .TabLabels a", element).each(function() {
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

	this.getLabels = function () {
		return $("> .TabLabels a", element);
	};

    this.addHandler = function (handler) {
        handlers.push(handler);
        return this;
    };

    this.select = function (index) {
        select(index);
    };

	this.add = function (label, id, cssClass, icon) {
		var label = $("<a />").attr({ href: "#", id: id }).addClass(cssClass).append(label);
		if (icon) label.append($("<i />").addClass("fi flaticon-" + icon));
		$("> .TabLabels", element).append(label);
    };

    this.clear = function () {
        $("> .TabLabels", element).empty();
	};

	this.resize = function () {
		resize();
	};

    setup();
};