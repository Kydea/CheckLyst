instances.Instance = function (element, options) {
    var
        instance = this,
		information = $("div#InstanceInformation", element),
        header = new instances.Group($("div#Header", element), "Group"),
        page = new instances.Group($("div.Page", element), "Page"),
        prototypeState,
        tab,
        pageLock,
		updateHandler,
        externalSources = {},
        expressions = [],

        scroll = function () {
            information.eq(1).toggle(matematici.elements.window.scrollTop() > information.offset().top);
        },

        update = function (model) {
            prototypeState = model.prototypeState;

            information
                .empty()
                .append($("<table />").addClass("Grid")
                    .append($("<thead />")
                        .append($("<tr />")
                            .append($("<th />").text("company"))
                            .append($("<th />").text("department"))
                            .append($("<th />").text("prototype type"))
                            .append($("<th />").text("code"))
                            .append($("<th />").text("state"))
                        )
                    )
                    .append($("<tbody />")
                        .append($("<tr />")
                            .append($("<td />").text(model.department))
                            .append($("<td />").text(model.prototypeType))
                            //.append($("<td />").text(options.prototype + "." + version + "." + (number < 0 ? "?" : year + "." + number))) TODO
                            .append($("<td />").append(model.state || "Draft"))
                        )
                    )
				);

			tab.clear();

			for (var pageIndex in model.pages)
				tab.add(model.pages[pageIndex].description)

			tab.select(0)
			tab.resize();

            pageLock.update(model);
            if (model.header)
                header.update(model.header);
            else
                header.getElement().hide();
            page.update(model.pages[tab.getCurrentItem().index]);

            if (updateHandler) updateHandler(model);
            externalSources = model.externalSources;
            expressions = model.expressions;
		},

		load = function () {
			$.ajax({
				url: matematici.urls.instances + "/" + (options.code == "" ? "Create" : "Get"),
				type: "post",
				data: {
					code: options.code,
					prototype: options.prototype,
					version: options.version
				}
			}).done(function (result) {
				console.log(result)
				instances.instance.update(result.data.checkList);
			});
		},

        save = function (state, callback, note) {
            $.ajax({
				url: matematici.urls.instances + "/Save",
                type: "post",
                data: { ClientId: matematici.client.getId(), State: state, Note: note }
            }).done(function (result) {
                if (result.error) {
                    matematici.dialog.error("Error", result.error);
                } else if (result.redirect) {
                    matematici.canClose = true;
                    matematici.navigator.open(result.redirect, false, false);
                } else {
                    matematici.canClose = true;

                    $("div.ActionBar a.Save").text("Save"); //possibly changes Open into Save

                    if (callback)
                        callback.call();
                }
            });
        };

    instance.getKey = function () {
        return { prototype, version, year, number };
    };

    instance.getElement = function () {
        return element;
    };

    instance.getExpressions = function () {
        return expressions;
    };

    instance.getExternalSources = function () {
        return externalSources;
    };

    instance.getPrototypeState = function () {
        return prototypeState;
    };

    instance.update = update;

    instances.instance = instance;

    instance.save = function (callback) {
        save(null, callback);
    };

    instance.refresh = function () {
        new instances.Event(instances.Events.refresh).trigger();
    };

    instance.export = function () {
        $("<a href='" + matematici.urls.instances.export + "?ClientID=" + matematici.client.getId() + "&" + new Date().getTime() + "' target='_blank'></a>")[0].click();
    };

    instance.quit = function () {
        matematici.navigator.open(matematici.urls.instances.list, false, true);
    };

    instance.cancel = function (callback) {
        matematici.dialog.show("Cancel", "Provide a note for the cancelation", [
			{
			    text: "Save", icon: "check", handler: function () {
			        save(instances.Instance.states.canceled, callback, $("textarea.Textarea", $(this).closest("div.Dialog")).val());
			    }
			},
			{ text: "Cancel", icon: "remove" },
			{ textarea: true, text: "Insert note here" }
        ]);
    };

    instance.confirm = function (callback) {
        save(instances.Instance.states.confirmed, callback);
    };

    instance.setOnUpdateHandler = function (handler) {
        updateHandler = handler;
    };

    instance.showHistory = function () {
        $.ajax({
			url: matematici.urls.instances + "/GetHistory",
            type: "post",
            data: { ClientId: matematici.client.getId() }
        }).done(function (result) {
            if (result.Error)
                matematici.dialog.error("Error", result.Error);
            else {
                var
                    checkListGrid = new matematici.CheckListGrid(result.Data),
                    parameters = checkListGrid.getParameters(),
                    attributes = { type: "text", readonly: "readonly" };

                parameters.append($("<label />").text("Company").append($("<input />").attr(attributes).val(result.Company)));
                parameters.append($("<label />").text("Department").append($("<input />").attr(attributes).val(result.Department)));
                parameters.append($("<label />").text("Prototype Type").append($("<input />").attr(attributes).val(result.PrototypeType)));
                parameters.append($("<label />").text("Prototype").append($("<input />").attr(attributes).val(result.Prototype + "." + result.Version)));
                parameters.append($("<label />").text("Instance").append($("<input />").attr(attributes).val(result.Year + "." + result.Number)));
            }
        });
    };

    instance.initialize = function () {
		information.clone().addClass("Fixed").insertAfter(information);
		information = $("div#InstanceInformation", element); //both the elements

        matematici.elements.window.on("resize", scroll);
        matematici.elements.window.on("scroll", scroll);
        scroll();

        instanceElementInformation = new instances.ElementInformation(element);
		tab = new matematici.Tab($("div.TabContainer", element)).addHandler(function (index) {
            page.clear();
            new instances.Event(instances.Events.pageChange, index).trigger();
        });
		pageLock = new instances.PageLock(instance);

		load();
    };

    instance.destroy = function () {
        matematici.elements.window.off("resize", scroll);
        matematici.elements.window.off("scroll", scroll);
    }

    instance.initialize();
};

instances.Instance.states = {
    draft: "Draft",
	progress: "In progress",
	completed: "Completed",
    confirmed: "Confirmed",
    canceled: "Canceled"
};