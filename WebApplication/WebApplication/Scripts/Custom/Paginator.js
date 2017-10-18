matematici.Paginator = function (selector, handler) {
	var
		element = $(selector),

		setup = function () {
			element.on("click", "a", function (e) {
				e.preventDefault();
				if ($(this).hasClass("Current")) return;
				$(".Current", element).removeClass("Current");
				$(this).addClass("Current");

				handler($(this).data("value"));
			});
		},

		addPage = function (index, page) {
			var a = 
				$("<a />")
					.data("value", index)
					.text(index)
					.appendTo(element);

			if (index == page) a.addClass("Current");
		},

		addSeparator = function () {
			element.append($("<span />").text("..."));
		};

	this.setPage = function (page, pages) {
		element.empty();

		if (pages > 1) {
			var ratio = 3; //3 pages after and 3 pages before the current
			if (page > ratio + 1) {
				addPage(1, page);
				if (page > ratio + 2)
					addSeparator();
			}

			for (index = page - ratio; index <= page + ratio; ++index)
				if (index >= 1 && index <= pages)
					addPage(index, page);

			if (page < pages - ratio) {
				if (page < pages - ratio - 1) addSeparator();
				addPage(pages, page);
			}
		} 
	};

	this.initialize = function () {
		setup();
	};
};
