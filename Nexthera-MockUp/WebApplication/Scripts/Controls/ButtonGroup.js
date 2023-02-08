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
				.prepend($("<i />").addClass("Icon fa fa-caret-down"))
				.on("click", function (e) {
					e.preventDefault();
					e.stopPropagation();
					if (element.is(":visible"))
						close();
					else
						open();
				})
				.insertBefore(element);

			element
				.data("buttonGroup", buttonGroup)
				.on("click", function () {
					close();
				})
				.appendTo(button);

		};

	buttonGroup.add = function (label, action, cssClass, icon) {
		var button = $("<a />").addClass(cssClass || "").html(label).prependTo(element).on("click", function (e) {
			e.stopPropagation();
			e.preventDefault();
			close();
			action();
		});
		if (icon) button.append($("<i />").addClass("Icon fa fa-" + icon));
		return this;
	};

	initialize();
}