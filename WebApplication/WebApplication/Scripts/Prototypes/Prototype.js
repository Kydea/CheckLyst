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
		currentItem,
		itemHandler,
		updateHandler,
		commands,

		completeModel = function () {
			var analyzeElement = function (node) {
				switch (node.typeName) {
					case "Group":
						for (var itemIndex in node.items || [])
							analyzeElement(node.items[itemIndex]);
						break;
					case "Signature":
						if (node.Code)
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

		update = function () {
			completeModel();
			title.text(model.description || "(Prototype name)");

			tab.clear();
			for (var index in model.pages)
				tab.add(model.pages[index].description);

			if (model.state == prototypes.Prototype.states.draft) tab.add($("<i >").addClass("Add fi flaticon-plus"));
			page.setIndex(pageIndex = pageIndex ? pageIndex : 0);
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

			if (updateHandler) updateHandler(model);
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
				if (currentItem.isPage && currentItem.isPage()) 
					$("a.Selected", tab.getElement()).addClass("Current").append(commands);
				else
					currentItem.getElement().append(commands);
			}
		},

		command = function (e) {
			e.preventDefault();
			e.stopPropagation();

			switch ($(this).data("command")) {
				case "up":
					break;
				case "down":
					break;
				case "remove":
					matematici.dialog.show("Warning", "The current item will be removed", [
						{
							text: "confirm", icon: "check", handler: function () {
								currentItem.remove();
							}
						},
						{ text: "cancel", icon: "remove", cancelButton: true }
					]);
					break;
				case "properties":
					break;
				default:
					alert("invalid command");
					console.log(this);
					break;
			}
		},

		addPage = function () {
			if (!model.pages) model.pages = [];
			model.pages.push({ typeName: "Page", description: "(page)" })
		},

		setup = function () {
			page = new prototypes.Group($("div.Page", element), "Page", prototype);

			header = new prototypes.Group($("div#Header", element), "Group", prototype);

			commands = $("<div />")
				.addClass("Commands")
				.append($("<a />").data("command", "up").addClass("Button MiniIcon").append($("<i />").addClass("fi flaticon-arrow-up")).on("click", command))
				.append($("<a />").data("command", "down").addClass("Button MiniIcon").append($("<i />").addClass("fi flaticon-arrow-down")).on("click", command))
				.append($("<a />").data("command", "remove").addClass("Button MiniIcon").append($("<i />").addClass("fi flaticon-times")).on("click", command))
				.append($("<a />").data("command", "properties").addClass("Button MiniIcon").append($("<i />").addClass("fi flaticon-cog")).on("click", command));

			tab = new matematici.Tab($("div.TabContainer", element), {}).addHandler(function (index) {
				pageIndex = index;
				if (index >= model.pages.length) {
					matematici.canClose = false;
					addPage();
				}
				update();

				select(null, null);
				select(null, page);
			});

			title = $("h2", element).on("click", function () {
				select(null, prototype);
			});

			$.ajax({
				url: element.data("action") + options.action,
				type: "post",
				data: { id: options.id, version: options.version }
			}).done(function (result) {
				if (result.error)
					matematici.dialog.error("Error", result.error);
				else {
					console.log(result.checkList);
					model = result.checkList;
					update();
				}
			});
		};

	prototype.getElement = function () {
		return element;
	};

	prototype.getModel = function () {
		return model;
	};

	prototype.setItemHandler = function (value) {
		itemHandler = value;
	};

	prototype.getItemHandler = function () {
		return itemHandler;
	};

	prototype.update = update;

	prototypes.prototype = prototype;

	prototype.select = select;

	setup();
}

prototypes.Prototype.states = {
	draft: "Draft",
	simulation: "In simulation",
	approved: "Approved",
	active: "Active",
	obsolete: "Obsolete"
}