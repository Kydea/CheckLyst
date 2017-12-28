matematici.Breadcrumbs = function (selector) {
	var
		breadcrumbs = this,
		fixedElement,
		element = $(selector),
		top = element.offset().top,

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
            if (item.content == null)
                console.log(new Error("content not found in bredcrumbs item"));

            element.append((item.url == null ? $("<span />") : $("<a />").attr("href", item.url)).html(item.content));
		}

		if (fixedElement) fixedElement.remove();
		fixedElement = element.clone().addClass("Fixed").insertBefore(element);
		fixedElement.toggleClass("Visible", false);
	}

	setup();
};