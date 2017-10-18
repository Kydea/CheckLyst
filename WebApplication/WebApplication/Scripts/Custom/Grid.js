matematici.Grid = function (selector) {
	var
		grid = this,
		element = $(selector).hide(),
		noRecordRow = $("tr.NoRecord", selector).remove(),
		prototypeRow = $("tbody tr", selector).remove(),
		columnNumber = prototypeRow.children().size(),
		tableBody = $("tbody", element),
		paginator,

		setup = function () {
			$("td", noRecordRow).attr("colspan", columnNumber);
		},

		setCellValue = function (cell, data) {
			var value = data[cell.data("column")];
			if (!!value)
				switch (cell.data("type")) {
					case "DateTimeOffset":
						var formattedDate = matematici.dateFormatter.format(new Date(value), true);
						cell.append($("<span />").html(formattedDate.date + " " + formattedDate.time).attr("title", formattedDate.offset));
						break;
					case "Boolean":
						cell.addClass("Centered").append($("<i />").addClass("fa").addClass("fa-" + (value || value == true ? "check" : "times")));
						break;
					default:
						cell.html(value);
				}
		},

		createRow = function (data) {
			var row = prototypeRow.clone();
			$("td", row).each(function () {
				setCellValue($(this), data);
			});
			return row;
		};

	this.initialize = function () {
		setup();
	};

	this.update = function (data) {
		tableBody.empty();

		if (!data || data.length == 0)
			noRecordRow.clone().appendTo(tableBody);
		else
			for (var index in data)
				tableBody.append(createRow(data[index]));
		element.show();
	};
};