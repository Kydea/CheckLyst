matematici.Paginator = function (selector, handler) {
	var
		element = $(selector),
        options = {},

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
		},

		addInput = function () {
			var input = $("<input />").attr("placeholder", "page");
            input.on("blur", function () {
				var value = parseInt($(this).val());
				if (value < 1) value = 1;
				if (value > options.pages) value = options.pages;
				
				if (isNaN(value))
					input.val("");
				else
				{
					input.val(value);
					handler(value);
				}
            });
			element.append(input);
			element.append($("<i />").addClass("Icon fas fa-arrow-right"));
		};

	this.setPage = function (page, pages) {
		element.empty();

		options.page = page;
		options.pages = pages;
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

			addInput();
		} 
	};

	this.initialize = function () {
		setup();
	};
};
