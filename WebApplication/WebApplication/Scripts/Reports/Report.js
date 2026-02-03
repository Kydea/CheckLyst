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
        resizingElement,

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
            if (navigator.msSaveBlob) // IE 10+
                navigator.msSaveBlob(blob, fileName);
            else {
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
            new reports.PDFDialog(options, tableHead, parameters).show();
        },

        quit = function () {
            hamburger.toggle(false);
            window.close();
        },

        showMenu = function () {
            matematici.dialog.popupMenu([
                { text: "cancel", icon: "times", cancelButton: true, handler: cancel },
                { text: "quit", icon: "door-closed", handler: quit },
                { text: "download csv", icon: "download", handler: download },
                { text: "download pdf", icon: "print", handler: print }
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
            if (result.enqueued) {
                matematici.notificationPollingClient.enable();
                matematici.dialog.show("report enqueued", "the report processing exceeded the time limit and has been queued for background execution, you will be notified once it is completed", [
                    {
                        text: "confirm", icon: "check", cancelButton: true, handler: function () {
                            return window.close();
                        }
                    }
                ]);
                return;
            } 
            model = result;
            var tr, th, td;
            if (initialized)
                while (tableBody.firstChild && tableBody.removeChild(tableBody.firstChild));
            else {
                var title = result.code + " - " + result.description;
                document.title = title;
                h1.html(title);
                hamburger = new matematici.Hamburger(selector + " thead button", showMenu)

                tr = tableHead.children[0];
                //tr = document.createElement("tr");
                //tableHead.appendChild(tr);
                //th = document.createElement("th");
                //tr.appendChild(th);
                for (var columnIndex in result.columns) {
                    th = document.createElement("th");
                    th.innerText = result.columns[columnIndex];
                    tr.appendChild(th);

                    th.addEventListener('mousemove', headerMouseMove);
                    th.addEventListener('mousedown', headerMouseDown);
                    th.addEventListener('mouseup', headerMouseUp);
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

            if (new URLSearchParams(window.location.search).has("test")) {
                console.log("test mode")
                (async () => {
                    const url = matematici.urls.reports.api + "/Viewer/GetReportData";

                    const data = { id: options.id, run: options.run, parameters };
                    const body = $.param(data);

                    const r = await fetch(url, {
                        method: "POST",
                        headers: {
                            "Authorization": "Bearer " + (localStorage.accessToken || ""),
                            ...(localStorage.timeZone ? { "ClientTimeZone": localStorage.timeZone } : {}),
                            "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
                            "X-Requested-With": "XMLHttpRequest"
                        },
                        body
                    });

                    const buf = await r.arrayBuffer();
                    console.log("STATUS:", r.status);
                    console.log("BYTE REALI RICEVUTI:", buf.byteLength);

                    const headSize = 8192;
                    const headBuf = buf.slice(0, headSize);
                    const head = new TextDecoder("utf-8", { fatal: false }).decode(headBuf);

                    console.log("HEAD(first 8KB):");
                    console.log(head);

                    const tailSize = 8192;
                    const tailBuf = buf.slice(Math.max(0, buf.byteLength - tailSize));
                    const tail = new TextDecoder("utf-8", { fatal: false }).decode(tailBuf);

                    console.log("TAIL(last 8KB):");
                    console.log(tail);
                })();

                return;
            }

            var request = $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/GetReportData",
                data: {
                    id: options.id,
                    run: options.run,
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
                tableTop = tableBody.children[0].getBoundingClientRect().top + window.scrollY,
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
        },

        headerMouseMove = function (e) {
            var th = e.target;
            if (resizingElement !== null) {
                if (e.buttons !== 1)
                    headerMouseUp();
                else {
                    var width = Math.max(0, resizingElement.offsetWidth + e.movementX);
                    resizingElement.style.setProperty("min-width", width + "px");
                }
                e.preventDefault();
                return false;
            }

            if (e.offsetX > th.offsetWidth - 20 && !th.classList.contains("tight"))
                th.classList.add("Resizable");
            else
                th.classList.remove("Resizable");
        },

        headerMouseDown = function(e) {
            var th = e.target;
            if (e.buttons === 1 && th.classList.contains("Resizable"))
                resizingElement = th;
        },

        headerMouseUp = function() {
            resizingElement = null;
        };

    setup();
};

reports.Report.states = {
    draft: "Draft",
    completed: "Completed",
    confirmed: "Confirmed",
    obsolete: "Obsolete"
}