matematici.blip = function () {

	var
		element,

		show = function (options) {
			element = $("<div />")
				.addClass("Blip " + (options.cssClass || ""))
				.appendTo(matematici.elements.body)
				.on("click", close);
			var container = $("<div />").addClass("Container").appendTo(element);
			container.append($("<i />").addClass("fi flaticon-" + options.icon));
			matematici.elements.body.addClass("Blip");
			element.fadeIn(300);
			//setTimeout(close, 2000);
		},

		close = function () {
			element.fadeOut(300, "linear", function () {
				element.remove();
				matematici.elements.body.removeClass("Blip");
				options = element = null;
			});
		};

	return {
		show: function (options) {
			show(options);
		},

		ok: function () {
			show({ icon: "check" });
		},

		ko: function () {
			show({ cssClass: "Error", icon: "exclamation" });
		}
	};
}();