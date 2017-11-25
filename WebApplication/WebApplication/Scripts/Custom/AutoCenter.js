matematici.AutoCenter = function (selector) {
	var
		autoCenter = this,
		element = $(selector).data("autoCenter", autoCenter),

		center = function () {
			element.css("top", (window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) < 760 ? 0 : Math.max(parseInt((element.parent().height() - element.outerHeight()) / 2), 0)); 
		},

		initialize = function () {
			matematici.elements.window.on("resize", center);
			center();
			element.show();
		},

		destroy = function () {
			matematici.elements.window.off("resize", center);
		};

	this.initialize = function () {
		setTimeout(initialize, 0);
	};

	this.destroy = function () {
		destroy();
	};

	this.center = function () {
		center();
	};
}

matematici.initializers.push(function (context) {
	$(".AutoCenter", context).each(function () {
		new matematici.AutoCenter(this).initialize();
	});
});

matematici.destroyers.push(function (context) {
	$(".AutoCenter", context).each(function () {
		$(this).data("autoCenter").destroy();
	});
});
