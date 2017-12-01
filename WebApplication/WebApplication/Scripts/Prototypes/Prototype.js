prototypes.Prototype = function (selector, options) {
	var
		prototype = this,
		element = $(selector),
		form = $("form", element),
		page,
		header,
		pageIndex,
		title,
		tab,
		pageLock,
		model,
		selectedItem,
		itemHandler,
		updateHandler,

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

			if (model.state == prototypes.Prototype.states.draft) tab.add($("<i >").addClass("Add fa fa-plus"));
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

		setup = function () {
			page = new prototypes.Group($("div.Page", element), "Page", prototype);

			header = new prototypes.Group($("div#Header", element), "Group", prototype);

			tab = new matematici.Tab($("div.TabContainer", element), {}).addHandler(function (index) {
				pageIndex = index;
				if (index >= model.pages.length) {
					matematici.canClose = false;
					addPage();
				}
				update();

				prototype.select(null, null);
				prototype.select(null, page);
			});

			title = $("h2", element).on("click", function () {
				prototype.select(null, prototype);
			});

			$.ajax({
				url: form.attr("action") + options.action,
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

	prototype.getModel = function () {
		return model;
	};

	prototype.update = update;

	prototypes.prototype = prototype;

	setup();
}

prototypes.Prototype.states = {
	draft: "Draft",
	simulation: "In simulation",
	approved: "Approved",
	active: "Active",
	obsolete: "Obsolete"
}