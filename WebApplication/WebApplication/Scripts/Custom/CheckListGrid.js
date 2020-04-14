matematici.CheckListGrid = function (values, handler, renderHandler, columns, rowRenderHandler) {
	var
		checkListGrid = this,
		element,
		container,
		parameters,
		body,

        setup = function () {
            matematici.checkListGrid = checkListGrid;
			matematici.elements.body.addClass("CheckListGrid");
			element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
			container = $("<div />").addClass("Container").appendTo(element);

			renderValues();
		},

		addRow = function (row, cssClass, rowIndex) {
			var tr = $("<tr />").data("row", row).appendTo(body).toggleClass("Handler", !!handler);
			if (cssClass) tr.addClass(cssClass);
			for (var columnIndex in columns) {
				var column = columns[columnIndex];
				if (!renderHandler || !renderHandler(tr, row, rowIndex, columnIndex, row[column.name])) {
					var
						td = $("<td />"),
						value = column.name ? row[column.name] : rowIndex;
					if (column.align == "center")
						td.addClass("Centered");
					if (column.type == "DateTimeOffset") {
						if (value) {
							var formattedDate = matematici.dateFormatter.format(moment.parseZone(value));
                            td.append($("<span />").html(formattedDate.date + " " + formattedDate.time + ":" + formattedDate.seconds).attr("title", formattedDate.offset));
						}
                    } else if (value === true)
                        td.append($("<i />").addClass("fa fa-check")).addClass("Centered");
					else if (value === false)
                        td.append($("<i />").addClass("fa fa-times")).addClass("Centered");
					else
						td.html(value)
					tr.append(td);
				}
			}

			return tr;
		},

		renderValues = function () {
			var tr;
			container.empty();

			if (parameters)
				container.append(parameters);

			$("<table />").addClass("Grid").appendTo(container)
				.append($("<thead />")
					.append(tr = $("<tr />"))
				).append(
				body = $("<tbody />")
				).on("click", "tr.Handler", function () {
					if (!handler || !handler.call(this, checkListGrid, $(this).data("row")))
						element.remove();
				});

			var
				buttons =
					$("<div />").addClass("Buttons").appendTo(container)
						.append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("Icon fa fa-times")).on("click", function () {
							checkListGrid.close();
						}));

			if (values.length == 0) {
				tr.append($("<th />").text("-"));
				body.append($("<tr />").addClass("NoRecord").append($("<td />").attr("colspan", 1).text("(no record)")))
			} else {
				if (columns) {
					for (var column in columns) 
						tr.append($("<th />").text(columns[column].label));
					for (var rowIndex in values) {
						var row = values[rowIndex];
						if (!rowRenderHandler || !rowRenderHandler(checkListGrid, rowIndex, row))
							addRow(row, null, rowIndex);
					}
				} else {
					var row = values[0];
					var isArray = Array.isArray(row);

					for (var column in row) {
						var label = isArray ? row[column] : column;
						label = (label || "").replace("_", " ");
						if (!renderHandler || !renderHandler.call(this, row, column, label))
							tr.append($("<th />").text(label));
					}

					var rows = isArray ? values.slice(1) : values;
					for (var row in rows) {
						tr = $("<tr />").data("row", rows[row]).appendTo(body).toggleClass("Handler", !!handler);
						for (var column in rows[row]) {
							if (!renderHandler || !renderHandler.call(this, row, column, rows[row][column])) {
								tr.append($("<td />").html((rows[row][column] == null || rows[row][column] == undefined ? "" : rows[row][column]).toString()))
							}
						}
					}
				}
			}
		};

	checkListGrid.setValues = function (v) {
		values = v;
		renderValues();
	};

	checkListGrid.getParameters = function () {
		container.prepend(parameters = $("<div />").addClass("Parameters Module"));
		return parameters;
	}

	checkListGrid.close = function () {
		element.remove();
		matematici.elements.body.removeClass("CheckListGrid");
        matematici.checkListGrid = null;
	}

	checkListGrid.addRow = addRow;

	setup();
}