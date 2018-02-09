instances.Instance = function (element, options) {
	var
		instance = this,
		title = $("h2", element),
		header = new instances.Group($("div#Header", element), "Group"),
		page = new instances.Group($("div.Page", element), "Page"),
		versionState,
		code,
		id,
		state,
		department,
		departmentName,
		prototypeType,
		prototypeTypeName,
		prototypeName,
		version,
		versionState,
		year,
		number,
		instanceDescription,
		workFlow,
		tab,
		pageLock,
		updateHandler,
		externalSources = {},
		expressions = [],
		lastResult,

		setupInstanceDescription = function () {
			header.getElement().prepend(instanceDescription = $("<div />").addClass("InstanceDescription"));
		},

		setupTab = function (pages) {
			for (var pageIndex in pages)
				tab.add(pages[pageIndex].description, null, null, "lock-o")

			pageLock = new instances.PageLock(instance);

			tab.select(0)
			tab.resize();
		},

		setupActionBar = function () {
			buttonGroup = $("div.ActionBar a.ButtonGroup", element),
				confirmButton = $("a.Confirm", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.completed),
				completeButton = $("a.Complete", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.progress),
				deleteButton = $("a.Delete", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.draft && state != instances.Instance.states.progress),
				saveButton = $("a.Save", buttonGroup).toggleClass("Hidden", state != instances.Instance.states.draft && state != instances.Instance.states.progress),
				historyButton = $("a.History", buttonGroup).toggleClass("Hidden", state == instances.Instance.states.draft);

			if ($("a:not(.Hidden)", buttonGroup).size() == 0)
				buttonGroup.hide();
		},

		getFormattedYear = function () {
			return versionState == prototypes.Prototype.states.draft ? "test" : versionState == prototypes.Prototype.states.simulation || year == 0 ? "SE" : year || "?";
		},

		setupBreadcrumbs = function () {
			matematici.breadcrumbs.set([
				{ content: departmentName, label: "department"},
				{ content: prototypeTypeName, label: "prototype type" },
				{ content: prototypeName + "." + version + "." + getFormattedYear() + "." + (number || "?"), label: "code" },
				{ content: state, label: "state" }
			]);
		},

		update = function (result) {
			lastResult = result;
			var model = result.checkList;
			versionState = result.versionState || versionState;

			if (result.prototypeDescription) title.text(result.prototypeDescription);
			instanceDescription.html(result.checkList.description).toggle(!!result.checkList.description && result.checkList.description != "");
			instanceDescription.attr("title", instanceDescription.text());

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
				url: matematici.urls.instances.api + "/" + (newInstance ? "Create" : "Get"),
				type: "post",
				data: {
					code: options.code,
					prototype: options.prototype,
					version: options.version
				}
			}).done(function (result) {
				id = result.id;
				state = result.state;
				code = result.code;
				department = result.department,
				departmentName = result.departmentName;
				prototypeType = result.prototypeType;
				prototypeTypeName = result.prototypeTypeName;
				prototypeName = result.prototypeName;
				version = result.version;
				versionState = result.versionState;
				year = result.year;
				number = result.number;

				setupInstanceDescription();
				setupTab(result.checkList.pages);
				setupActionBar();
				setupBreadcrumbs();

				update(result);
				if (newInstance)
					matematici.navigator.setUrl(options.detailUrl + "/" + result.code);

				element.removeClass("Loading");
			});
		},

		sign = function (grid, rows, row) {
			if (row.approved) {
				matematici.dialog.error("error", "This signature has already been issued");
				return true;
			}

			var
				allowed = false,
				tags = row.tags.split(", ");

			for (var role in tags)
				if (
					matematici.tokenManager.checkRole("Instances#Confirm#" + department + "##" + tags[role])
					|| matematici.tokenManager.checkRole("Instances#Confirm#" + department + "#" + prototypeType + "#" + tags[role])
				) {
					allowed = true;
					break;
				}

			if (!allowed) {
				matematici.dialog.error("error", "Permission denied");
				return true;
			}

			for (var rowIndex in rows) {
				if (rows[rowIndex].step < row.step && !rows[rowIndex].approved) {
					allowed = false;
					break;
				}

				if (rows[rowIndex].id == row.id)
					break;
			}
			if (!allowed) {
				matematici.dialog.error("error", "The previous step is not completed");
				return true;
			}

			for (var rowIndex in rows) {
				if (rows[rowIndex].step == row.step && rows[rowIndex].user == matematici.tokenManager.getUserCode()) {
					allowed = false;
					break;
				}
			}
			if (!allowed) {
				matematici.dialog.error("error", "You have already signed for this step");
				return true;
			}

			workFlowSignature = new matematici.WorkFlowSignature({
				title: "Instance confirmation",
				reason: row.reason,
				url: element.data("signature"),
				instance: id,
				applicationStep: row.id,
				rows: rows,
				concurrencyInformation: { modifiedBy: null, modifiedOn: null }, //TODO: concurrency fot the signature 

				done: function (result) {
					grid.close();
					matematici.popup.close();
					var stateChanged = state != result.state;
					state = result.state;
					setupActionBar();
					if (!stateChanged)
						instance.confirm(action);
					if (result.approved)
						matematici.blip.ok();
					else
						matematici.blip.ko();
				}
			});
			return true;
		},

		send = function (action, note, callback) {
			$.ajax({
				url: matematici.urls.instances.api + "/" + action,
				type: "post",
				data: {
					code: code,
					note: note
				}
			}).done(function (result) {
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

	instance.getVersionState = function () {
		return versionState;
	};

	instance.update = update;

	instances.instance = instance;

	instance.refresh = function () {
		new instances.Event(instances.Events.refresh).trigger();
	};

	instance.export = function () {
		$.ajax({
			url: matematici.urls.instances.api + "/Export",
			type: "post",
			data: { code: code }
		}).done(function (result) {
			location = matematici.urls.instances.api + "/Export?Token=" + result.token;
		});
	};

	instance.quit = function () {
		matematici.navigator.open(options.listUrl);
	};

	instance.save = function () {
		send(state == instances.Instance.states.draft ? "Open" : "Save");
	};

	instance.delete = function () {
		if (versionState == prototypes.Prototype.states.active && state == instances.Instance.states.progress)
			matematici.dialog.show("Instance deletion", "Instance will be set as deleted. Provide a note for the deletion", [
				{
					text: "Confirm", icon: "check", handler: function () {
						send("Delete", $("textarea.Textarea", $(this).closest("div.Dialog")).val(), instance.quit);
					}
				},
				{ icon: "times", hidden: true, cancelButton: true },
				{ textarea: true, text: "Note" }
			]);
		else
			matematici.dialog.show("Instance deletion", "The instance will be deleted", [
				{
					text: "Confirm", icon: "check", handler: function () {
						send("Delete", null, instance.quit);
					}
				},
				{ icon: "times", hidden: true, cancelButton: true },
			]);
	};

	instance.complete = function () {
		matematici.dialog.show("warning", "Please confirm the change of state.", [
			{
				text: "confirm", icon: "check", handler: function () {
					send("Complete");
				}
			},
			{ text: "cancel", icon: "times", cancelButton: true }
		]);
	};

	instance.confirm = function () {
		return $.ajax({
			url: element.data("api") + "/WorkFlows/Applications/GetSteps",
			type: "POST",
			data: {
				instance: id
			}
		}).done(function (result) {
			var columns = [
				{ name: "step", label: "Step", align: "center" },
				{ name: "tags", label: "Level" },
				{ name: "reason", label: "Reason" },
				{ name: "date", label: "Date", type: "DateTimeOffset" },
				{ name: "user", label: "User" },
				{ name: "approved", label: "Approved", align: "center" },
			];
			new matematici.CheckListGrid(result.steps,
				function (grid, row) {
					return sign(grid, result.steps, row);
				},
				null,
				columns);
		});
	};

	instance.setOnUpdateHandler = function (handler) {
		updateHandler = handler;
	};

	instance.showHistory = function () {
		$.ajax({
			url: matematici.urls.instances.api + "/History",
			type: "post",
			data: {
				id: id
			}
		}).done(function (result) {
			var
				columns = [
					{ label: "State", name: "state" },
					{ label: "Date", name: "date", type: "DateTimeOffset" },
					{ label: "User", name: "user" },
					{ label: "Note", name: "note"  },
					{ label: "Approved", name: "approved", type: "Boolean", align: "center" },
					{ label: "Proposed state", name: "proposedState" },
					{ label: "Signature step", name: "step", align: "center" },
					{ label: "Signature reason", name: "reason" },
					{ label: "Signature comment", name: "comment" },
					{ label: "Appointed signers", name: "appointedUser" }
				],
				rowRenderHandler = function (checkListGrid, rowIndex, data) {
					checkListGrid.addRow(data, rowIndex % 2 == 0 ? "Odd" : "Even");
					for (var subRowIndex in data.steps) {
						var
							subRow = data.steps[subRowIndex],
							tr = checkListGrid.addRow(subRow, "SubRow " + (subRowIndex % 2 == 0 ? "Odd " : "Even ") + (subRow.approved == true ? "Green" : subRow.approved == false ? "Red" : ""));

						$("td", tr).eq(0).addClass(rowIndex % 2 == 0 ? "Odd" : "Even");
					}
					return true;
				},
				checkListGrid = new matematici.CheckListGrid(result, null, null, columns, rowRenderHandler),
				parameters = checkListGrid.getParameters(),
				attributes = { type: "text", readonly: "readonly" };

			parameters.append($("<label />").append($("<span />").text("Department")).append($("<i />").addClass("Icon fi flaticon-building-o")).append($("<input />").attr(attributes).val(departmentName)));
			parameters.append($("<label />").append($("<span />").text("Prototype type")).append($("<i />").addClass("Icon fi flaticon-folder-o")).append($("<input />").attr(attributes).val(prototypeTypeName)));
			parameters.append($("<label />").append($("<span />").text("Prototype version")).append($("<i />").addClass("Icon fi flaticon-prototype")).append($("<input />").attr(attributes).val(prototypeName + "." + version)));
			parameters.append($("<label />").append($("<span />").text("Instance")).append($("<i />").addClass("Icon fi flaticon-version")).append($("<input />").attr(attributes).val(getFormattedYear() + "." + number)));
		});
	};

	instance.initialize = function () {
		instanceElementInformation = new instances.ElementInformation(element);
		tab = new matematici.Tab($("div.TabContainer", element)).addHandler(function (item) {
			if (state == instances.Instance.states.draft || state == instances.Instance.states.progress) {
				page.clear();
				new instances.Event(instances.Events.pageChange, item.index).trigger();
			}
			else
				update(lastResult);
		});
		load();
	};

	instance.close = function (callback) {
		if (state == instances.Instance.states.draft || state == instances.Instance.states.progress) {
			new instances.Event(instances.Events.quit, null, null, null, true).done(function () {
				callback();
			}).trigger();
		}
	};

	instance.destroy = function () {
	};

	instance.initialize();
};

instances.Instance.states = {
	draft: "Draft",
	progress: "In progress",
	completed: "Completed",
	confirmed: "Confirmed",
	deleted: "Deleted"
};