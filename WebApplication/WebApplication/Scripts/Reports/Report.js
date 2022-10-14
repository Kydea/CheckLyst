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

        renderRows = function (firstRowIndex) {
            if (reports.disableRendering) return; //just for debug

            if (renderTimeout)
                clearTimeout(renderTimeout);
            else
                matematici.loader.show();

            renderTimeout = setTimeout(function (firstRowIndex) {
                var rowsCount = Math.floor(window.innerHeight / 40) + 1;
                if (currentRange && currentRange.firstRowIndex == Math.max(0, firstRowIndex) && currentRange.firstRowIndex == firstRowIndex + rowsCount)
                    return; //nothing to redraw

                //clear previous range
                if (currentRange)
                    for (var rowIndex = currentRange.firstRowIndex; rowIndex <= currentRange.lastRowIndex; ++rowIndex) {
                        var tr = tableBody.children[rowIndex];
                        while (tr.children.length > 1 && tr.removeChild(tr.children[1]));
                    }

                //render new range
                currentRange = { firstRowIndex: Math.max(0, firstRowIndex), lastRowIndex: firstRowIndex + rowsCount }
                for (var rowIndex = currentRange.firstRowIndex; rowIndex <= currentRange.lastRowIndex; ++rowIndex) {
                    var
                        tr = tableBody.children[rowIndex],
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
            if (initialized)
                while (tableBody.firstChild && tableBody.removeChild(tableBody.firstChild));
            else {
                h1.html(h1.html() + result.code + " - " + result.description);
                hamburger = new matematici.Hamburger(selector + " h1 button", showMenu)

                var tr = document.createElement("tr");
                tableHead.appendChild(tr);
                var th = document.createElement("th");
                tr.appendChild(th);
                for (var columnIndex in result.columns) {
                    var th = document.createElement("th");
                    th.innerText = result.columns[columnIndex];
                    tr.appendChild(th);
                }
                noRecord.children().eq(0).attr("colspan", (result.columns.length || 1) + 1)
                initialized = true;
            }

            detailRowIndex = 1;
            if (result.rows.length) {
                var td;
                for (var rowIndex in result.rows) {
                    var
                        row = result.rows[rowIndex],
                        tr = document.createElement("tr");

                    tableBody.appendChild(tr);
                    if (row.total) {
                        tr.className("Total");
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

        scrollHandler = function () {
            if (!tableHead.children) return;

            if (currentRow) toggleRow(currentRow);

            var
                tableTop = tableBody.children[0].getBoundingClientRect().top + window.pageYOffset,
                rowHeight = tableBody.children[0].offsetHeight,
                firstRowIndex = Math.floor((window.scrollY - tableTop) / 40);

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