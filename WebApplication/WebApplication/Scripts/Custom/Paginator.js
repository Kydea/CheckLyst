matematici.Paginator = function (selector, grid, handler) {
	var
		element = $(selector),
		settingsButton,
        options = {},

		initialize = function () {
			settingsButton =
				$("<a />")
					.attr("title", "grid settings")
					.addClass("Settings")
					.append($("<i />").addClass("fal fa-cog"));

			element.on("click", "a", function (e) {
				e.preventDefault();
				var anchor = $(this);
				if (anchor.hasClass("Current")) return;

				if (anchor.hasClass("Settings"))
					new matematici.GridSettingsPopup(grid).show();
				else {
					$(".Current", element).removeClass("Current");
					anchor.addClass("Current");
					handler(anchor.data("value"));
				}
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
		element.append(settingsButton);

		options.page = page;
		options.pages = pages;
		if (pages == 0)
			addPage(0, 0);
		else
		{
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

	this.initialize = initialize;
};
