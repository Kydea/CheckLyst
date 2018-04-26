prototypes.SourcesList = function () {
    var
        model = prototypes.prototype.getModel(),
        element,
        container,
        table,
        body,
        sources,
        noRecord,
        currentIndex,
        removeButton,

        select = function (index) {
        	if (currentIndex != null) body.children().eq(currentIndex).removeClass("Selected").addClass("Handler").children().attr("contenteditable", false);
        	if (model.state == prototypes.Prototype.states.draft)
        	{
        		currentIndex = index;
        		if (currentIndex != null) body.children().eq(currentIndex).addClass("Selected").removeClass("Handler").children(":first-child").attr("contenteditable", true).focus();
        		removeButton.toggle(currentIndex != null);
        	}
        },

        add = function (row) {
            noRecord.remove();
            var value = $("<td />").addClass("SourceColumn");
            if (model.state == prototypes.Prototype.states.draft)
                value.append(sources.clone().val(row.source));
            else
                value.text(row.source);

            $("<tr />")
                .addClass("Handler")
                .appendTo(table)
                .append($("<td />").addClass("CodeColumn").text(row.code))
                .append(value);

            if (model.state == prototypes.Prototype.states.draft) {
            	matematici.canClose = false;
            }
        },

        remove = function () {
            body.children().eq(currentIndex).remove();
            if (body.children().size() == 0) noRecord.appendTo(body);
            select();
            matematici.canClose = false;
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
                    )
                ).append(body = $("<tbody />")
                    .append(noRecord = $("<tr />").addClass("NoRecord")
                        .append($("<td />").attr("colspan", "2").text("(no record)"))
                    )
                ).on("click", "tr.Handler", function () {
                    select($(this).prevAll().size());
                });


            var
                buttons =
                    $("<div />").addClass("Buttons").appendTo(container)
						.append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("fi flaticon-times")).on("click", function () {
                        if (model.state == prototypes.Prototype.states.draft) {
                            model.externalSources = {};
                            body.children().each(function () {
                                var row = $(this);
                                model.externalSources[row.children("td").eq(0).text()] = $("select", row).val();
                            });
                        }
                        element.remove();
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
			for (index in dataSources.data)
				$("<option />").appendTo(sources).text(dataSources.data[index].code).val(dataSources.data[index].code);

            for (var code in model.externalSources)
                add({
                    code: code,
                    source: model.externalSources[code]
                });
        };

    setup();
}