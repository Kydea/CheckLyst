matematici.Breadcrumbs = function (selector) {
	var
		breadcrumbs = this,
		fixedElement,
		element = $(selector),
		top = element.offset().top,

		onScroll = function () {
			var scrolled = top - matematici.elements.document.scrollTop() < -10;
			scrolled = true;
			fixedElement.toggleClass("Visible", scrolled);
		},

		setup = function () {
			matematici.elements.window.on("resize", onScroll);
			matematici.elements.document.on("scroll", onScroll);
		};

	breadcrumbs.set = function (lines) {
		element.empty();
		var position;
		for (lineIndex in lines) {
			var
				line = lines[lineIndex],
				count = line.items && line.items.length || 0,
				container = $("<div />").appendTo(element).addClass(count == 0 ? "" : count == 1 ? "OneChild" : count == 2 ? "TwoChildren" : "ManyChildren");

			for (itemIndex in line.items) {
				var
					item = line.items[itemIndex],
					label = (item.url ? $("<a />").attr("href", item.url) : $("<span />")).html(item.label);
				container.append(label);
				if (count == 1 && position != null)
					label.css("left", position - label.outerWidth() / 2 + "px");
				if (item.selected) {
					label.addClass("Strong");
					position = label.offset().left - 100 + label.outerWidth() / 2;
				}
			}
		}
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