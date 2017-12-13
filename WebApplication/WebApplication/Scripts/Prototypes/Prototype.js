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

			if (model.state == prototypes.Prototype.states.draft) tab.add($("<span />").prepend($("<i >").addClass("Add fi flaticon-plus")));
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

	prototype.getTab = function () {
		return tab;
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

	prototype.getChildren = function () {
		return model.pages;
	};

	prototype.setChildren = function (value) {
		model.pages = value;
	};

	prototype.getNewItemId = getNewItemId;

	prototype.update = update;

	prototypes.prototype = prototype;

	prototype.select = select;

	prototype.completeModel = completeModel;

	setup();
}

prototypes.Prototype.states = {
	draft: "Draft",
	simulation: "In simulation",
	approved: "Approved",
	active: "Active",
	obsolete: "Obsolete"
}