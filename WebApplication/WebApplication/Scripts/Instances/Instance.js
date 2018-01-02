instances.Instance = function (element, options) {
	var
		instance = this,
		header = new instances.Group($("div#Header", element), "Group"),
		page = new instances.Group($("div.Page", element), "Page"),
		prototypeState,
		code,
		id,
		state,
		departmentName,
		prototypeTypeName,
		prototypeName,
		version,
		versionState,
		year,
		number,
		tab,
		pageLock,
		updateHandler,
		externalSources = {},
		expressions = [],

		setupTab = function (pages) {
			for (var pageIndex in pages)
				tab.add(pages[pageIndex].description)

			tab.select(0)
			tab.resize();
		},

		setupActionBar = function () {
			buttonGroup = $("div.ActionBar a.ButtonGroup", element),
				confirmButton = $("a.Confirm", buttonGroup).toggleClass("Hidden", true); //state != instances.Instance.states.completed
				completeButton = $("a.Complete", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.progress),
				cancelButton = $("a.Cancel", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.draft && state != instances.Instance.states.progress),
				saveButton = $("a.Save", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.draft && state != instances.Instance.states.progress),
				historyButton = $("a.History", buttonGroup).toggleClass("Hidden", true); //state == instances.Instance.states.draft

			if ($("a:not(.Hidden)", buttonGroup).size() == 0)
				buttonGroup.hide();
		},

		setupBreadcrumbs = function () {
			matematici.breadcrumbs.set([
				{ content: departmentName },
				{ content: prototypeTypeName },
				{ content: prototypeName + "." + version },
				{
					content:
					(versionState == prototypes.Prototype.states.draft ? "test" : versionState == prototypes.Prototype.states.simulation ? "SE" : year || "?") + "." + (number || "?") + " (" + state + ")"
				},
			]);
		},

		update = function (result) {
			var model = result.checkList;
			prototypeState = result.prototypeState;

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
			var newInstance = options.code == "";
			$.ajax({
				url: matematici.urls.instances + "/" + (newInstance ? "Create" : "Get"),
				type: "post",
				data: {
					code: options.code,
					prototype: options.prototype,
					version: options.version
				}
			}).done(function (result) {
				console.log(result)

				id = result.id;
				state = result.state;
				code = result.code;
				departmentName = result.departmentName;
				prototypeTypeName = result.prototypeTypeName;
				prototypeName = result.prototype;
				version = result.version;
				versionState = result.versionState;
				year = result.year;
				number = result.number;

				setupTab(result.checkList.pages);
				setupActionBar();
				setupBreadcrumbs();

				update(result);
				if (newInstance)
					matematici.navigator.setUrl(options.detailUrl + "/" + result.code);

				element.removeClass("Loading");
			});
		},

		send = function (action, note, callback) {
			$.ajax({
				url: matematici.urls.instances + "/" + action,
				type: "post",
				data: {
					code: code,
					note: note
				}
			}).done(function (result) {
				console.log(result)

				id = result.id || id;
				state = result.state || state;
				year = result.year || year;
				number = result.number || number;

				setupActionBar();
				setupBreadcrumbs();

				if (result.checkList) 
					update(result);

				if (callback)
					callback();
			});
		};

	instance.getCode = function () {
		return code;
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

	instance.refresh = function () {
		new instances.Event(instances.Events.refresh).trigger();
	};

	instance.export = function () {
		$("<a href='" + matematici.urls.instances.export + "?ClientID=" + matematici.client.getId() + "&" + new Date().getTime() + "' target='_blank'></a>")[0].click();
	};

	instance.quit = function () {
		if (
			window.opener
			&& (versionState == prototypes.Prototype.states.simulation || versionState == prototypes.Prototype.states.draft)
		)
			window.close();
		else
			matematici.navigator.open(options.listUrl);
	};

	instance.save = function () {
		send(state == instances.Instance.states.draft ? "Open" : "Save");
	};

	instance.cancel = function () {
		if (versionState == prototypes.Prototype.states.active && state == instances.Instance.states.progress)
			matematici.dialog.show("Cancel", "Provide a note for the cancelation", [
				{
					text: "Confirm", icon: "check", handler: function () {
						send("Cancel", $("textarea.Textarea", $(this).closest("div.Dialog")).val(), instance.quit);
					}
				},
				{ icon: "times", hidden: true, cancelButton: true },
				{ textarea: true, text: "Note" }
			]);
		else
			matematici.dialog.show("Cancel", "The instance will be deleted", [
				{
					text: "Confirm", icon: "check", handler: function () {
						send("Cancel", null, instance.quit);
					}
				},
				{ icon: "times", hidden: true, cancelButton: true },
			]);
	};

	instance.complete = function () {
		send("Complete");
	};

	instance.confirm = function () {
		alert("not implemented"); //todo
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
		instanceElementInformation = new instances.ElementInformation(element);
		tab = new matematici.Tab($("div.TabContainer", element)).addHandler(function (item) {
			page.clear();
			new instances.Event(instances.Events.pageChange, item.index).trigger();
		});
		pageLock = new instances.PageLock(instance);

		load();
	};

	instance.destroy = function () {
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