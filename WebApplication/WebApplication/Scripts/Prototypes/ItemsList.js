prototypes.ItemsList = function () {
    var
        itemsList = this,
        model = prototypes.prototype.getModel(),
        autoCenter,
        element,
        table,
        tableBody,
        currentIndex,
        items = {},
        openButton, dependenciesButton,
        valid = true,

        getRow = function (index) {
            return $("tr", tableBody).eq(index);
        },

        showDependencies = function () {
            var
                item = prototypes.prototype.getModel().items[id],
                model = item.getModel(),
                title = item.toString();

            if (valid)
                matematici.dependenciesDialog.show(title, model.dependingRelations, model.dependentRelations);
        },

        select = function (index) {
            if (currentIndex != null) {
                getRow(currentIndex).removeClass("Selected");
                if (currentIndex == index) open(currentIndex);
            }
            currentIndex = index;
            if (currentIndex != null)
                getRow(currentIndex).addClass("Selected");

            openButton.toggle(currentIndex != null);
            dependenciesButton.toggle(currentIndex != null);
        },

        open = function (index) {
            prototypes.propertyPanel.select(items[tableBody.children().eq(index).data("id")]);
        },

        close = function () {
            element.remove();
            matematici.elements.body.removeClass("CheckListGrid");
            autoCenter.destroy();
            matematici.itemsList = null;
        },

        add = function (item, code) {
            items[code] = {
                getModel: function () { return model; }
            };
            $("<tr />")
                .appendTo(tableBody)
                .addClass("Handler").data("id", code)
                .append($("<td />").append($("<i />").addClass("fa fa-caret-right")))
                .append($("<td />").addClass("ItemTypeColumn").text(item.typeName))
                .append($("<td />").addClass("IdColumn").text(item.id || item.index && (item.index + 1) || ""))
                .append($("<td />").addClass("CodeColumn").text(item.code))
                .append($("<td />").addClass("DescriptionColumn").text(item.description))
                .append($("<td />").addClass("Centered")
                    .append($("<i />").addClass("Valid fal"))
                );
        },

        traverse = function (model) {
            if (model.typeName == "Prototype") {
                items = {}
                add(model, "prototype");
                tableBody.empty();
                traverse(model.header);
                for (var index in model.pages)
                    traverse(model.pages[index]);
            } else if (model.typeName == "Page") {
                add(model, "page" + index);
                for (var index in model.groups)
                    traverse(model.groups[index]);
            } else if (model.typeName == "Group") {
                add(model, "item" + model.id);
                for (var index in model.items)
                    traverse(model.items[index]);
            } else
                add(model, "item" + model.id);
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
                        .append(openButton = $("<a />").addClass("Button").text("open").hide().prepend($("<i />").addClass("Icon fa fa-edit")).on("click", function () {
                            open(currentIndex);
                        }))
                        .append(dependenciesButton = $("<a />").addClass("Button").text("dependencies").hide().prepend($("<i />").addClass("Icon fa fa-sitemap")).on("click", function () {
                            showDependencies();
                        }));

            traverse(model);
        };

    setup();
};