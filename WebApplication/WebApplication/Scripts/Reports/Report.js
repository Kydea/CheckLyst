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
        tr,
        startedOn = new Date(),

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
                csvFile += processRow(rows[i].record);

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

        toggleRow = function () {
            var newRow = $(this);
            if (newRow.hasClass("Current")) {
                tr.removeClass("Current");
                tr.prev().removeClass("CurrentPrevious")
                tr = null;
            } else {
                if (tr) {
                    tr.removeClass("Current");
                    tr.prev().removeClass("CurrentPrevious")
                }
                tr = newRow.addClass("Current");
                tr.prev().addClass("CurrentPrevious")
            }
        },

        cancel = function () {
            hamburger.toggle(false);
        },

        download = function () {
            hamburger.toggle(false);
            createCSV(options.code + ".csv", model.columns, model.rows)
        },

        print = function () {
            hamburger.toggle(false);

            var
                select = $("<select />"),
                content = $("<label />").text("Zoom factor").append(select);

            for (var index = 10; index > 0; --index)
                select.append($("<option />").text(index * 10 + "%").attr("value", index / 10));

            setTimeout(function () {
                matematici.dialog.show("print scale", content, [
                    {
                        text: "OK", icon: "check", defaultButton: true, handler: function () {
                            $("body").css("zoom", select.val());
                            setTimeout(function () {
                                window.print();
                            }, 0);
                        }
                    },
                    { hidden: true, cancelButton: true }
                ]);
            }, 0);
        },

        quit = function () {
            hamburger.toggle(false);
            window.close();
        },

        showMenu = function () {
            matematici.dialog.popupMenu([
                { text: "cancel", icon: "times", cancelButton: true, handler: cancel },
                { text: "quit", icon: "door-closed", handler: quit },
                { text: "download", icon: "download", handler: download },
                { text: "print", icon: "print", handler: print }
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
                $("<th />").appendTo(tr).text("");
                for (var columnIndex in result.columns)
                    $("<th />").appendTo(tr).text(result.columns[columnIndex]);
                noRecord.children().eq(0).attr("colspan", result.columns.length || 1)
                initialized = true;
            }

            detailRowIndex = 1;
            if (result.rows.length)
                for (var rowIndex in result.rows) {
                    var
                        row = result.rows[rowIndex],
                        tr = $("<tr />").appendTo(tableBody);
                    if (row.total) {
                        tr.addClass("Total");
                        $("<td />").appendTo(tr).text("");
                    } else 
                        $("<td />").appendTo(tr).text(detailRowIndex++);
                    for (var columnIndex in row.record)
                        $("<td />").appendTo(tr).text(row.record[columnIndex]);
                }
            else
                noRecord.appendTo(tableBody);

            console.log("Elapsed time: " + (new Date() - startedOn) + "ms");
        },

        load = function (page) {
            var request = $.ajax({
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

            matematici.loader.setCancelHandler(function () {
                request.abort("operation canceled");
                loadHandler({
                    description: "operation canceled",
                    columns: [""],
                    rows: []
                });
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
            tableBody = $("table tbody", selector).on("click", "tr", toggleRow);
            noRecord = $("tr.NoRecord", selector).remove();
            load(1);
        };

    setup();
};