prototypes.Prototype = function (selector, options) {
	var
		prototype = this,
		element = $(selector),
		page,
		header,
		pageIndex,
		title,
		tab,
		pageLock,
		model,
		prototypeKey,
		concurrencyInformation,
		currentItem,
		itemHandler,
		commands,
		newItemsId = 1,

		getNewItemId = function () {
			return newItemsId++;
		},

		completeModel = function () {
			var analyzeElement = function (node) {
				switch (node.typeName) {
					case "Group":
						for (var itemIndex in node.items || [])
							analyzeElement(node.items[itemIndex]);
						break;
					case "Signature":
						if (node.code)
							model.signatures[node.code] = node;
						break;
				}
			};

			model.typeName = "Prototype";
			model.signatures = {};

			analyzeElement(model.header);
			for (var pageIndex in model.pages || []) {
				var page = model.pages[pageIndex];
				for (var groupIndex in page.groups || [])
					analyzeElement(page.groups[groupIndex])
			}
		},

		setupActionBar = function () {
			var
				buttonGroup = $("div.ActionBar a.ButtonGroup", element),
				previewButton = $("a.Preview", buttonGroup).addClass("Hidden"),
				startSimulationButton = $("a.StartSimulation", buttonGroup).addClass("Hidden"),
				cancelSimulationButton = $("a.CancelSimulation", buttonGroup).addClass("Hidden"),
				deleteButton = $("a.Delete", buttonGroup).addClass("Hidden"),
				submitForApprovalButton = $("a.SubmitForApproval", buttonGroup).addClass("Hidden"),
				approveButton = $("a.Approve", buttonGroup).addClass("Hidden"),
				submitForObsolescence = $("a.SubmitForObsolescence", buttonGroup).addClass("Hidden"),
				obsoleteButton = $("a.Obsolete", buttonGroup).addClass("Hidden"),
				saveButton = $("a.Save", buttonGroup).addClass("Hidden"),
				historyButton = $("a.History", buttonGroup).toggle(model.version > 0),
				importButton = $("a.Import", buttonGroup).addClass("Hidden"),
				exportButton = $("a.Export", buttonGroup);

			switch (model.state) {
				case prototypes.Prototype.states.active:
				case prototypes.Prototype.states.approved:
					if (model.proposedState)
						obsoleteButton.removeClass("Hidden");
					else 
						submitForObsolescence.removeClass("Hidden");
					break;
				case prototypes.Prototype.states.draft:
					if (model.version > 0) {
						previewButton.removeClass("Hidden");
						startSimulationButton.removeClass("Hidden");
						deleteButton.removeClass("Hidden");
					}
					importButton.removeClass("Hidden");
					saveButton.removeClass("Hidden");
					break;
				case prototypes.Prototype.states.simulation:
					previewButton.removeClass("Hidden");
					submitForApprovalButton.removeClass("Hidden");
					cancelSimulationButton.removeClass("Hidden");
					break;
				case prototypes.Prototype.states.approval:
					approveButton.removeClass("Hidden");
				case prototypes.Prototype.states.obsolete:
					break;
				case prototypes.Prototype.states.draftReadOnly:
					break;
				default:
					alert("Invalid prototype state: " + (model.state || "(null)"));
					console.log(model);
					break;
			}

			if ($("a:not(.Hidden)", buttonGroup).size() == 0)
				buttonGroup.hide();
		},

		update = function () {
			var
				currentItemId = currentItem && (currentItem.isPage && currentItem.isPage() && $("a.Current", element).attr("id") || $("div.Current", element).attr("id")); //saves current item

			completeModel();
			title.children("span").text(model.description || "(Prototype name)");

			commands = $("<div />").addClass("Commands")
				.append($("<a />").attr("data-command", "properties").addClass("Button MiniIcon Properties").append($("<i />").addClass("fi flaticon-cog")))
			if (model.state == prototypes.Prototype.states.draft)
				commands
					.append($("<a />").attr("data-command", "add").addClass("Button MiniIcon Add").append($("<i />").addClass("fi flaticon-plus")))
					.append($("<a />").attr("data-command", "up").addClass("Button MiniIcon Up").append($("<i />").addClass("fi flaticon-arrow-up")))
					.append($("<a />").attr("data-command", "down").addClass("Button MiniIcon Down").append($("<i />").addClass("fi flaticon-arrow-down")))
					.append($("<a />").attr("data-command", "remove").addClass("Button MiniIcon Remove").append($("<i />").addClass("fi flaticon-trash-o")));

			tab.clear();
			for (var index in model.pages)
				tab.add(model.pages[index].description, "Page" + model.pages[index].itemId);

			if (model.state == prototypes.Prototype.states.draft) tab.add($("<span />").prepend($("<i >").addClass("Add fi flaticon-plus")), null, "Add");
			page.setIndex(pageIndex = pageIndex >= model.pages.length ? model.pages.length - 1 : pageIndex ? pageIndex : 0);
			tab.select(pageIndex);

			if (model.header) {
				header.clear();
				header.update(model.header);
			} else
				header.getElement().hide();

			page.clear();
			if (model.pages[tab.getCurrentItem().index])
				page.update(model.pages[tab.getCurrentItem().index]);

			for (var index in model.expressions)
				if (model.expressions[index].Error) {
					$("div.ActionBar a.Button.ExpressionsList").addClass("Invalid");
					break;
				}

			if (currentItemId) {
				//restores current item
				currentItem = null;
				$("#" + currentItemId, element).removeClass("Current").removeClass("Selected").click(); //Selected is for the tab when current item is a page
			}

			setupActionBar();
		},

		select = function (event, item) {
			if (event) {
				event.stopPropagation();
				event.preventDefault();
			}

			if (itemHandler && !itemHandler.call(item.getElement()))
				return;

			if (currentItem) {
				currentItem.getElement().removeClass("Current");
				commands.remove();
				if (currentItem.isPage && currentItem.isPage())
					$("div.Commands", $("a.Selected", tab.getElement()).removeClass("Current")).remove();
			}
			currentItem = currentItem == item ? null : item;
			if (currentItem) {
				currentItem.getElement().addClass("Current");
				switch (currentItem.getModel().typeName) {
					case "Prototype":
						commands.insertBefore(title)
						break;
					case "Page":
						commands.insertAfter($("a.Selected", tab.getElement()).addClass("Current"));
						break;
					default:
						currentItem.getElement().append(commands);
						break;
				}

				commands.on("click", "a", command);
			}
		},

		command = function (e) {
			e.preventDefault();
			e.stopPropagation();

			var
				parent = currentItem.getParent && currentItem.getParent(),
				siblings = !!parent && parent.getChildren(),
				itemIndex = currentItem.getIndex && currentItem.getIndex();

			switch ($(this).data("command")) {
				case "up":
					var items = [];
					for (var index in siblings)
						items.push(siblings[index == itemIndex - 1 ? itemIndex : index == itemIndex ? index - 1 : index]);
					parent.setChildren(items);
					update();
					matematici.canClose = false;
					break;
				case "down":
					var items = [];
					for (var index in siblings)
						items.push(siblings[index == itemIndex ? itemIndex + 1 : index == itemIndex + 1 ? itemIndex : index]);
					parent.setChildren(items);
					update();
					matematici.canClose = false;
					break;
				case "add":
					currentItem.add();
					break;
				case "remove":
					matematici.dialog.show("Warning", "The current item will be removed", [
						{
							text: "confirm", icon: "check", handler: function () {
								parent.getChildren().splice(itemIndex, 1);
								update();
								matematici.canClose = false;
							}
						},
						{ text: "cancel", icon: "times", cancelButton: true }
					]);
					break;
				case "properties":
					prototypes.propertyPanel.select(currentItem);
					break;
				default:
					alert("invalid command");
					console.log(this);
					break;
			}
		},

		addPage = function () {
			if (!model.pages) model.pages = [];
			model.pages.push({ typeName: "Page", description: "(page)", itemId: getNewItemId() })
		},

		sign = function (action, grid, rows, row) {
			if (row.approved) {
				matematici.dialog.error("error", "This signature has already been issued");
				return true;
			}

			var
				allowed = false,
				tags = row.tags.split(", ");

			for (var role in tags)
				if (
					matematici.tokenManager.checkRole("Prototypes#Approve#" + model.department + "##" + tags[role])
					|| matematici.tokenManager.checkRole("Prototypes#Approve#" + model.department + "#" + model.prototypeType + "#" + tags[role])
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
				title: action,
				url: element.data("signature"),
				prototype: prototypeKey.id,
				version: prototypeKey.version,
				applicationStep: row.id,
				model: model,
				rows: rows,
				concurrencyInformation: concurrencyInformation,

				done: function (result) {
					grid.close();
					matematici.popup.close();
					model.state = result.state;
					model.validFrom = result.validFrom;
					model.validUntil = result.validUntil;
					if (result.completed)
						model.proposedState = model.proposedDate = null;
					concurrencyInformation = {
						modifiedBy: result.modifiedBy,
						modifiedOn: result.modifiedOn
					};
					update();
					setupActionBar();
					if (!result.completed)
						prototype.loadSignatureBook(action);
					if (result.approved)
						matematici.blip.ok();
					else
						matematici.blip.ko();
				}
			});
			return true;
		},

		setup = function () {
			page = new prototypes.Group($("div.Page", element), "Page", prototype);

			header = new prototypes.Group($("div#Header", element), "Group", prototype);

			tab = new matematici.Tab($("div.TabContainer", element), {}).addHandler(function (item) {
				var
					pageChange = pageIndex != item.index,
					newPage = item.index == model.pages.length;

				if (pageChange) {
					pageIndex = item.index;
					if (newPage) {
						matematici.canClose = false;
						addPage();
					}
					select(null, null);
					update();
				}

				select(null, page);
			});

			title = $("h2", element).on("click", function () {
				select(null, prototype);
			});

			//opening
			$.ajax({
				url: element.data("action") + options.action,
				type: "post",
				data: { id: options.id, version: options.version }
			}).done(function (result) {
				if (result.error)
					matematici.dialog.error("Error", result.error);
				else {
					model = result.checkList;

					prototypeKey = {
						id: result.id,
						version: result.version
					};
					if (result.version > 0) model.version = result.version;
					concurrencyInformation = {
						modifiedBy: result.modifiedBy,
						modifiedOn: result.modifiedOn
					};

					model.state = result.state;
					model.proposedState = result.proposedState;
					model.proposedDate = result.proposedDate;
					model.validFrom = result.validFrom;
					model.validUntil = result.validUntil;
					model.instanceConfirmationWorkFlow = result.instanceConfirmationWorkFlow;
					model.name = result.name;
					model.description = result.description;
					model.department = result.department;
					model.prototypeType = result.prototypeType;
					model.departmentName = result.departmentName;
					model.prototypeTypeName = result.prototypeTypeName;

					if (
						result.id >= 0
						&& !matematici.tokenManager.checkRole("Prototypes#Edit#" + result.department + "##")
						&& !matematici.tokenManager.checkRole("Prototypes#Edit#" + result.department + "#" + result.prototypeType)
					) 
						if (model.state == prototypes.Prototype.states.draft)
							model.state = prototypes.Prototype.states.draftReadOnly;

					update();
					element.removeClass("Loading");
				}
			});
		};

	prototype.getElement = function () {
		return element;
	};

	prototype.getTab = function () {
		return tab;
	};

	prototype.getModel = function () {
		return model;
	};

	prototype.getPrototypeKey = function () {
		return prototypeKey;
	};

	prototype.setItemHandler = function (value) {
		itemHandler = value;
	};

	prototype.getItemHandler = function () {
		return itemHandler;
	};

	prototype.getChildren = function () {
		return model.pages;
	};

	prototype.setChildren = function (value) {
		model.pages = value;
	};

	prototype.save = function (callback) {
		select();
		$.ajax({
			url: element.data("action") + "Save",
			type: "post",
			contentType: "application/json",
			data: JSON.stringify({
				id: prototypeKey.id,
				version: prototypeKey.version,
				modifiedOn: concurrencyInformation.modifiedOn,
				modifiedby: concurrencyInformation.modifiedBy,
				validFrom: model.validFrom,
				validUntil: model.validUntil,
				instanceConfirmationWorkFlow: model.instanceConfirmationWorkFlow,
				name: model.name,
				description: model.description,
				department: model.department,
				prototypeType: model.prototypeType,

				checkList: model
			})
		}).done(function (result) {
			matematici.canClose = true;
			prototypeKey.id = result.id;
			if (model.version == null) {
				matematici.navigator.setUrl(options.detailUrl + "/" + result.id + "/" + result.version);
				model.version = prototypeKey.version = result.version;
			}
			concurrencyInformation = {
				modifiedBy: result.modifiedBy,
				modifiedOn: result.modifiedOn
			};
			setupActionBar();

			if (callback)
				callback.call();
			else
				matematici.blip.ok();
		});
	};

	prototype.delete = function () {
		select();
		matematici.dialog.show("warning", "This draft will be permanently deleted.", [
			{
				text: "confirm", icon: "check", handler: function () {
					var done = function () {
						matematici.canClose = true;
						matematici.navigator.open(options.listUrl, false, false);
					};

					if (prototypeKey.version < 0)
						done()
					else
						$.ajax({
							url: element.data("action") + "Delete",
							type: "post",
							data: {
								id: prototypeKey.id,
								version: prototypeKey.version
							}
						}).done(function (result) {
							done();
						});
				}
			},
			{ text: "cancel", icon: "times", cancelButton: true }
		]);
	};

	prototype.setState = function (state, proposed) {
		select();
		var handler = function () {
			model.State = state;
			$.ajax({
				url: element.data("action") + "SetState",
				type: "post",
				data: {
					id: prototypeKey.id,
					version: prototypeKey.version,
					state: state,
					stateProposed: proposed
				}
			}).done(function (result) {
				model.state = result.state;
				model.proposedState = result.proposedState;
				model.proposedDate = result.proposedDate;
				update();
				matematici.blip.ok();
			});
		};

		matematici.dialog.show("warning", "Please confirm the change of state.", [
			{
				text: "confirm", icon: "check", handler: function () {
					if (matematici.canClose)
						handler();
					else
						prototype.save(handler);
				}
			},
			{ text: "cancel", icon: "times", cancelButton: true }
		]);
	};

	prototype.loadSignatureBook = function (action) {
		return $.ajax({
			url: element.data("api") + "/WorkFlows/Applications/GetSteps",
			type: "POST",
			data: {
				prototype: prototypeKey.id,
				version: prototypeKey.version,
			}
		}).done(function (result) {
			var columns = [
				{ name: "step", label: "Step", align: "center" },
				{ name: "tags", label: "Level" },
				{ name: "date", label: "Date", type: "DateTimeOffset" },
				{ name: "user", label: "User" },
				{ name: "approved", label: "Approved", align: "center" },
			];
			new matematici.CheckListGrid(result.steps,
				function (grid, row) {
					return sign(result.action, grid, result.steps, row);
				},
				null,
				columns);
		});
	};

	prototype.preview = function () {
		if (
			!matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "##")
			&& !matematici.tokenManager.checkRole("Prototypes#Edit#" + model.department + "#" + model.prototypeType + "#")
		) {
			matematici.dialog.error("error", "Permission denied");
			return;
		}

		var handler = function () {
			matematici.navigator.open(options.previewUrl + "/" + prototypeKey.id + "/" + prototypeKey.version);
		};

		if (matematici.canClose)
			handler();
		else
			matematici.dialog.show("warning", "There are unsaved changes.", [
				{ text: "save", icon: "check", handler: function () { prototype.save(handler); } },
				{ text: "cancel", icon: "times", cancelButton: true }
			]);
	};

	prototype.showHistory = function () {
		$.ajax({
			url: element.data("action") + "History",
			type: "post",
			data: {
				id: prototypeKey.id,
				version: prototypeKey.version,
			}
		}).done(function (result) {
			var
				columns = [
					{ label: "State", name: "state" },
					{ label: "Date", name: "date", type: "DateTimeOffset" },
					{ label: "User", name: "user" },
					{ label: "Approved", name: "approved", type: "Boolean", align: "center" },
					{ label: "Proposed state", name: "proposedState" },
					{ label: "Signature step", name: "step", align: "center" },
					{ label: "Signature level", name: "level" },
					{ label: "Signature comment", name: "comment" },
					{ label: "Appointed signer", name: "appointedUser"}
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

			parameters.append($("<label />").append($("<span />").text("Department")).append($("<i />").addClass("Icon fi flaticon-building-o")).append($("<input />").attr(attributes).val(model.departmentName)));
			parameters.append($("<label />").append($("<span />").text("Prototype type")).append($("<i />").addClass("Icon fi flaticon-folder-o")).append($("<input />").attr(attributes).val(model.prototypeTypeName)));
			parameters.append($("<label />").append($("<span />").text("Prototype")).append($("<i />").addClass("Icon fi flaticon-prototype")).append($("<input />").attr(attributes).val(model.name)));
			parameters.append($("<label />").append($("<span />").text("Version")).append($("<i />").addClass("Icon fi flaticon-version")).append($("<input />").attr(attributes).val(prototypeKey.version)));
		});
	};

	prototype.import = function () {
		var handler = function () {
			var oldModel = model;
			try {
				model = JSON.parse($("textarea", matematici.dialog.getElement()).val());
				model.name = model.department = model.prototypeType = model.version = validFrom = validUntil = null;
				state = prototypes.Prototype.states.draft;
				update();
				matematici.blip.ok();
			} catch (error) {
				matematici.dialog.error("Error", error);
				model = oldModel;
				update();
				return true;
			}
		};

		matematici.dialog.show({
			title: "Prototype import",
			content: "Beware, the current prototype properties and items will be replaced by the imported one:",
			buttons: [{
				textarea: true,
				height: 300,
			}, {
				icon: "check",
				text: "ok",
				handler: handler
			}, {
				icon: "times",
				text: "cancel",
				cancelButton: true
			}]
		});
	};

	prototype.export = function () {
		matematici.dialog.show({
			title: "Click to copy",
			buttons: [{
				textarea: true,
				val: JSON.stringify(model),
				readOnly: true,
				height: 300,
				handler: function () {
					$(this).select();
					document.execCommand("copy");
					matematici.blip.ok(matematici.dialog.close());
				}
			}, {
				hidden: true,
				cancelButton: true
			}]
		});

		matematici.dialog.setWidth(600);
	};

	prototype.update = function () {
		select();
		update();
	};

	prototype.quit = function () {
		matematici.navigator.open(options.listUrl);
	};

	prototype.getNewItemId = getNewItemId;

	prototypes.prototype = prototype;

	prototype.select = select;

	prototype.completeModel = completeModel;

	setup();
}

prototypes.Prototype.states = {
	draft: "Draft",
	draftReadOnly: "Draft ",
	simulation: "In simulation",
	approval: "In approval",
	approved: "Approved",
	active: "Active",
	obsolete: "Obsolete"
}