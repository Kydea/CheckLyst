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
            }).fail(function () {
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
            if (window.opener)
                window.close();
            else
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
            if (!callback)
                callback = options.id >= 0 && !options.newVersion ?
                    function (result) {
                        $("input[name=modifiedBy]", element).val(result.modifiedBy);
                        var format = matematici.dateFormatter.format(moment(result.modifiedOn));
                        $("input[name=modifiedOn]", element).val(format.date + " " + format.time).attr("title", format.offset);
                    } :
                    function (result) {
                        //reloads the page with the correct url
                        matematici.navigator.open(options.detailUrl + "/" + result.id);
                    };

            detailPage.clickHandler(null, "Save", callback);
        },

        verify = function (soft, callBack) {
            var form = detailPage.getForm();
            form.trigger("Verify", function (data) {
                form.handleResult(data);
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
            if (!model.code || options.newVersion)
                quit();
            else
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
            editor.setReadOnly(reportSource != null || !model.editable);
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

                if (data.columnErrors && data.columnErrors.length) {
                    errors.push("Some columns have errors");
                    $("div.ActionBar a.ColumnsButton").toggleClass("Invalid", true);
                }

                if (data.clauseErrors && data.clauseErrors.length) {
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
                if (model.code) {
                    $("h2", element).text(model.code + (options.newVersion ? " (new version)" : ""));

                    matematici.breadcrumbs.set([
                        { content: "report editor", label: "page" },
                        { content: model.code, label: "code" }
                    ]);
                }

                var buttonGroup = $("div.ActionBar a.ButtonGroup", element);
                $("a.History", buttonGroup).toggleClass("Hidden", options.id <= 0);
                $("a.Delete", buttonGroup).toggleClass("Hidden", !model.editable);

                if (options.newVersion) 
                    model.fields.description.editable = true;

                detailPage = new matematici.DetailPage({
                    readOnly: !model.editable,
                    wholePage: true,
                    takeOpenOnSave: true,
                    formSelector: "form#Detail",
                    parametersPrepareHandler: parametersPrepare,
                    editEvent: "Save",
                    resultHandler: resultHandler,
                    ignoreUserReason: true
                });

                matematici.save = function (callback) {
                    setTimeout(function () {
                        save(callback)
                    }, 0);
                };

                var form = detailPage.getForm();
                form.handleResult(model);
                form.setValue("reportState", model.state);
                form.setValue("version", model.version);
                form.setValue("newVersion", !!model.newVersion);
                form.setValue("createdBy", model.createdBy);
                form.setValue("createdOn", model.createdOn);
                form.setValue("modifiedBy", model.modifiedBy);
                form.setValue("modifiedOn", model.modifiedOn);
                form.setValue("proposedState", model.proposedState);
                form.setValue("proposedDate", model.proposedDate);

                setupReportSources(reportSources);
                var autoComplete = matematici.AutoComplete.get(reportSourceSelect[0]);
                autoComplete.refreshValues();
                autoComplete.setValue(model.fields && model.fields.reportSource && model.fields.reportSource.value);
                reportSourceChanged();
                if (reportSourceSelect.val() == null) editor.setValue(model.fields && model.fields.query && model.fields.query.value || "", -1);
                editor.setChangeHandler(function () {
                    matematici.canClose = false;
                });
                editor.setReadOnly(!model.editable);

                options.actionButtonsGroup.toggleByClass("View", model.id != null);
                options.actionButtonsGroup.toggleByClass("Save", model.editable);
                options.actionButtonsGroup.toggleByClass("Delete", model.state == reports.Report.states.draft);
                options.actionButtonsGroup.toggleByClass("Verify", model.editable);
                options.actionButtonsGroup.toggleByClass("SubmitForObsolescence", model.state == reports.Report.states.confirmed && model.proposedState == null);
                options.actionButtonsGroup.toggleByClass("Obsolete", model.state == reports.Report.states.confirmed && model.proposedState == reports.Report.states.obsolete);
                options.actionButtonsGroup.toggleByClass("Complete", model.state == reports.Report.states.draft);
                options.actionButtonsGroup.toggleByClass("Confirm", model.state == reports.Report.states.completed);
            }
        },

        setState = function (state) {
            if (!matematici.canClose) {
                matematici.dialog.error("error", "The report state cannot be changed because unsaved changes are present.");
                return;
            }

            var handler = function () {
                model.State = state;
                $.ajax({
                    url: detailPage.getForm().getElement().attr("action") + "/" + "SetState",
                    type: "post",
                    data: {
                        id: model.id,
                        state: state
                    }
                }).done(function (result) {
                    model = result;
                    completeHandler(reportSources);
                    matematici.blip.ok();
                    matematici.canClose = true;
                });
            };

            matematici.dialog.show("warning", "Please confirm the change of state.", [
                { text: "confirm", icon: "check", handler: handler },
                { text: "cancel", icon: "times", cancelButton: true }
            ]);
        },

        sign = function (action, grid, rows, row) {
            if (row.approved) {
                matematici.dialog.error("error", "This signature has already been issued");
                return true;
            }

            var
                allowed = false,
                tags = row.tags.split(", ");

            for (var role in tags)
                if (matematici.tokenManager.checkRole("ReportWorkFlowLevel####" + tags[role])) {
                    allowed = true;
                    break;
                }

            if (!allowed) {
                matematici.dialog.error("error", "Permission denied");
                return true;
            }

            for (var rowIndex in rows) {
                if (rows[rowIndex].step < row.step && !rows[rowIndex].approved) {
                    allowed = false;
                    break;
                }

                if (rows[rowIndex].id == row.id)
                    break;
            }
            if (!allowed) {
                matematici.dialog.error("error", "The previous step is not completed");
                return true;
            }

            for (var rowIndex in rows) {
                if (rows[rowIndex].step == row.step && rows[rowIndex].user == matematici.tokenManager.getUserCode()) {
                    allowed = false;
                    break;
                }
            }
            if (!allowed) {
                matematici.dialog.error("error", "You have already signed for this step");
                return true;
            }

            workFlowSignature = new matematici.WorkFlowSignature({
                title: model.code + " " + action,
                reason: row.reason,
                report: model.code,
                applicationStep: row.id,
                rows: rows,
                concurrencyInformation: {
                    modifiedBy: model.modifiedBy,
                    modifiedOn: model.modifiedOn
                },

                done: function (result) {
                    if (result.state == reports.Report.states.draft) {
                        location.reload(); //reload with correct editability
                        return;
                    }
                    grid.close();
                    matematici.popup.close();
                    model.state = result.state;
                    model.proposedState = model.proposedDate = null;
                    model.modifiedBy = result.modifiedBy,
                    model.modifiedOn = result.modifiedOn;
                    completeHandler(reportSources);
                    matematici.canClose = true;
                    if (!result.completed)
                        loadSignatureBook();
                    if (result.approved)
                        matematici.blip.ok();
                    else 
                        matematici.blip.ko(); 
                }
            });
            return true;
        },

        loadSignatureBook = function () {
            new matematici.WorkFlowSignatureBook().showForReport(model.code, model.id, sign);
        },

        showHistory = function () {
            $.ajax({
                url: matematici.urls.reports.api + "/List/History",
                type: "post",
                data: {
                    id: model.id
                }
            }).done(function (result) {
                var
                    columns = [
                        { label: "State", name: "state" },
                        { label: "Date", name: "date", type: "DateTimeOffset" },
                        { label: "User", name: "user" },
                        { label: "Approved", name: "approved", type: "Boolean", align: "center" },
                        { label: "Proposed state", name: "proposedState" },
                        { label: "Signature step", name: "step", align: "center" },
                        { label: "Signature reason", name: "reason" },
                        { label: "Signature comment", name: "comment" },
                        { label: "Appointed signer", name: "appointedUser" }
                    ],
                    rowRenderHandler = function (checkListGrid, rowIndex, data) {
                        checkListGrid.addRow(data, rowIndex % 2 == 0 ? "Odd" : "Even");
                        for (var subRowIndex in data.steps) {
                            var
                                subRow = data.steps[subRowIndex],
                                tr = checkListGrid.addRow(subRow, "SubRow " + (subRowIndex % 2 == 0 ? "Odd " : "Even ") + (subRow.approved == true ? "Green" : subRow.approved == false ? "Red" : ""));

                            $("td", tr).eq(0).addClass(rowIndex % 2 == 0 ? "Odd" : "Even");
                        }
                        checkListGrid.refresh();
                        return true;
                    },
                    checkListGrid = new matematici.CheckListGrid("Report " + model.id, result, null, null, columns, rowRenderHandler),
                    parameters = checkListGrid.getParameters(),
                    attributes = { type: "text", readonly: "readonly" };

                parameters.append($("<label />").append($("<span />").text("ID")).append($("<i />").addClass("Icon fal fa-industry")).append($("<input />").attr(attributes).val(model.id)));
                parameters.append($("<label />").append($("<span />").text("Code")).append($("<i />").addClass("Icon fal fa-folder")).append($("<input />").attr(attributes).val(model.code)));
                parameters.append($("<label />").append($("<span />").text("Version")).append($("<i />").addClass("Icon fal fa-list-ol")).append($("<input />").attr(attributes).val(model.version)));
            });
        },

        initialize = function () {
            var
                reportSources = null;

            reports.reportEditor = reportEditor;
            reportSourceSelect = $("select[name=reportSource]", element).on("change", reportSourceChanged);
            workFlowSelect = $("select[name=workFlow]", element);
            editor = new matematici.SQLEditor("AceEditor");

            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/List/GetReportSources"
            }).done(function (result) {
                reportSources = result;
                completeHandler(reportSources);
            });

            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/List/Load",
                data: {
                    data: {
                        id: options.id
                    }
                }
            }).done(function (result) {
                model = result;
                if (options.newVersion) {
                    model.newVersion = true;
                    ++model.version;
                    model.state = reports.Report.states.draft;
                    model.editable = true;
                }
                completeHandler(reportSources);
            });
        };

    reportEditor.getModel = function () {
        return model;
    };

    reportEditor.getDetailPage = function () {
        return detailPage;
    };

    reportEditor.isEditable = function () {
        return model.editable;
    };

    reportEditor.showColumns = showColumns;
    reportEditor.showClauses = showClauses;
    reportEditor.showGroups = showGroups;
    reportEditor.quit = quit;
    reportEditor.save = save;
    reportEditor.verify = verify;
    reportEditor.view = view;
    reportEditor.deleteRecord = deleteRecord;
    reportEditor.loadSignatureBook = loadSignatureBook;
    reportEditor.setState = setState;
    reportEditor.showHistory = showHistory;

    initialize();
};