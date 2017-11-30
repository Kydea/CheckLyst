instances.ExternalSources = function (itemId, name) {
    var
        element,
        page = 0,
        parameters,
        next,
        previous,
        search,
        header,
        container,
        body,

        filterChanged = function () {
            next.hide();
            previous.hide();
            page = 0;
        },

        addFilter = function (parameter) {
            parameters
                .append($("<label />").text(parameter.Label)
                    .append($("<input />").attr({ type: "text", id: parameter.Name }).on("change, keydown", filterChanged))
                );
        },

        setup = function () {
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            container = $("<div />").addClass("Container").appendTo(element);
            $("<h2 />").text(name).appendTo(container);
            parameters = $("<div />").addClass("Parameters").appendTo(container);
            $("<table />").addClass("Grid").appendTo(container)
                .append($("<thead />")
                    .append(header = $("<tr />"))
                ).append(
                    body = $("<tbody />")
                ).on("click", "tr.Handler", function () {
                    var tr = $(this);
                    new instances.Event(instances.Events.externalSourceRowChange, itemId, { RowNumber: tr.data("index"), Row: JSON.stringify(tr.data("row")), FakeRow: tr.data("row") }).trigger();
                    element.remove();
                });

            $("<div />").addClass("Buttons").appendTo(container)
                .append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("fi flaticon-times")).on("click", function () {
                    element.remove();
                }))
				.append(next = $("<a />").addClass("Button").text("next").prepend($("<i />").addClass("fi flaticon-arrow-right")).on("click", function () {
                    ++page;
                    load();
                }).hide())
				.append(previous = $("<a />").addClass("Button").text("previous").prepend($("<i />").addClass("fi flaticon-arrow-left")).on("click", function () {
                    --page;
                    load();
                }).hide())
				.append(search = $("<a />").addClass("Button").text("search").prepend($("<i />").addClass("fi flaticon-search-o")).on("click", function () {
                    load();
                }));

            $.ajax(matematici.urls.instances.externalSource, {
                method: "post",
                data: { Name: instances.instance.getExternalSources()[name], Page: -1 }
            }).done(function (result) {
                if (result.Error)
                    matematici.dialog.error("error", result.Error);
                else
                    for (var index in result.Parameters)
                        addFilter(result.Parameters[index]);
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR, textStatus, errorThrown);
                matematici.dialog.error("server error", "Client unique identifier unavailable");
            });
        },

        getValues = function () {
            var values = {};
            $("input", parameters).each(function () {
                var input = $(this);
                values[input.attr("id")] = input.val();
            });
            return values;
        },

        fill = function (values) {
            body.empty();
            header.empty();
            previous.toggle(page > 0);
            next.toggle(values.length > 0 && values.length < 20);
            if (values.length == 0) {
                header.append($("<th />").text(name));
                body.append($("<tr />").addClass("NoRecord").append($("<td />").attr("colspan", 1).text("(no record)")))
            } else {
                for (var column in values[0])
                    if (column[0] != "_")
                        header.append($("<th />").text(column.split("_").join(" ")));

                for (var row in values) {
                    var tr = $("<tr />").appendTo(body).addClass("Handler");
                    for (var column in values[row])
                        if (column[0] != "_")
                            tr.append($("<td />").html(values[row][column])).data({ index: parseInt(row) + 20 * page, row: values[row] });
                }
            }
        },

        load = function () {
            $.ajax(matematici.urls.instances.externalSource, {
                method: "post",
                data: { Values: getValues(), Name: instances.instance.getExternalSources()[name], Page: page }
            }).done(function (result) {
                if (result.Error)
                    matematici.dialog.error("error", result.Error);
                else
                    fill(result.Data);
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR, textStatus, errorThrown);
                matematici.dialog.error("server error", "Client unique identifier unavailable");
            });
        };

    setup();
}