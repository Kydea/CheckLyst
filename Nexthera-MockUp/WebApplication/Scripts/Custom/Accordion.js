matematici.Accordion = function (bar) {
	var
		accordion = this,
		panel = bar.next(),
		button,

		setup = function () {
			button = $("<a />").text(bar.data("text") || "filters").addClass("Button").attr("href", "#").append($("<i />").addClass("Icon fa fa-caret-down"));
			bar.removeClass("Accordion").addClass("Accordion-Bar");
			bar.append(button);

			panel.addClass("Accordion");

			button.on("click", function () {
				click();
			});

			if (button.hasClass("Closed"))
				accordion.close();
		},

		click = function () {
			if (button.hasClass("Closed"))
				accordion.open();
			else
				accordion.close();
		};

	accordion.close = function () {
		panel.slideUp();
		button.addClass("Closed");
	};

	accordion.open = function () {
		panel.slideDown();
		button.removeClass("Closed");
	};

	setup();
}

matematici.initializers.push(function (context) {
	$("div.Accordion", context).each(function () {
		new matematici.Accordion($(this));
	});
});
