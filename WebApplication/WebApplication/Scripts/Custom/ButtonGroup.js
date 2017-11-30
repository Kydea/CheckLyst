matematici.ButtonGroup = function (selector) {
	var
		buttonGroup = this,
		element = $(selector),
		button,

		open = function () {
			element.show();
			matematici.elements.body.on("click.ButtonGroup", function () { close(); });
			button.addClass("Open");
		},

		close = function () {
			matematici.elements.body.off("click.ButtonGroup");
			element.hide();
			button.removeClass("Open");
		},

		initialize = function () {
			button = $("<a />")
				.attr("href", "#")
				.addClass("Button ButtonGroup")
				.text(element.data("text"))
				.prepend($("<i />").addClass("fi flaticon-arrow-down"))
				.on("click", function (e) {
					e.preventDefault();
					e.stopPropagation();
					if (element.is(":visible"))
						close();
					else
						open();
				})
				.on("mouseover", function () {
					element.addClass("Hover");
				})
				.on("mouseout", function () {
					element.removeClass("Hover");
				})
				.insertBefore(element);

			element
				.data("buttonGroup", buttonGroup)
				.on("click", function () {
					close();
				});

		},

		destroy = function () {
			close();
		};

	this.initialize = initialize;

	this.destroy = destroy;
}

matematici.initializers.push(function (context) {
	$("div.ButtonGroup", context).each(function () {
		new matematici.ButtonGroup($(this)).initialize();
	});
});

matematici.destroyers.push(function (context) {
	$("div.ButtonGroup", context).each(function () {
		$(this).data("buttonGroup").destroy();
	});
});
