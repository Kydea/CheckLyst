prototypes.SourcesList = function () {
    var
        model = prototypes.prototype.getModel(),
        element,
        container,
        table,
        body,
        sources,
        noRecord,
        list = [],
        currentIndex,
        removeButton,

        up = function (icon) {
            var
                row = $(icon).closest("tr"),
                index = row.prevAll().length,
                item = list[index];

            list[index] = list[index - 1];
            list[index - 1] = item;
            console.log(list);

            row.insertBefore(row.prev());
        },

        down = function (icon) {
            var
                row = $(icon).closest("tr"),
                index = row.prevAll().length,
                item = list[row.prevAll().length];

            list[index] = list[index + 1];
            list[index + 1] = item;
            console.log(list);

            row.insertAfter(row.next());
        },

        select = function (index) {
            if (currentIndex != null) body.children().eq(currentIndex).removeClass("Selected").addClass("Handler").children().attr("contenteditable", false);
            if (model.state == prototypes.Prototype.states.draft) {
                currentIndex = index;
                if (currentIndex != null)
                    body.children().eq(currentIndex).addClass("Selected").removeClass("Handler")
                        .children(":first-child").attr("contenteditable", true).focus();
                removeButton.toggle(currentIndex != null);
            }
        },

        add = function (row) {
            noRecord.remove();
            list.push(row);

            var value = $("<td />").addClass("SourceColumn");
            if (model.state == prototypes.Prototype.states.draft) {
                var select = sources.clone().val(row.source);
                value.append(select);
                setTimeout(function () {
                    new matematici.AutoComplete(select).initialize();
                }, 0)
            } else
                value.text(row.source);

            row.node = $("<tr />")
                .addClass("Handler")
                .appendTo(table)
                .append($("<td />").addClass("CodeColumn").text(row.code))
                .append(value)
                .append($("<td />").addClass("Order Centered Inactive").append($("<i />").addClass("Up fi flaticon-arrow-up")).append($("<i />").addClass("Down fi flaticon-arrow-down")));

            if (model.state == prototypes.Prototype.states.draft)
                matematici.canClose = false;
        },

        remove = function () {
            body.children().eq(currentIndex).remove();
            list.splice(currentIndex, 1);
            if (list.length == 0) noRecord.appendTo(body);
            select();
            matematici.canClose = false;
        },

        checkCodes = function () {
            for (var index in list) {
                var code = list[index].node.find(".CodeColumn").text();
                code = code.replace(/(?:^\w|[A-Z]|\b\w|\s+)/g, function (match, index) {
                    if (+match === 0) return ""; // or if (/\s+/.test(match)) for white spaces
                    //return index == 0 ? match.toLowerCase() : match.toUpperCase();
                    return match;
                });
                list[index].node.find(".CodeColumn").text(code);
                if (!matematici.codeRegularExpression.test(code)) {
                    matematici.dialog.error("Invalid code", "The code " + code + " is not valid");
                    return false;
                }
            }
            return true;
        },

        close = function () {
            if (model.state == prototypes.Prototype.states.draft) {
                if (!checkCodes()) return;

                model.externalSources = {};
                body.children().each(function () {
                    var row = $(this);
                    model.externalSources[row.children("td").eq(0).text()] = $("select", row).val();
                });
            }
            element.remove();
        },

        setup = function () {
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            container = $("<div />").addClass("Container").appendTo(element).on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
            table = $("<table />").addClass("Grid SourcesList").appendTo(container)
                .append($("<thead />")
                    .append($("<tr />")
                        .append($("<th />").text("code"))
                        .append($("<th />").text("source"))
                        .append($("<th />"))
                    )
                ).append(body = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").attr("colspan", "3").text("(no record)"))
                    )
                ).on("click", "tr.Handler td.Order i.Up", function (e) {
                    e.stopPropagation();
                    up(this);
                }).on("click", "tr.Handler td.Order i.Down", function (e) {
                    e.stopPropagation();
                    down(this);
                }).on("click", "tr.Handler td:not(.Inactive)", function () {
                    select($(this).prevAll().size());
                });


            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container)
                        .append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("fi flaticon-times")).on("click", function () {
                            close();
                        }))
                        .append($("<a />").addClass("Button").text("add").toggle(model.state == prototypes.Prototype.states.draft).prepend($("<i />").addClass("fi flaticon-plus")).on("click", function () {
                            add({ code: "" });
                            select(body.children().size() - 1);
                        }))
                        .append(removeButton = $("<a />").addClass("Button").text("remove").prepend($("<i />").addClass("fi flaticon-minus")).on("click", function () {
                            remove();
                        }).hide());

            sources = $("<select />");
            var dataSources = prototypes.prototype.getDataSources();
            for (index in dataSources)
                $("<option />").appendTo(sources).text(dataSources[index].code).val(dataSources[index].code);

            for (var code in model.externalSources)
                add({
                    code: code,
                    source: model.externalSources[code]
                });
        };

    setup();
}