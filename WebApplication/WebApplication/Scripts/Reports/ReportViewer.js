reports.ReportViewer = function (element, options) {
    var
        reportViewer = this,
        form = $("form", element),
        code,
        inputs = [],

        run = function () {
            var queryStrings = [];
            for (var parameterIndex in inputs) {
                var
                    parameter = inputs[parameterIndex],
                    value = parameter.getValue();
                if (value != "" && !parameter.getOption("readOnly"))
                    queryStrings.push(encodeURIComponent(parameter.getOption("parameterName")) + "=" + encodeURIComponent(value));
            }
            window.open(options.reportUrl + "?" + queryStrings.join("&"));
        },

        prepareParameters = function () {
            var parameters = [];
            for (var parameterIndex in inputs) {
                var
                    parameter = inputs[parameterIndex],
                    readOnly = parameter.getOption("readOnly"),
                    code = parameter.getOption("parameterName"),
                    value = parameter.getValue();
                if (value != "" && !readOnly)
                    parameters.push({ code: code, value: value });
            }
            return parameters;
        },

        enqueue = function () {
            var data = {
                id: options.id,
                parameters: prepareParameters()
            };
            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/Enqueue",
                data: data
            }).done(function () {
                matematici.dialog.show("report enqueued", "the report has been queued for background execution as requested, you will be notified once it is completed");
                matematici.notificationPollingClient.enable();
            });
        },

        download = function () {
            new reports.PDFDialog(options, null, prepareParameters()).show();
        },

        query = function () {
            var queryParameters = [];
            for (var parameterIndex in inputs) {
                var
                    parameter = inputs[parameterIndex],
                    value = parameter.getValue();
                if (value != "" && !parameter.getOption("readOnly"))
                    queryParameters.push({ code: parameter.getOption("parameterName"), value: value });
            }

            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/GetReportQuery",
                data: {
                    id: options.id,
                    parameters: queryParameters
                }
            }).done(function (result) {
                matematici.dialog.show("report query", result.split(" ").join("&nbsp;").split("\n").join("<br />"), [{
                    hidden: true,
                    cancelButton: true
                }, {
                    text: "copy",
                    icon: "copy",
                    defaultButton: true,
                    handler: function () {
                        matematici.clipboard.copy(result);
                    }
                }]);
            });
        },

        quit = function () {
            var editor = matematici.tokenManager.checkRole("Reports#Edit#");

            var
                closeTab = function () {
                    window.close();
                },
                openEditor = function () {
                    matematici.navigator.open(options.editorUrl);
                },
                openList = function () {
                    matematici.navigator.open(options.listUrl);
                };

            if (editor) {
                var buttons = [
                    { text: "cancel", icon: "times", cancelButton: true },
                    { text: "report editor", icon: "pencil", handler: openEditor }
                ];

                if (window.opener)
                    buttons.push({ text: "close the tab", icon: "door-closed", handler: closeTab });
                else
                    buttons.push({ text: "report list", icon: "list", handler: openList });
                matematici.dialog.popupMenu(buttons);
            } else
                openList();
        },

        completeHandler = function (result) {
            code = result.code;

            if (result.favourite)
                options.removeFromFavouritesButton.removeClass("Hidden");
            else
                options.addToFavouritesButton.removeClass("Hidden");

            matematici.breadcrumbs.set([
                { content: "report viewer", label: "page" },
                { content: result.code, label: "code" }
            ]);

            $("h2", element).text(result.description == null || result.description == "" ? result.code : result.description);

            if (result.clauses.length)
                for (var index = 0; index < result.clauses.length; ++index) {
                    var
                        clause = result.clauses[index],
                        label = $("<label />").appendTo(form).append($("<span />").text(clause.label)),
                        input = new matematici.TypedInput(label[0], clause.parameterType, {
                            parameterName: clause.parameterName,
                            readOnly: !clause.editable
                        });

                    input.setValue(clause.clause.value);
                    inputs.push(input);
                }
            else
                label = $("<div />").addClass("NoParameter").appendTo(form).text("(this report has no parameter)");
        },

        addToFavourites = function () {
            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/ToggleFavourite",
                data: {
                    reportCode: code,
                    value: true
                },
            }).done(function () {
                options.addToFavouritesButton.addClass("Hidden");
                options.removeFromFavouritesButton.removeClass("Hidden");
            });
        },

        removeFromFavourites = function () {
            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/ToggleFavourite",
                data: {
                    reportCode: code,
                    value: false
                },
            }).done(function () {
                options.addToFavouritesButton.removeClass("Hidden");
                options.removeFromFavouritesButton.addClass("Hidden");
            });
        },

        setup = function () {
            reports.reportViewer = reportViewer;
            options.addToFavouritesButton.on("click", addToFavourites);
            options.removeFromFavouritesButton.on("click", removeFromFavourites);

            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/Viewer/LoadParameters",
                data: {
                    id: options.id
                },
            }).done(function (result) {
                completeHandler(result);
            });
        };

    reportViewer.quit = quit;
    reportViewer.run = run;
    reportViewer.download = download;
    reportViewer.enqueue = enqueue;
    reportViewer.query = query;

    setup();
};