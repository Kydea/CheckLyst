prototypes.ItemsList = function () {
    var
        itemsList = this,
        model = prototypes.prototype.getModel(),
        autoCenter,
        element,
        table,
        tableBody,
        currentIndex,
        items = [],
        openButton, dependenciesButton,
        valid = true,

        getRow = function (index) {
            return $("tr", tableBody).eq(index);
        },

        showDependencies = function () {
            var
                item = prototypes.prototype.getCurrentItem(),
                model = item.getModel(),
                title = item.toString();

            if (valid)
                matematici.dependenciesDialog.show(title, model.dependingRelations, model.dependentRelations);
        },

        select = function (index) {
            if (currentIndex != null) {
                getRow(currentIndex).removeClass("Selected");
                if (currentIndex == index) open();
            }
            currentIndex = index;
            if (currentIndex != null)
                getRow(currentIndex).addClass("Selected");

            openButton.toggle(currentIndex != null);
            dependenciesButton.toggle(currentIndex != null);
            if (currentIndex != null) {
                var item = items[currentIndex];
                prototypes.prototype.select();
                if (item.typeName == "Prototype")
                    prototypes.prototype.select(null, prototypes.prototype);
                else
                    prototypes.prototype.selectById(item.typeName == "Page" ? "Page" : "ItemId", item.typeName == "Page" ? item.index : item.id);
            }
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
                .addClass(model.errors.length == 0 ? "fa-check" : "fa-times");
        },

        open = function () {
            prototypes.propertyPanel.select(prototypes.prototype.getCurrentItem(), {
                okHandler: function () {
                    var
                        model = prototypes.prototype.getCurrentItem().getModel(),
                        tr = $("tr.Selected", tableBody);
                    $("td.CodeColumn", tr).text(model.code);
                    $("td.DescriptionColumn", tr).text(model.description);
                    toggleErrors(tr, model);

                    $("div.ActionBar a.ItemsList").toggleClass("Invalid", $("tr i.Valid.fa-times", tableBody).length > 0);
                }
            });
        },

        close = function () {
            element.remove();
            matematici.elements.body.removeClass("CheckListGrid");
            autoCenter.destroy();
            matematici.itemsList = null;
        },

        add = function (item) {
            items.push(item);
            var tr = $("<tr />")
                .appendTo(tableBody)
                .addClass("Handler")
                .append($("<td />").append($("<i />").addClass("fa fa-caret-right")))
                .append($("<td />").addClass("ItemTypeColumn").text(item.typeName == "Prototype" ? "Checklist" : item.typeName))
                .append($("<td />").addClass("IdColumn").text(item.id || item.index != null && (item.index + 1) || ""))
                .append($("<td />").addClass("CodeColumn").text(item.code))
                .append($("<td />").addClass("DescriptionColumn").text(item.description))
                .append($("<td />").addClass("Centered").append($("<i />").addClass("Valid fal")))

                toggleErrors(tr, item);
        },

        traverse = function (model) {
            add(model);
            if (model.typeName == "Prototype") {
                traverse(model.header);
                for (var index in model.pages)
                    traverse(model.pages[index]);
            } else if (model.typeName == "Page") 
                for (var index in model.groups)
                    traverse(model.groups[index]);
            else if (model.typeName == "Group") 
                for (var index in model.items)
                    traverse(model.items[index]);
        },

        setup = function () {
            matematici.elements.body.addClass("CheckListGrid");
            matematici.itemsList = itemsList;

            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            var container = $("<div />").appendTo($("<div />").addClass("Container ItemsList").appendTo(element).on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
            }));

            autoCenter = new matematici.AutoCenter(container.parent());
            autoCenter.initialize();

            var scrollablePanel = $("<div />").addClass("ScrollablePanel").appendTo(container);

            table = $("<table />").addClass("Grid").appendTo(scrollablePanel)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />"))
                        .append($("<th />").text("item type"))
                        .append($("<th />").text("id"))
                        .append($("<th />").text("code"))
                        .append($("<th />").text("description"))
                        .append($("<th />"))
                    )
                ).append(
                   tableBody = $("<tbody />")
                ).on("click", "tr.Handler td:not(.Inactive)", function () {
                    select($(this).parent().prevAll().size());
                });

            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container.parent())
                        .append($("<a />").addClass("Button CancelButton").text("close").prepend($("<i />").addClass("Icon fa fa-times")).on("click", function () {
                            close();
                        }))
                        .append(openButton = $("<a />").addClass("Button").text("properties").hide().prepend($("<i />").addClass("Icon fa fa-edit")).on("click", function () {
                            open();
                        }))
                        .append(dependenciesButton = $("<a />").addClass("Button").text("dependencies").hide().prepend($("<i />").addClass("Icon fa fa-sitemap")).on("click", function () {
                            showDependencies();
                        }));

            traverse(model);
        };

    setup();
};