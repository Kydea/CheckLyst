matematici.Grid = function (selector, options) {
    var
        grid = this,
        element = $(selector).hide(),
        noRecordRow = $("tr.NoRecord", selector).remove(),
        prototypeRow = $("tbody tr", selector).remove(),
        columnNumber = prototypeRow.children().size(),
        tableBody = $("tbody", element),
        paginator,

        setup = function () {
            element.appendTo($("<div style='overflow: auto' />").insertBefore(element));
            $("td", noRecordRow).attr("colspan", columnNumber);

            options = options || {};

            if (options.rowClick)
                element.on("click", "tr.Handler", function () {
                    options.rowClick($(this));
                });
        },

        setCellValue = function (cell, data) {
            var
                column = cell.data("column"),
                value = data[column],
                type = cell.data("type");
            if (!!value) {
                switch (type) {
                    case "DateTimeOffset":
                        var formattedDate = matematici.dateFormatter.format(moment.parseZone(value), true);
                        cell.append($("<span />").html(formattedDate.date + " " + formattedDate.time).attr("title", formattedDate.offset));
                        break;
                    case "Boolean":
                        cell.addClass("Centered").append($("<i />").addClass("fa").addClass("fa-" + (value || value == true ? "check" : "times")));
                        break;
                    case "Custom":
                        break;
					case "Text":
						cell.text(value);
						break;
                    default:
                        cell.html(value);
                }
                if (options.setCellValueHandler)
                    options.setCellValueHandler(cell, data, column, type, value);
            }
        },

        createRow = function (data) {
            var row = prototypeRow.clone();

            row.data("values", data);

            $("td", row).each(function () {
                setCellValue($(this), data);
            });
            return row;
        };

    this.updateCell = function (row, columnName, value) {
        setCellValue()
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

    this.getElement = function () {
        return element;
    }
};