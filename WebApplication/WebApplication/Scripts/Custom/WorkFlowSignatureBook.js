matematici.WorkFlowSignatureBook = function (options) {
    var
        workFlowSignatureBook = this,

        show = function (caption, data, handler) {
            $.ajax({
                url: matematici.urls.api + "/WorkFlows/Applications/GetSteps",
                type: "POST",
                data: data
            }).done(function (result) {
                var columns = [
                    { name: "step", label: "Step", align: "center" },
                    { name: "tags", label: "Level" },
                    { name: "reason", label: "Reason" },
                    { name: "date", label: "Date", type: "DateTimeOffset" },
                    { name: "user", label: "User" },
                    { name: "approved", label: "Approved", align: "center" },
                ];
                new matematici.CheckListGrid(caption, result.steps,
                    function (grid, row) {
                        return handler(result.action, grid, result.steps, row);
                    },
                    null,
                    columns);
            });
        },

        initialize = function () {
        };

    workFlowSignatureBook.showForPrototype = function (code, id, version, handler) {
        show("Prototype " + code, {
            prototype: id,
            version: version,
        }, handler)
    };

    workFlowSignatureBook.showForInstance = function (code, id, handler) {
        show("Instance " + code, {
            instance: id
        }, handler);
    };

    workFlowSignatureBook.showForReport = function (code, id, handler) {
        show("Report " + code, {
            report: id
        }, handler);
    };

    initialize();
}