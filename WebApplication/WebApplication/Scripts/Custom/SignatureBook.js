matematici.SignatureBook = function (code) {
    var
        element,
        header,
        container,
        body,

        setup = function () {
            element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
            container = $("<div />").addClass("Container").appendTo(element);
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
                .append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("fa fa-remove")).on("click", function () {
                    element.remove();
                }))
                .append(next = $("<a />").addClass("Button").text("next").prepend($("<i />").addClass("fa fa-caret-right")).on("click", function () {
                    ++page;
                    load();
                }))
                .append(previous = $("<a />").addClass("Button").text("previous").prepend($("<i />").addClass("fa fa-caret-left")).on("click", function () {
                    --page;
                    load();
                }));
            load();
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
                    header.append($("<th />").text(column));

                for (var row in values) {
                    var tr = $("<tr />").appendTo(body).addClass("Handler");
                    for (var column in values[row])
                        tr.append($("<td />").html(values[row][column])).data({ index: parseInt(row) + 20 * page, row: values[row] });
                }
            }
        },

        load = function () {
            $.ajax(matematici.urls.instances.externalSource, {
                method: "post",
                data: { Name: instances.instance.getExternalSources()[name], Page: page }
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