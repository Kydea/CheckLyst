instances.ExternalSources = function (itemId, name) {
    var
        element,
        page = 0,
        parameters,
        columns,
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
                .append($("<label />").text(parameter.label)
                    .append($("<input />").attr({ type: "text", id: parameter.name }).on("change, keydown", filterChanged))
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
                    var
                        tr = $(this),
                        row = tr.data("row"),
                        data = {};

                    for (var index = 0; index < columns.length; ++index)
                        data[index] = { key: columns[index], value: row[index] };

                    new instances.Event(instances.Events.externalSourceRowChange, itemId, { rowIndex: tr.data("index"), row: data }).trigger();
                    element.remove();
                });

            $("<div />").addClass("Buttons").appendTo(container)
                .append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("Icon fa fa-times")).on("click", function () {
                    element.remove();
                }))
				.append(next = $("<a />").addClass("Button").text("next").prepend($("<i />").addClass("Icon fa fa-caret-right")).on("click", function () {
                    ++page;
                    load();
                }).hide())
				.append(previous = $("<a />").addClass("Button").text("previous").prepend($("<i />").addClass("Icon fa fa-caret-left")).on("click", function () {
                    --page;
                    load();
                }).hide())
				.append(search = $("<a />").addClass("Button").text("search").prepend($("<i />").addClass("Icon fa fa-search")).on("click", function () {
                    load();
                }));

			$.ajax({
				type: "post",
				url: matematici.urls.dataSources.api + "/Data/GetDataSource",
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				data: JSON.stringify({
					code: instances.instance.getExternalSources()[name]
				})
			}).done(function (result) {
				for (var index in result.parameters)
                    addFilter(result.parameters[index]);

                columns = result.columns;

                for (var column in columns)
                    if (columns[column][0] != "_")
                        header.append($("<th />").text(columns[column].split("_").join(" ")));
			});
        },

        getValues = function () {
            var values = {};
            $("input", parameters).each(function (index) {
                var input = $(this);
				values[index] = {
					key: input.attr("id"),
					value: input.val()
				}
            });
            return values;
        },

        fill = function (rows) {
            body.empty();
            previous.toggle(page > 0);
            next.toggle(rows.length > 0 && rows.length >= 20);
            if (!rows || rows.length == 0) 
                body.append($("<tr />").addClass("NoRecord").append($("<td />").attr("colspan", header.children().size()).text("(no record)")))
            else 
                for (var row in rows) {
                    var tr = $("<tr />").appendTo(body).addClass("Handler");
                    for (var column in rows[row])
                        if (column[0] != "_")
                            tr.append($("<td />").html(rows[row][column])).data({ index: parseInt(row) + 20 * page, row: rows[row] });
                }
        },

		load = function () {
			$.ajax(matematici.urls.instances.api + "/GetExternalSourceRows", {
				method: "post",
				data: { data: { instanceCode: instances.instance.getCode(), parameters: getValues(), externalSourceCode: name, page: page } }
            }).done(function(result) {
                fill(JSON.parse(result).rows);
            });
        };

    setup();
}