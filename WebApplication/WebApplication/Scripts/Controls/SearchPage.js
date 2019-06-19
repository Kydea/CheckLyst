matematici.SearchPage = function (options) {
    var
        searchPage = this,
        grid,
        form,
        paginator,
        filtered = false,
        advancedSearchPanel = $("div.AdvancedSearch", options.filterFormSelector),
        advancedSearchMode = false,

        filterHandler = function (result) {
            grid.update(result.data);
            paginator.setPage(result.currentPage, result.pagesCount);
            filtered = true;
        },

        closeHandler = function () {
            matematici.canClose = true;
            matematici.save = null;
            if (filtered && matematici.popup.getResult().saved) form.getOptions().applyButton.click();
        },

        setAdvancedSearchMode = function (value) {
            advancedSearchMode = value;
            if (advancedSearchMode) {
                advancedSearchPanel.slideDown();
                $(this).addClass("Open");

                setTimeout(function () {
                    $("select.AutoComplete", advancedSearchPanel).each(function (index) {
                        var
                            select = $(this),
                            autoComplete = select.data("autoComplete");
                        if (autoComplete) autoComplete.refresh();
                    });
                }, 0);
            } else {
                advancedSearchPanel.slideUp();
                $(this).removeClass("Open");
            };
        },

        toggleAdvancedSearch = function () {
            setAdvancedSearchMode(!advancedSearchMode);
        },

		setup = function () {
			options.readOnly = options.readOnly != null ? options.readOnly : !matematici.tokenManager.checkRole(options.pageName + "#Edit#");
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
						rowClick: options.rowClickHandler || function (row) {
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
						},
						searchPage: searchPage
					}),
					paginator = new matematici.Paginator("div#Content div.Paginator", function (index) {
						form.trigger("GetData", filterHandler, { CurrentPage: index });
					})
				],

				breadcrumbs: options.breadCrumbs
			});

            if (advancedSearchPanel.length == 1) {
                form.getElement().addClass("AdvancedSearch");
                $("<a />")
                    .addClass("AdvancedSearch")
                    .text("Advanced search")
                    .on("click", toggleAdvancedSearch)
                    .attr("href", "#")
                    .append($("<i >").addClass("fi flaticon-arrow-down"))
                    .appendTo(form.getElement());
            }
        };

	searchPage.isReadOnly = function () {
		return options.readOnly;
	};

	searchPage.getForm = function () {
		return form;
    };

    searchPage.isAdvancedSearchMode = function () {
        return advancedSearchMode;
    };

    searchPage.setAdvancedSearchMode = function (value) {
        setAdvancedSearchMode(value); 
    };

	setup();
}