matematici.CheckListGrid = function (values, handler, renderHandler, columns) {
	var
		element,
		container,
		parameters,
		self = this,

		setup = function () {
			var
				tr,
				body;

			matematici.elements.body.addClass("CheckListGrid");
			element = $("<div />").addClass("CheckListGrid").appendTo(matematici.elements.body);
			container = $("<div />").addClass("Container").appendTo(element);

			renderValues();
		},

		renderValues = function () {
			container.empty();

			if (parameters)
				container.append(parameters);

			$("<table />").addClass("Grid").appendTo(container)
				.append($("<thead />")
					.append(tr = $("<tr />"))
				).append(
				body = $("<tbody />")
				).on("click", "tr.Handler", function () {
					if (!handler || !handler.call(this, self, $(this).data("row")))
						element.remove();
				});

			var
				buttons =
					$("<div />").addClass("Buttons").appendTo(container)
						.append($("<a />").addClass("Button").text("close").prepend($("<i />").addClass("fi flaticon-times")).on("click", function () {
							element.remove();
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
						tr = $("<tr />").data("row", row).appendTo(body).toggleClass("Handler", !!handler);
						for (var columnindex in columns) {
							var column = columns[columnindex];
							if (!renderHandler || !renderHandler.call(this, rowIndex, columnIndex, row[column.name])) {
								var
									td = $("<td />"),
									value = row[column.name];
								if (column.align == "center")
									td.addClass("Centered");
								if (column.type == "DateTimeOffset") {
									var formattedDate = matematici.dateFormatter.format(new Date(value), true);
									td.append($("<span />").html(formattedDate.date + " " + formattedDate.time).attr("title", formattedDate.offset));
								} else if (value === true)
									td.append($("<i />").addClass("fi flaticon-check"));
								else if (value === false)
									td.append($("<i />").addClass("fi flaticon-times"));
								else 
									td.html(value || "")
								tr.append(td);
							}
						}
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

	setup();

	this.setValues = function (v) {
		values = v;
		renderValues();
	};

	this.getParameters = function () {
		container.prepend(parameters = $("<div />").addClass("Parameters"));
		return parameters;
	}

	this.close = function () {
		element.remove();
		matematici.elements.body.removeClass("CheckListGrid");
	}
}