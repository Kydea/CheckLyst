reports.ReportEditor = function (element, options) {
    var
        reportEditor = this,
        reportSources,
        reportSourceSelect,
        detailPage,
        editor,
        model,

        getQueryColumns = function (handler) {
            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/List/GetColumns",
                data: {
                    query: editor.getValue(),
                    clauses: model.clauses,
                },
                suppressErrors: true
            }).done(function (result) {
                handler(result);
            }).error(function () {
                handler([])
            });
        },

        showColumns = function () {
            getQueryColumns(function (result) {
                new reports.ColumnsList({
                    columns: (result || []).map(function (column) {
                        return "\"" + column + "\"";
                    })
                });
            });
        },

        showClauses = function () {
            getQueryColumns(function (result) {
                new reports.ClausesList({
                    columns: (result || []).map(function (column) {
                        return "\"" + column + "\"";
                    })
                });
            });
        },

        showGroups = function () {
            new reports.GroupsList();
        },

        quit = function () {
            matematici.navigator.open(options.listUrl);
        },

        parametersPrepare = function () {
            return {
                data: {
                    restricted: model.fields && model.fields.restricted && model.fields.restricted.value == "true",
                    query: reportSourceSelect.val() == null ? editor.getValue() : null,
                    columns: model.columns,
                    clauses: model.clauses,
                    groups: model.groups
                }
            };
        },

        save = function (callback) {
            if (!callback && options.code == "")
                callback = function () {
                    matematici.navigator.open(options.detailUrl + "/" + $("input[name=code]", detailPage.getElement()).val());
                };

            detailPage.clickHandler(null, "Save", callback);
        },

        verify = function (soft, callBack) {
            detailPage.getForm().trigger("Verify", function (data) {
                var valid = validate(!soft, data);
                if (valid)
                    matematici.blip.ok();
                else if (soft)
                    matematici.blip.warning();
                callBack && callBack(data);
            }, parametersPrepare());
        },

        view = function () {
            matematici.navigator.open(options.viewerUrl);
        },

        deleteRecord = function () {
            detailPage.deleteRecord(function () {
                matematici.canClose = true;
                quit();
            });
        },

        setupReportSources = function (list) {
            reportSources = {};

            for (var index in list) {
                var reportSource = list[index];
                reportSources[reportSource.code] = reportSource;
                reportSourceSelect.append($("<option />").attr("value", reportSource.code).text(reportSource.code));
            }
        },

        reportSourceChanged = function () {
            var
                reportSourceCode = reportSourceSelect.val(),
                reportSource = reportSourceCode && reportSources[reportSourceCode];
            editor.setReadOnly(reportSource != null);
            editor.setValue(reportSource && reportSource.query || "", -1);
        },

        validate = function (showErrorDialog, data) {
            var valid = true;
            $("div.ActionBar a.ColumnsButton").toggleClass("Invalid", false);
            $("div.ActionBar a.ClausesButton").toggleClass("Invalid", false);
            editor.setError();

            if (data && data.valid == false) {
                valid = false;
                var
                    errors = [],
                    error = $("> div.Errors", detailPage.getForm().getElement()).text();

                if (error && error != "") errors.push(error);

                if (data.columnErrors) {
                    errors.push("Some columns have errors");
                    $("div.ActionBar a.ColumnsButton").toggleClass("Invalid", true);
                }

                if (data.clauseErrors) {
                    errors.push("Some clauses have errors");
                    $("div.ActionBar a.ClausesButton").toggleClass("Invalid", true);
                }

                if (data.fields && data.fields.query) {
                    record = data.fields.query.validationMessages[0];
                    errors.push("The query has errors");
                    editor.setError(record.position || 0, record.message, "error");
                }

                if (showErrorDialog)
                    setTimeout(function () {
                        matematici.dialog.error("error", errors.join("<br />"));
                    }, 0)
            }

            model.columnErrors = data.columnErrors;
            model.clauseErrors = data.clauseErrors;
            return valid;
        },

        resultHandler = function (data) {
            validate(true, data);
        },

        completeHandler = function (reportSources) {
            if (reportSources != null && model != null) {
                matematici.breadcrumbs.set([
                    { content: "report editor", label: "page" },
                    { content: options.code || "(new report)", label: "code" }
                ]);

                $("div.ActionBar a.ButtonGroup a.Delete", element).toggleClass("Hidden", options.code == "");

                detailPage = new matematici.DetailPage({
                    readOnly: false,
                    wholePage: true,
                    takeOpenOnSave: true,
                    formSelector: "form#Detail",
                    parametersPrepareHandler: parametersPrepare,
                    editEvent: "Save",
                    resultHandler: resultHandler
                });

                matematici.save = function (callback) {
                    setTimeout(function () {
                        save(callback)
                    }, 0);
                };

                var form = detailPage.getForm();
                form.handleResult(model);
                form.setValue("createdBy", model.createdBy);
                form.setValue("createdOn", model.createdOn);
                form.setValue("modifiedBy", model.modifiedBy);
                form.setValue("modifiedOn", model.modifiedOn);

                setupReportSources(reportSources);
                var autoComplete = matematici.AutoComplete.get(reportSourceSelect[0]);
                autoComplete.refreshValues();
                autoComplete.setValue(model.fields.reportSource.value);
                reportSourceChanged();
                if (reportSourceSelect.val() == null) editor.setValue(model.fields.query.value || "", -1);
                editor.setChangeHandler(function () {
                    matematici.canClose = false;
                });
            }
        },

        setup = function () {
            var
                reportSources = null;

            reports.reportEditor = reportEditor;
            reportSourceSelect = $("select[name=reportSource]", element).on("change", reportSourceChanged);
            editor = new matematici.SQLEditor("AceEditor");

            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/List/GetReportSources"
            }).done(function (result) {
                reportSources = result;
                completeHandler(reportSources);
            });

            if (options.code)
                $.ajax({
                    type: "post",
                    url: matematici.urls.reports.api + "/List/Load",
                    data: {
                        data: {
                            code: options.code
                        }
                    }
                }).done(function (result) {
                    model = result;
                    completeHandler(reportSources);
                });
            else
                model = {
                    fields: {
                        query: { value: null },
                        reportSource: { value: null },
                        columns: [],
                        clauses: []
                    }
                };
        };

    reportEditor.getModel = function () {
        return model;
    };

    reportEditor.showColumns = showColumns;
    reportEditor.showClauses = showClauses;
    reportEditor.showGroups = showGroups;
    reportEditor.quit = quit;
    reportEditor.save = save;
    reportEditor.verify = verify;
    reportEditor.view = view;
    reportEditor.deleteRecord = deleteRecord;

    setup();
};