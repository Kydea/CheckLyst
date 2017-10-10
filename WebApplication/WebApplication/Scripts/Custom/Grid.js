matematici.Grid = function (selector) {
	var
		grid = this,
		element = $(selector).hide(),
		noRecordRow = $("tr.NoRecord", selector).remove(),
		prototypeRow = $("tbody tr", selector).remove(),
		columnNumber = prototypeRow.children().size(),
		tableBody = $("tbody", element),

		setup = function () {
			$("td", noRecordRow).attr("colspan", columnNumber);
		},

		createRow = function (data) {
			var row = prototypeRow.clone();
			$("td", row).each(function () {
				var cell = $(this);
				cell.html(data[cell.data("column")]);
			});
			return row;
		};

	this.initialize = function () {
		setup();
	}

	this.update = function (rows) {
		console.log(rows);
		tableBody.empty();
		if (!rows || rows.length == 0)
			noRecordRow.clone().appendTo(tableBody);
		else
			for (var index in rows)
				tableBody.append(createRow(rows[index]));
		element.show();
	}
}