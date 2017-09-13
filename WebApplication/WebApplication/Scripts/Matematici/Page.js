matematici.Page = function (config) {
    var
        setup = function () {
            config = config || {};
            if (!config.name) config.name = "";
            if (!config.cssClass) config.cssClass = "";
            matematici.navigator.setBaseCSSClass(config.name == "" || config.cssClass == "" ? config.name + config.cssClass : config.name + " " + config.cssClass);

            matematici.breadcrumbs.set(config.breadcrumbs);

            matematici.help.setCode(config.helpPage, config.helpParagraph);

            if (config.initializers)
                for (var index in config.initializers)
                    if (config.initializers[index].initialize)
                        config.initializers[index].initialize();

            if (config.resize) {
                matematici.elements.window
                    .on("resize", config.resize)
                    .on("scroll", config.resize);
                config.resize();
            }

            matematici.events.onPageDestroy = function () {
                if (config.initializers)
                    for (var index in config.initializers)
                        if (config.initializers[index].destroy)
                            config.initializers[index].destroy();

                if (config.resize)
                    matematici.elements.window
                        .off("resize", config.resize)
                        .off("scroll", config.resize);
            };
        };

    setup();
};
/*
	config = {
		name: "",
		cssClass: "",
		breadcrumbs: "",
		initializers: [
			new ....
		]
        resize: function () {...}
	}
 */