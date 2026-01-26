matematici.SearchPage = function (options) {
    var
        searchPage = this,
        grid,
        form,
        paginator,
        filtered = false,
        advancedSearchPanel = $("div.AdvancedSearch", options.filterFormSelector),
        advancedSearchButton,
        advancedSearchMode = false,

        filterHandler = function (result) {
            grid.update(result.data, options.updateHandler);
            paginator.setPage(result.currentPage, result.pagesCount);
            filtered = true;
            options.filterHandler && options.filterHandler(result);
        },

        closeHandler = function () {
            matematici.canClose = true;
            matematici.save = null;
            if (filtered && matematici.popup.getResult().saved) form.getOptions().applyButton.trigger("click");
        },

        setAdvancedSearchMode = function (value) {
            advancedSearchMode = value;

            if (advancedSearchPanel.length == 0) return;

            if (advancedSearchMode) {
                advancedSearchPanel.slideDown();
                advancedSearchButton.addClass("Open");
                grid.getElement().addClass("AdvancedSearch")

                setTimeout(function () {
                    $("select.AutoComplete", advancedSearchPanel).each(function (index) {
                        var autoComplete = matematici.AutoComplete.get(this);
                        if (autoComplete) autoComplete.refresh();
                    });
                }, 0);
            } else {
                advancedSearchPanel.slideUp();
                advancedSearchButton.removeClass("Open");
                grid.getElement().removeClass("AdvancedSearch")
            };
        },

        toggleAdvancedSearch = function () {
            setAdvancedSearchMode(!advancedSearchMode);
        },

		setup = function () {
            options.readOnly = options.readOnly != null ? options.readOnly : !matematici.tokenManager.checkRole((options.functionGroup || options.pageName) + "#Edit#");
			if (!options.readOnly)
				new matematici.ActionBar("div.ActionBar").add("Add", function () {
					matematici.popup.show({
						url: options.detailUrl,
						data: null,
						sender: form,
						closeHandler: closeHandler
					});
                }, null, "plus");

            $(options.filterFormSelector).append($("<a />")
                .attr({ href: "#", id: "ApplyButton" })
                .addClass("Button GreyYellowButton DefaultButton")
                .text(options.searchButtonLabel || "search")
                .prepend($("<i />").addClass("Icon fa fa-caret-right"))
            );

            if (advancedSearchPanel.length == 1)
                advancedSearchButton = $("<a />")
                    .addClass("AdvancedSearch")
                    .text("Advanced search")
                    .on("click", toggleAdvancedSearch)
                    .attr("href", "#")
                    .append($("<i >").addClass("Icon fal fa-caret-down"));

			new matematici.Page({
				name: options.pageName,
				initializers: [
					//new matematici.FixedHeader("div#Content table.Grid")
                    grid = new matematici.Grid(options.resultGridSelector, {
                        name: options.pageName,
						rowClick: options.rowClickHandler || function (row) {
							matematici.popup.show({
								url: options.detailUrl,
								data: row.data("values"),
								sender: form,
								closeHandler: closeHandler
							});
                        },
                        setCellValueHandler: options.setCellValueHandler,
                        handleSettings: true
					}),
					form = new matematici.Form(options.filterFormSelector, {
						bindEvent: options.bindEvent,
						applyButton: {
							button: "#ApplyButton",
							event: options.getDataEvent || "GetData",
							handler: filterHandler,
						},
						searchPage: searchPage
                    }),
                    paginator = new matematici.Paginator("div#Content div.Paginator", grid, function (index) {
						form.trigger("GetData", filterHandler, { CurrentPage: index });
					})
				],

                breadcrumbs: options.breadCrumbs,
                helpPage: options.helpPage
            });

            if (advancedSearchPanel.length == 1) 
                form.getElement().addClass("AdvancedSearch").append(advancedSearchButton);
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

    searchPage.setAdvancedSearchMode = setAdvancedSearchMode;
    searchPage.filterHandler = filterHandler;

	setup();
}