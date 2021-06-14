prototypes.SourcesList = function () {
    var
        sourcesList = this,
        model = prototypes.prototype.getModel(),
        element,
        autoCenter,
        table,
        tableBody,
        noRecord,
        currentIndex,
        list = [],
        addButton, openButton, dependenciesButton,
        orderBar,
        filtersBar,
        fixedHeader,

        getRow = function (index) {
            return $("tr", tableBody).eq(index);
        },

        toggleMoveButton = function () {
            orderBar.children().eq(0).toggleClass("Off", currentIndex == null || currentIndex == 0 || filtersBar.isFiltering());
            orderBar.children().eq(1).toggleClass("Off", currentIndex == null || currentIndex == list.length - 1 || filtersBar.isFiltering());
        },

        toggleNoRecord = function () {
            if ($("tr:not(.NoRecord):visible", tableBody).length == 0)
                noRecord.appendTo(tableBody);
            else
                noRecord.remove();
        },

        showDependencies = function () {
            var
                code = list[currentIndex].code,
                model = prototypes.prototype.getModel().externalSources[code];
            matematici.dependenciesDialog.show("Source " + code, model && model.dependingRelations, model && model.dependentRelations);
        },

        up = function () {
            var
                item = list[currentIndex],
                row = getRow(currentIndex);

            list[currentIndex] = list[currentIndex - 1];
            list[currentIndex - 1] = item;

            row.insertBefore(row.prev());
            --currentIndex;

            toggleMoveButton();
            matematici.canClose = false;
        },

        down = function (icon) {
            var
                item = list[currentIndex],
                row = getRow(currentIndex);

            list[currentIndex] = list[currentIndex + 1];
            list[currentIndex + 1] = item;

            row.insertAfter(row.next());
            ++currentIndex;

            toggleMoveButton();
            matematici.canClose = false;
        },

        select = function (index) {
            if (currentIndex != null) {
                getRow(currentIndex).removeClass("Selected");
                if (currentIndex == index) open(currentIndex);
            }
            currentIndex = index;
            if (currentIndex != null)
                getRow(currentIndex).addClass("Selected");

            toggleMoveButton();
            var editable = model.state == prototypes.Prototype.states.draft && currentIndex != null;
            openButton.toggle(editable);
            dependenciesButton.toggle(currentIndex != null);

            autoCenter.center();
        },

        toggleErrors = function (tr, model) {
            var errorMessage = "";
            for (var index in model.errors) {
                var error = model.errors[index];
                errorMessage += error.property + ": " + error.message + "\n";
            }

            $("i.Valid", tr)
                .removeClass("fa-check fa-times")
                .attr("title", errorMessage)
                .addClass(!model.errors || model.errors.length == 0 ? "fa-check" : "fa-times");
        },

        open = function (index) {
            new prototypes.SourceEditor(list, index,
                function (row) {
                    matematici.canClose = false;
                    if (index == null)
                        select(insert(row, currentIndex));
                    else {
                        list[index] = row;
                        update(index);
                    }
                    verify();
                }, remove);
        },

        removeItem = function (index) {
            list.splice(index, 1);
        },

        remove = function () {
            matematici.canClose = false;
            getRow(currentIndex).remove();
            removeItem(currentIndex);
            toggleNoRecord();
            verify();
            colorRows();
            select();
        },

        insertItem = function (row, index) {
            if (index == null) {
                list.push(row);
                return list.length - 1;
            } else {
                list.splice(index + 1, 0, row);
                return index + 1;
            }
        },

        insert = function (row, index) {
            var
                result,
                tr = $("<tr />")
                    .addClass("Handler")
                    .append($("<td />").append($("<i />").addClass("fa fa-caret-right")).attr("title", row.serializedValue))
                    .append($("<td />").addClass("CodeColumn"))
                    .append($("<td />").addClass("DataSourceColumn"))
                    .append($("<td />").addClass("Centered")
                        .append($("<i />").addClass("Valid fal"))
                    );

            noRecord.remove();
            insertItem(row, index);

            if (index == null) {
                tr.appendTo(tableBody)
                update(list.length - 1);
                result = list.length - 1;
            } else {
                tr.insertAfter(getRow(index));
                update(index + 1);
                result = index + 1;
            }

            colorRows();
            fixedHeader && fixedHeader.refresh();

            return result;
        },

        update = function (index) {
            var
                tr = getRow(index),
                row = list[index],
                cells = $("td", tr),
                commands = cells.last();

            cells.filter(".CodeColumn").text(row.code).attr("title", row.code);
            cells.filter(".DataSourceColumn").text(row.dataSource).attr("title", row.dataSource);

            toggleErrors(tr, row);
        },

        updatePrototype = function () {
            model.externalSources = {};
            for (var index in list)
                model.externalSources[list[index].code] = list[index];
        },

        verify = function (handler) {
            updatePrototype();
            prototypes.prototype.verify(function () {
                var model = prototypes.prototype.getModel();

                for (var index in list) {
                    var
                        row = model.externalSources[list[index].code],
                        error = list[index].error = row.error,
                        cells = getRow(index);

                    cells.find("i.Valid")
                        .removeClass("fa-check fa-times")
                        .attr("title", error)
                        .addClass(error == null ? "fa-check" : "fa-times");
                }

                if (handler) handler.call(this);
            });
        },

        close = function () {
            element.remove();
            matematici.elements.body.removeClass("CheckListGrid");
            autoCenter.destroy();
            matematici.sourcesList = null;
        },

        colorRows = function () {
            var
                filtering = filtersBar.isFiltering(),
                even = false;
            $("tr:visible", tableBody).each(function () {
                $(this).toggleClass("Even", filtering && even).toggleClass("Odd", filtering && !even);
                even = !even;
            });
        },

        filter = function (filtering, keyword, flags) {
            var
                rows = $("tr", tableBody),
                even = false;
            for (var index in list) {
                var
                    item = list[index],
                    visible = keyword == null;
                if (!visible && flags[0]) visible = !!item.code && item.code.toLowerCase().indexOf(keyword.toLowerCase()) >= 0;
                if (!visible && flags[1]) visible = !!item.dataSource && item.dataSource.toLowerCase().indexOf(keyword.toLowerCase()) >= 0;
                if (visible && !flags[2]) visible = !item.errors || item.errors.length == 0;
                if (visible && !flags[3]) visible = item.errors && item.errors.length > 0;
                rows.eq(index).toggle(visible).toggleClass("Even", filtering && even).toggleClass("Odd", filtering && !even);
                if (visible) even = !even;
            }
            toggleNoRecord();
            fixedHeader.refresh();
            select();
        },

        setup = function () {
            matematici.elements.body.addClass("CheckListGrid");
            matematici.sourcesList = sourcesList;

            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            var container = $("<div />").appendTo($("<div />").addClass("Container SourcesList").appendTo(element));

            autoCenter = new matematici.AutoCenter(container.parent());
            autoCenter.initialize();

            container.append($("<h2 />").html("Data sources list"));
            filtersBar = new matematici.GridFiltersBar(container, filter);
            filtersBar.addFlag("code", true);
            filtersBar.addFlag("source", true);
            filtersBar.addFlag($("<i />").addClass("fal fa-times"), true, true);
            filtersBar.addFlag($("<i />").addClass("fal fa-check"), true, true);

            container
                .append($("<a />").attr("href", "#").addClass("CloseButton CancelButton")
                    .append($("<i />").addClass("fas fa-times-circle"))
                    .on("click", function () {
                        updatePrototype();
                        close();
                    }));

            orderBar = $("<div />")
                .appendTo(container)
                .addClass("OrderBar")
                .append($("<a />").addClass("Icon Up").append($("<i />").addClass("fa fa-caret-up")))
                .append($("<a />").addClass("Icon Down").append($("<i />").addClass("fa fa-caret-down")))
                .on("click", "a.Up:not(.Off)", function (e) {
                    e.preventDefault();
                    up();
                }).on("click", "a.Down:not(.Off)", function (e) {
                    e.preventDefault();
                    down();
                });

            toggleMoveButton();

            var scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container);

            table = $("<table />").addClass("Grid").appendTo(scrollablePanel)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />"))
                        .append($("<th />").text("code"))
                        .append($("<th />").text("source"))
                        .append($("<th />"))
                    )
                ).append(tableBody = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").addClass("NoRecord").attr("colspan", "3").text("(no record)"))
                    )
                ).on("click", "tr.Handler td:not(.Inactive)", function () {
                    select($(this).parent().prevAll().size());
                });

            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container)
                        .append($("<a />").addClass("Button DefaultButton").text("close").prepend($("<i />").addClass("Icon fa fa-check")).on("click", function () {
                            updatePrototype();
                            close();
                        }))
                        .append(openButton = $("<a />").addClass("Button").text("open").hide().prepend($("<i />").addClass("Icon fa fa-edit")).on("click", function () {
                            open(currentIndex);
                        }))
                        .append($("<a />").addClass("Button").text("add").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("Icon fa fa-plus")).on("click", function () {
                            open();
                        }))
                        .append(dependenciesButton = $("<a />").addClass("Button").text("dependencies").hide().prepend($("<i />").addClass("Icon fa fa-sitemap")).on("click", function () {
                            showDependencies();
                        }));

            for (var code in model.externalSources)
                insert(model.externalSources[code]);

            fixedHeader = new matematici.GridFixedHeader(table);
        };

    this.select = function (code) {
        for (var item in list)
            if (list[item].code == code) {
                select(item);
                return;
            }
    };

    this.close = close;

    setup();
}