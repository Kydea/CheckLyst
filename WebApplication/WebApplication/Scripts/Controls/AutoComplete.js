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
			label.width(element.width());
			var width = label.outerWidth();
			input.outerWidth(width);
			list.outerWidth(width);
		},

		setup = function () {
			element.data("autoComplete", autoComplete);
			label = $("<input />").addClass("AutoComplete").attr({ type: "text" }).insertAfter(element);
			var width = label.outerWidth();
			input = $("<input />").addClass("AutoComplete-Filter").attr({ type: "text" }).hide().insertAfter(label);
			list = $("<div />").addClass("AutoComplete-List").insertAfter(input);
			label
				.on("focus", function () {
					if (input.val() == "")
						updateList();
					showList();
					label.select();
					input.focus();
				})
				.on("click", function (e) {
					e.stopPropagation();
				});
			input
				.on("keydown", function (e) {
					if (e.which == 38 || e.which == 40) { //up and down
						e.preventDefault();
						setCurrentItem(currentItem + (e.which == 38 ? -1 : 1) || 0);
					}
				})
				.on("keyup", function (e) {
					if (e.which < 37 || e.which > 40) { //arrow keys
						updateList();
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
			$("<i />").addClass("AutoComplete fi flaticon-freccia-giu").insertAfter(input).css("margin-left", width - 20)
				.on("click", function (e) {
					e.stopPropagation();
					input.focus();
				});
			updateList();
			setWidth();
			setValue();
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
			setCurrentItem(0);
		};

	this.refresh = function () {
		options = $("option", element);
		updateList();
		setWidth();
		setValue();
	};

	this.getControl = function () {
		return label;
	};

	setup();
};

matematici.initializers.push(function (context) {
	$("select", context).each(function () { new matematici.AutoComplete(this); });
});
