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
        currentRow,
        currentRange,
        startedOn = new Date(),
        renderTimeout,
        scrollTop = 0,

        createCSV = function (fileName, columns, rows) {
            var processRow = function (row) {
                var finalValue = "";
                for (var j = 0; j < row.length; j++) {
                    var innerValue = row[j] === null ? "" : row[j].toString();
                    if (row[j] instanceof Date)
                        innerValue = row[j].toLocaleString();
                    var result = innerValue.replace(/"/g, '""');
                    if (result.search(/("|;|\n)/g) >= 0)
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

        toggleRow = function (newRow) {
            if (newRow.hasClass("Current")) {
                currentRow.removeClass("Current");
                currentRow.prev().removeClass("CurrentPrevious")
                currentRow = null;
            } else {
                if (currentRow) {
                    currentRow.removeClass("Current");
                    currentRow.prev().removeClass("CurrentPrevious")
                }
                currentRow = newRow.addClass("Current");
                currentRow.prev().addClass("CurrentPrevious")
            }
        },

        cancel = function () {
            hamburger.toggle(false);
        },

        download = function () {
            hamburger.toggle(false);
            createCSV(model.code + ".csv", model.columns, model.rows)
        },

        print = function () {
            hamburger.toggle(false);
            var
                content = $("<div />"),

                lastFontSize = localStorage.ReportFontSize || "8",
                lastFormat = localStorage.ReportFormat || "4",
                lastOrientation = localStorage.ReportOrientation || "0",

                fontSizeSelect = $("<select />"),
                formatSelect = $("<select />"),
                orientationSelect = $("<select />");


            content.append($("<label />").text("Font size"));
            var index = 0
            for (index = 5; index < 21; ++index) {
                var option = $("<option />").text(index + "px").attr("value", index);
                if (lastFontSize == index)
                    option.attr("selected", "selected");
                fontSizeSelect.append(option);
            }
            content.append(fontSizeSelect);

            content.append($("<label />").text("Paper format").css("margin-top", 10));
            index = 0
            var formats = [
                "A0",
                "A1",
                "A2",
                "A3",
                "A4",
                "A5",
                "A6",
                "B5",
                "Letter",
                "Legal",
                "Ledger"
            ]
            for (index = 0; index < formats.length; ++index) {
                var option = $("<option />").text(formats[index]).attr("value", index);
                if (lastFormat == index)
                    option.attr("selected", "selected");
                formatSelect.append(option);
            }
            content.append(formatSelect);

            content.append($("<label />").text("Orientation").css("margin-top", 10));
            index = 0
            var option = $("<option />").text("Portrait").attr("value", index++);
            if (lastOrientation != 1) option.attr("selected", "selected")
            orientationSelect.append(option);
            option = $("<option />").text("Landscape").attr("value", index++);
            if (lastOrientation == 1) option.attr("selected", "selected")
            orientationSelect.append(option);
            content.append(orientationSelect);


            setTimeout(function () {
                matematici.dialog.show("print", content, [
                    {
                        text: "OK", icon: "check", defaultButton: true, handler: function () {
                            $("body").css("format", formatSelect.val());

                            localStorage.ReportFontSize = fontSizeSelect.val();
                            localStorage.ReportFormat = formatSelect.val();
                            localStorage.ReportOrientation = orientationSelect.val();

                            $.ajax({
                                url: matematici.urls.reports.api + "/Viewer/Download",
                                type: "post",
                                data: {
                                    id: options.id,
                                    parameters: parameters,
                                    fontSize: fontSizeSelect.val(),
                                    format: formatSelect.val(),
                                    orientation: orientationSelect.val()
                                }
                            }).done(function (result) {
                                location = matematici.urls.reports.api + "/Viewer/Download?Token=" + result.token;
                            });
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

        renderRows = function (firstRowIndex) {
            if (reports.disableRendering) return; //just for debug

            if (renderTimeout)
                clearTimeout(renderTimeout);
            else
                matematici.loader.show();

            renderTimeout = setTimeout(function (firstRowIndex) {
                var
                    rowIndex, td, row,
                    rowsCount = Math.floor(window.innerHeight / 40) + 1;
                if (currentRange && currentRange.firstRowIndex === Math.max(0, firstRowIndex) && currentRange.firstRowIndex === firstRowIndex + rowsCount)
                    return; //nothing to redraw

                //clear previous range
                if (currentRange)
                    for (rowIndex = currentRange.firstRowIndex; rowIndex <= currentRange.lastRowIndex; ++rowIndex) {
                        var tr = tableBody.children[rowIndex];
                        while (tr.children.length > 1 && tr.removeChild(tr.children[1]));
                    }

                //render new range
                currentRange = { firstRowIndex: Math.max(0, firstRowIndex), lastRowIndex: Math.min(firstRowIndex + rowsCount, model.rows.length - 1) }
                for (rowIndex = currentRange.firstRowIndex; rowIndex <= currentRange.lastRowIndex; ++rowIndex) {
                    tr = tableBody.children[rowIndex];
                    row = model.rows[rowIndex];
                    for (var columnIndex in row.record) {
                        var value = row.record[columnIndex];
                        td = document.createElement("td");
                        if (value.startsWith("https://") || value.startsWith("http://")) {
                            var anchor = document.createElement("a");
                            anchor.innerText = value;
                            anchor.setAttribute("href", value);
                            anchor.setAttribute("target", "_blank");
                            anchor.className = "Standard";
                            td.appendChild(anchor);
                        } else
                            td.innerText = value;
                        tr.appendChild(td);
                    }
                }

                matematici.loader.hide();
                renderTimeout = null;
            }, 10, firstRowIndex)
        },

        clickHandler = function () {
            var
                cell = $(this),
                content = cell.html();
            toggleRow(cell.parent());
        },

        loadHandler = function (result) {
            console.log("Query time: " + (new Date() - startedOn) + "ms");
            model = result;
            var tr, th, td;
            if (initialized)
                while (tableBody.firstChild && tableBody.removeChild(tableBody.firstChild));
            else {
                h1.html(h1.html() + result.code + " - " + result.description);
                hamburger = new matematici.Hamburger(selector + " h1 button", showMenu)

                tr = document.createElement("tr");
                tableHead.appendChild(tr);
                th = document.createElement("th");
                tr.appendChild(th);
                for (var columnIndex in result.columns) {
                    th = document.createElement("th");
                    th.innerText = result.columns[columnIndex];
                    tr.appendChild(th);
                }
                noRecord.children().eq(0).attr("colspan", (result.columns.length || 1) + 1)
                initialized = true;
            }

            detailRowIndex = 1;
            if (result.rows.length) {
                for (var rowIndex in result.rows) {
                    var row = result.rows[rowIndex];
                    tr = document.createElement("tr");

                    tableBody.appendChild(tr);
                    if (row.total) {
                        tr.className = "Total";
                        td = document.createElement("td")
                        tr.appendChild(td);
                    } else {
                        td = document.createElement("td")
                        td.innerText = detailRowIndex++;
                        tr.appendChild(td);
                    }
                }

                renderRows(0);
            }
            else
                noRecord.appendTo(tableBody);

            console.log("Total time: " + (new Date() - startedOn) + "ms");
        },

        load = function () {
            var request = $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/GetReportData",
                data: {
                    id: options.id,
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

        scrollHandler = function (e) {
            if (!tableHead.children || scrollTop === window.scrollY) return;
            scrollTop = window.scrollY;

            if (currentRow) toggleRow(currentRow);

            var
                tableTop = tableBody.children[0].getBoundingClientRect().top + window.pageYOffset,
                rowHeight = tableBody.children[0].offsetHeight,
                firstRowIndex = Math.floor((window.scrollY - tableTop) / 40);

            //console.log(e);
            renderRows(firstRowIndex);
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
            window.onscroll = scrollHandler;
            h1 = $("h1", selector);
            tableHead = $("table thead", selector)[0];
            tableBody = $("table tbody", selector).on("click", "td", clickHandler)[0];
            noRecord = $("tr.NoRecord", selector).remove();
            load(1);
        };

    setup();
};