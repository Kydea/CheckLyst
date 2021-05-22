reports.Report = function (element, options) {
    var
        report = this,
        justLoaded = true,
        reportSources,
        reportSourceSelect,
        form,
        editor,
        model,

        save = function () {

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

        setupSQLEditor = function () {
            editor = ace.edit("AceExpressionEditor");
            editor.session.setMode("ace/mode/sql");
            editor.setTheme("ace/theme/custom")
            editor.setShowPrintMargin(false);
            editor.setOptions({
                enableBasicAutocompletion: true
            });
            editor.on("input", function () {
                if (justLoaded)
                    justLoaded = false;
                else
                    matematici.canClose = false;
            });

            var document = editor.session.doc;
            document.applyAnyDelta = document.applyAnyDelta || document.applyDelta;
            document.applyDelta = function (delta) {
                var joinedLines = delta.lines.join("\n");

                if (delta.action == "insert" && this.$maxLength && this.getValue().length + joinedLines.length > this.$maxLength) {
                    var newPasteLength = this.$maxLength - this.getValue().length;
                    if (newPasteLength > 0) {
                        delta.lines = joinedLines.substr(0, newPasteLength).split("\n");
                        delta.end = delta.lines.length == 1 && delta.start.row == delta.end.row ?
                            {
                                row: delta.start.row,
                                column: delta.start.column + newPasteLength
                            } :
                            {
                                row: delta.start.row + delta.lines.length,
                                column: delta.lines[delta.lines.length - 1].length
                            }
                    } else
                        return false;
                }
                return this.applyAnyDelta(delta);
            }

            document.$maxLength = 10000;
        },

        completeHandler = function (reportSources) {
            if (reportSources != null && model != null) {
                form = new matematici.Form("form#Detail");
                form.initialize();
                form.handleResult(model);
                form.setValue("createdBy", model.createdBy);
                form.setValue("createdOn", model.createdOn);
                form.setValue("modifiedBy", model.modifiedBy);
                form.setValue("modifiedOn", model.modifiedOn);
                form.addChangeHandler(function () {
                    matematici.canClose = false;
                });

                setupReportSources(reportSources);
                editor.setValue(model.fields.query.value || "", -1);
                var autoComplete = matematici.AutoComplete.get(reportSourceSelect[0]);
                autoComplete.refreshValues();
                autoComplete.setValue(model.fields.reportSource.value);
                reportSourceChanged();
            }
        },

        setup = function () {
            var
                reportSources = null;

            matematici.report = report;
            reportSourceSelect = $("select[name=reportSource]", element)
                .on("change", reportSourceChanged);
            setupSQLEditor();

            $.ajax({
                type: "post",
                url: matematici.urls.reports.api + "/GetReportSources"
            }).done(function (result) {
                reportSources = result;
                completeHandler(reportSources);
            });

            if (options.code)
                $.ajax({
                    type: "post",
                    url: matematici.urls.reports.api + "/Load",
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
                        reportSource: { value: null }
                    }
                };
        };

    report.save = save;
  
    setup();
};