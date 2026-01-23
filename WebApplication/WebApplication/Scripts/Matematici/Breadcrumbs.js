matematici.Breadcrumbs = function (selector) {
	var
		breadcrumbs = this,
		fixedElement,
		element = $(selector),
		top = element.offset().top,
		commandHandler,

		onScroll = function () {
			var scrolled = top - matematici.elements.document.scrollTop() < -10;
			fixedElement.toggleClass("Visible", scrolled);
		},

		setup = function () {
			matematici.elements.window.on("resize", onScroll);
			matematici.elements.document.on("scroll", onScroll);
		};

	breadcrumbs.set = function (items) {
		element.empty();
		for (index in items) {
			var item = items[index];
			if (item.element)
				element.append(item.element);
			else {
				element.append($("<span />").html(item.label || "page").addClass("Extended Label"));
				var span = (item.url == null ? $("<span />") : $("<a />").attr("href", item.url)).html(item.content);
				element.append(span);
				if (item.cssClass) span.addClass(item.cssClass);
			}
		}

		if (fixedElement) fixedElement.remove();
		fixedElement = element.clone().addClass("Fixed").insertBefore(element);
		fixedElement.on("click", "i", function () {
            if (commandHandler) commandHandler(this);
		});
		onScroll();
	};

	breadcrumbs.setCommandHandler = function (handler) {
		commandHandler = handler;
	};

	setup();
};