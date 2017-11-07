matematici.Accodion = function (bar) {
	var
		accordion = this,
		button = $("a.Button", bar),
		panel = bar.next(),

		setup = function () {
			panel.addClass("Accordion");

			button.on("click", function () {
				console.log("OK")
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
	$("div.Accordion-Bar", context).each(function () {
		new matematici.Accodion($(this));
	});
});
