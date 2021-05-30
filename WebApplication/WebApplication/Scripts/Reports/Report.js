reports.Report = function (selector, options) {
    var
        report = this,
        model,
        initialized = false,
        h1,
        tableHead,
        tableBody,
        noRecord,
        hamburger,
        parameters = [],

        createCSV = function (fileName, columns, rows) {
            var processRow = function (row) {
                var finalValue = "";
                for (var j = 0; j < row.length; j++) {
                    var innerValue = row[j] === null ? "" : row[j].toString();
                    if (row[j] instanceof Date)
                        innerValue = row[j].toLocaleString();
                    var result = innerValue.replace(/"/g, '""');
                    if (result.search(/("|,|\n)/g) >= 0)
                        result = "\"" + result + "\"";
                    if (j > 0)
                        finalValue += ";";
                    finalValue += result;
                }
                return finalValue + "\n";
            };

            var csvFile = "";
            csvFile += processRow(columns);
            for (var i = 0; i < rows.length; i++)
                csvFile += processRow(rows[i]);

            var blob = new Blob([csvFile], { type: 'text/csv;charset=utf-8;' });
            if (navigator.msSaveBlob) { // IE 10+
                navigator.msSaveBlob(blob, fileName);
            } else {
                var link = document.createElement("a");
                if (link.download !== undefined) {
                    var url = URL.createObjectURL(blob);
                    link.setAttribute("href", url);
                    link.setAttribute("download", fileName);
                    link.className = "Standard";
                    link.style.visibility = 'hidden';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                } else
                    alert("this browser does not support javascript downloads");
            }
        },

        cancel = function () {
            hamburger.toggle(false);
        },

        download = function () {
            hamburger.toggle(false);
            createCSV(options.code + ".csv", model.columns, model.rows)
        },

        quit = function () {
            hamburger.toggle(false);
            window.close();
        },

        showMenu = function () {
            matematici.dialog.popupMenu([
                { text: "cancel", icon: "times", cancelButton: true, handler: cancel },
                { text: "quit", icon: "door-closed", handler: quit },
                { text: "download", icon: "download", handler: download }
            ]);
        },

        loadHandler = function (result) {
            model = result;
            if (initialized)
                tableBody.empty();
            else {
                h1.html(h1.html() + " - " + result.description);
                hamburger = new matematici.Hamburger(selector + " h1 button", showMenu)

                var tr = $("<tr />").appendTo(tableHead);
                for (var columnIndex in result.columns)
                    $("<th />").appendTo(tr).text(result.columns[columnIndex]);
                noRecord.attr("colspan", result.columns.length || 1)
                initialized = true;
            }
            if (result.rows.length)
                for (var rowIndex in result.rows) {
                    var
                        row = result.rows[rowIndex],
                        tr = $("<tr />").appendTo(tableBody);
                    for (var columnIndex in row)
                        $("<td />").appendTo(tr).text(row[columnIndex]);
                }
            else
                noRecord.appendTo(tableBody);
        },

        load = function (page) {
            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/GetReportData",
                data: {
                    code: options.code,
                    page: page,
                    parameters: parameters
                }
            }).done(function (result) {
                loadHandler(result);
            });
        },

        setup = function () {
            var tokens = window.location.search.substring(1).split('&');
            for (var index in tokens) {
                var
                    parameter = tokens[index].split('='),
                    code = parameter.length === 2 && parameter[0],
                    value = parameter.length === 2 && parameter[1];

                if (code) parameters.push({ code: code, value: decodeURIComponent(value) });
            }
            h1 = $("h1", selector);
            tableHead = $("table thead", selector);
            tableBody = $("table tbody", selector);
            noRecord = $("tr.NoRecord", selector).remove();
            load(1);
        };

    setup();
};