/// estendere per lista da web service

matematici.AutoComplete = function (selector) {
	var
		autoComplete = this,
		element = $(selector).addClass("AutoComplete"),
		options = $("option", element),
		items,
		currentItem,
		label,
		input,
		list,
		icon,
		noRow,

		setValue = function () {
			var value = element.val();
			items.each(function (index) {
				if ($(this).data("value") == value) {
					setCurrentItem(index);
					return false;
				}
			});
		},

		setWidth = function () {
			var width = label.outerWidth(), position = label.position();
			input.width(width - parseInt(input.css("padding-left")) - parseInt(input.css("padding-right")) - 2);
			items.each(function () {
				var itemWidth = $(this).outerWidth();
				if (itemWidth > width)
					width = itemWidth;
			});
			list.outerWidth(width);

            var labelPos = {
                'left': position.left + parseInt(label.css('marginLeft'), 10),
                'top': position.top + label.outerHeight(true)
            };

            list.css(labelPos);
            input.css(labelPos);

            icon.css({
                'left': labelPos.left + width - 20,
                'top': position.top + parseInt(label.css('marginTop'), 10)
            });
		},

		initialize = function () {
			element.data("autoComplete", autoComplete);
			label = $("<input />").addClass("AutoComplete").attr({ type: "text" }).insertAfter(element);
			var width = label.outerWidth();
			input = $("<input />").addClass("AutoComplete-Filter").attr({ type: "text" }).hide().insertAfter(label);
			list = $("<div />").addClass("AutoComplete-List").insertAfter(input);
			icon = $("<i />").addClass("AutoComplete fi flaticon-freccia-giu").insertAfter(input);
			noRow = $("<i />").addClass("fa fa-minus");
			label
				.on("focus", function () {
					showList();
					input.focus();
					input.select();
				})
				.on("click", function (e) {
					e.stopPropagation();
				});
			input
				.on("keydown", function (e) {
					if (e.which == 38 || e.which == 40) { //up and down
						e.preventDefault();
						setCurrentItem(currentItem + (e.which == 38 ? -1 : 1) || 0);
					} else if (e.which == 13) { //return
						e.preventDefault();
						e.stopPropagation();
						hideList();
					}

				})
				.on("keyup", function (e) {
					if (e.which < 37 || e.which > 40) { //arrow keys
						updateList();
						setCurrentItem(0);
						showList();
					}
				});
			list
				.on("click", "a", function (e) {
					e.preventDefault();
					setCurrentItem($(this).prevAll().size());
					hideList();
				})
				.on("click", function () {
					hideList();
				});
			icon
				.on("click", function (e) {
					e.stopPropagation();
					input.focus();
				});
			updateList();
			setValue();

			matematici.elements.window.on("resize", setWidth);
		},

		destroy = function () {
			matematici.elements.window.off("resize", setWidth);
		},

		setCurrentItem = function (value) {
			if (currentItem != null) items.eq(currentItem).removeClass("Selected");
			currentItem = items.size() == 0 ? null : value < 0 ? 0 : items.size() <= value ? items.size() - 1 : value;
			if (currentItem != null) {
				var
					item = items.eq(currentItem).addClass("Selected"),
					itemHeight = item.outerHeight(),
					scroll = list.scrollTop(),
					listHeight = list.height(),
					top = currentItem * itemHeight;
				if (top < scroll)
					list.scrollTop(top);
				else if (top + itemHeight - listHeight > scroll)
					list.scrollTop(top + itemHeight - listHeight);

				element.val(item.data("value"));
				label.val(item.text());
			}
		},

		showList = function () {
			label.addClass("Open");
			input.show();
			list.show();
			setValue();
			setTimeout(function () {
				$(document).on("click.autocomplete", hideList)
				$(document).on("keydown.autocomplete", function (e) {
					if (e.which == 13)
						hideList();
				});
			}, 0);
		},

		hideList = function () {
			$(document)
				.off("click.autocomplete")
				.off("keydown.autocomplete");
			list.hide();
			input.hide();
			setCurrentItem(null);
			label.removeClass("Open");
		},

		updateList = function () {
			list.empty();
			var count = 0;
			options
				.each(function () {
					var
						option = $(this),
						filter = input.val().toUpperCase();
					if (filter == "" || option.text().toUpperCase().indexOf(filter) >= 0) {
						list.append($("<a />").attr("href", "#").data("value", option.attr("value")).text(option.text()));
						++count;
					}
				});
			items = $("a", list);
			if (items.size() == 0)
				list.append(noRow);
			setWidth();
		};

	this.refresh = function () {
		options = $("option", element);
		updateList();
		setValue();
	};

	this.getControl = function () {
		return label;
	};

	this.initialize = function () {
		initialize();
	};

	this.destroy = function () {
		destroy();
	};
};

matematici.initializers.push(function (context) {
	$("select", context).each(function () {
		new matematici.AutoComplete(this).initialize();
	});
});

matematici.destroyers.push(function (context) {
	$("select", context).each(function () {
		$(this).data("autoComplete").destroy();
	});
});
