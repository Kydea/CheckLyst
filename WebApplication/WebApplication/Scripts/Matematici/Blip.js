matematici.blip = function () {

	var
		element,
		callback,

		show = function (options) {
			if (element) close();
			element = $("<div />")
				.addClass("Blip " + (options.cssClass || ""))
				.appendTo(matematici.elements.body)
				.on("click", close);
			var container = $("<div />").addClass("Container").appendTo(element);
			container.append($("<i />").addClass("fal fa-" + options.icon));
			matematici.elements.body.addClass("Blip");
			element.fadeIn(300);
			callback = options.callback;
			//setTimeout(close, 2000);
		},

		close = function () {
			element.fadeOut(300, "linear", function () {
				element.remove();
				matematici.elements.body.removeClass("Blip");
				if (callback) callback();
				callback = element = null;
			});
		};

	return {
		show: function (options) {
			show(options);
		},

		ok: function (callback) {
			show({
				icon: "check", callback: callback
			});
		},

		ko: function (callback) {
			show({
				cssClass: "Error", icon: "exclamation", callback: callback
			});
		}
	};
}();