matematici.Breadcrumbs = function (selector) {
	var
		breadcrumbs = this,
		fixedElement,
		element = $(selector),
		top = element.offset().top,
		position,

		onScroll = function () {
			var scrolled = top - matematici.elements.document.scrollTop() < -10;
			scrolled = true;
			fixedElement.toggleClass("Visible", scrolled);
		},

		setup = function () {
			matematici.elements.window.on("resize", onScroll);
			matematici.elements.document.on("scroll", onScroll);
		},

		addItems = function (items) {
			var
				count = items && items.length || 0,
				container = $("<div />").appendTo(element).addClass(count == 0 ? "" : count == 1 ? "OneChild" : count == 2 ? "TwoChildren" : "ManyChildren"),
				selectedItem;
			//$("<i />").addClass("Point").appendTo(container).css({ left: position, top: 0 }).text("note");

			for (itemIndex in items) {
				var
					item = items[itemIndex],
					label = (item.url ? $("<a />").attr("href", item.url) : $("<span />")).html(item.label);
				container.append(label);
				if (count == 1 && position != null) {
					label.css("left", position - 15 - label.outerWidth() / 2 + "px");
				} if (item.selected) {
					label.addClass("Strong " + label.outerWidth());
					position = label.offset().left - 70 + label.outerWidth() / 2;
					selectedItem = item;
				}
			}

			if (selectedItem && selectedItem.items)
				addItems(selectedItem.items);

		};

	breadcrumbs.set = function (items) {
		position = null;

		element.empty();

		addItems(items);
		
		//$("<div />").appendTo(element);
		if (fixedElement) fixedElement.remove();
		fixedElement = element.clone().addClass("Fixed").insertBefore(element);
		onScroll();
		//fixedElement.toggleClass("Visible", false);
	};

	this.getElement = function () {
		return element;
	}

	setup();
};