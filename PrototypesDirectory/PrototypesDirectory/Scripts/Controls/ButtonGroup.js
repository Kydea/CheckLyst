matematici.ButtonGroup = function (selector) {
	var
		buttonGroup = this,
		element = $(selector),
		button,
		buttonsByLabel = {},
		buttonsByClass = {},

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

		add = function (label, action, cssClass, icon) {
			var button = $("<a />").addClass(cssClass || "").html(label).prependTo(element).on("click", function (e) {
				e.stopPropagation();
				e.preventDefault();
				close();
				action();
			});
			if (icon) button.append($("<i />").addClass("Icon fa fa-" + icon));

			if (label) buttonsByLabel[label] = button;
			if (cssClass) buttonsByClass[cssClass] = button;
			return this;
		},

		toggleByLabel  = function (lable, value) {
            if (value)
				buttonsByLabel[lable].show();
			else
				buttonsByLabel[lable].hide();
		},

		toggleByClass = function (cssClass, value) {
			if (value)
				buttonsByClass[cssClass].show();
			else
				buttonsByClass[cssClass].hide();
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

	buttonGroup.add = add;
	buttonGroup.toggleByLabel = toggleByLabel;
	buttonGroup.toggleByClass = toggleByClass;

	initialize();
}