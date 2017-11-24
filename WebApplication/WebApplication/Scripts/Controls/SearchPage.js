matematici.SearchPage = function (options) {
	var
		grid,
		form,
		paginator,
		filtered = false,
		filterHandler = function (result) {
			grid.update(result.data);
			paginator.setPage(result.currentPage, result.pagesCount);
			filtered = true;
		},

		closeHandler = function () {
			if (filtered && matematici.popup.getResult().saved) form.getOptions().applyButton.click();
		},

		setup = function () {
			if (!options.readOnly)
				new matematici.ActionBar("div.ActionBar").add("Add", function () {
					matematici.popup.show({
						url: options.detailUrl,
						data: null,
						sender: form,
						closeHandler: closeHandler
					});
				}, null, "plus");

			$(options.filterFormSelector).append(
				$("<a />")
					.attr({ href: "#", id: "ApplyButton" })
					.addClass("Button GreyYellowButton DefaultButton")
					.text("search")
					.prepend($("<i />").addClass("fi flaticon-arrow-right"))
			);

			new matematici.Page({
				name: options.pageName,
				initializers: [
					//new matematici.FixedHeader("div#Content table.Grid")
					grid = new matematici.Grid(options.resultGridSelector, {
						rowClick: function (row) {
							matematici.popup.show({
								url: options.detailUrl,
								data: row.data("values"),
								sender: form,
								closeHandler: closeHandler
							});
						}
					}),
					form = new matematici.Form(options.filterFormSelector, {
						bindEvent: options.bindEvent,
						applyButton: {
							button: "#ApplyButton",
							event: "GetData",
							handler: filterHandler,
						}
					}),
					paginator = new matematici.Paginator("div#Content div.Paginator", function (index) {
						form.trigger("GetData", filterHandler, { CurrentPage: index });
					})
				],

				breadcrumbs: options.breadCrumbs
			});
		}

	setup();
}