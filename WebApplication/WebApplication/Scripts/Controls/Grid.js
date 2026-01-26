matematici.Grid = function (selector, options) {
    var
        grid = this,
        element = $(selector).hide(),
        noRecordRow = $("tr.NoRecord", selector).remove(),
        prototypeRow = $("tbody tr", selector).remove(),
        tableBody = $("tbody", element),
        columnsDomElementsMap = new Map(),
        defaultSettings,
        settings,
        savedData,

        initialize = function () {
            element.appendTo($("<div style='overflow: auto' />").insertBefore(element));
            $("td", noRecordRow).attr("colspan", prototypeRow.children().length);

            options = options || {};

            if (options.rowClick)
                element.on("click", "tr.Handler", function () {
                    options.rowClick($(this));
                });

            if (options.handleSettings) {
                defaultSettings = { columns: [] };
                var
                    tds = $("td", prototypeRow),
                    ths = $("thead tr th", element);

                tds.each(function (index) {
                    const
                        name = this.getAttribute("data-column"),
                        th = ths.eq(index),
                        model = {
                            th: th,
                            td: $(this),
                            advancedSearch: th.hasClass("AdvancedSearch"),
                            label: th.text()
                        };

                    columnsDomElementsMap.set(name, model);

                    defaultSettings.columns.push({
                        name: name,
                        visible: true,
                    });
                });

                matematici.userSettingsClient.load("GridSetup", options.name, function (data) {
                    if (data == null)
                        settings = defaultSettings;
                    else {
                        settings = JSON.parse(data);
                        applySettings();
                    }
                });
            }
        },

        applySettings = function () {
            for (var column of settings.columns) {
                var domElements = columnsDomElementsMap.get(column.name);
                $("thead tr", element).append(domElements.th.toggleClass("Hidden", !column.visible));
                prototypeRow.append(domElements.td.toggleClass("Hidden", !column.visible));
            }
        },

        setCellValue = function (cell, data) {
            var
                column = cell.data("column"),
                value = data[column],
                type = cell.data("type");

            if (!!value) {
                switch (type) {
                    case "DateTimeOffset":
                        var formattedDate = matematici.dateFormatter.format(moment.parseZone(value));
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
        },

        update = function (data) {
            tableBody.empty();

            if (!data || data.length == 0)
                noRecordRow.clone().appendTo(tableBody);
            else
                for (var index in data)
                    tableBody.append(createRow(data[index]));
            element.show();
        };

    this.initialize = initialize;

    this.applySettings = function (newSettings) {
        settings = newSettings;
        applySettings()
        update(savedData);
    };

    this.getName = function () {
        return options.name;
    };

    this.update = function (data) {
        savedData = data;
        update(data);
    };

    this.getSettings = function () {
        return settings;
    };

    this.getDefaultSettings = function () {
        return defaultSettings;
    };

    this.getColumnDomElements = function (name) {
        return columnsDomElementsMap.get(name);
    };

    this.getElement = function () {
        return element;
    }
};