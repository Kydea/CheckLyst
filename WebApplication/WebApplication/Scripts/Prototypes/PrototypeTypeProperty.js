prototypes.PrototypeTypeProperty = function (model, options) {
    var
        input,
        handler,
        company,
        department,
        prototypeType,

        loadCompanies = function (valuePanel) {
            $.ajax({
                url: matematici.urls.prototypes.companies,
                type: "post",
                data: {}
            }).done(function (result) {
                if (result.Error)
                    matematici.dialog.error("Error", result.Error);
                else {
                    var option =
                    company =
                        $("<select />")
                        .append($("<option />").attr("selected", !model.Company))
                        .on("change", function () { loadDepartments(valuePanel); })
                        .appendTo(valuePanel);
                    for (var index in result)
                        company.append($("<option />").text(result[index]).attr({ value: result[index], selected: result[index] == model.Company }));

                    if (model.Company)
                        loadDepartments(valuePanel);
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.log("Server error", jqXHR, textStatus, errorThrown);
                alert("Server error: " + errorThrown);
            });
        },

        loadDepartments = function (valuePanel) {
            if (department) department.remove();

            if (company.val())
                $.ajax({
                    url: matematici.urls.prototypes.departments,
                    type: "post",
                    data: { Company: company.val() }
                }).done(function (result) {
                    if (result.Error)
                        matematici.dialog.error("Error", result.Error);
                    else {
                        department =
                            $("<select />")
                            .append($("<option />").attr("selected", true))
                            .on("change", function () { loadPrototypeTypes(valuePanel); })
                            .appendTo(valuePanel);
                        for (var index in result)
                            department.append($("<option />").text(result[index]).attr({ value: result[index], selected: result[index] == model.Department }));

                        if (model.Department)
                            loadPrototypeTypes(valuePanel);
                    }
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    console.log("Server error", jqXHR, textStatus, errorThrown);
                    alert("Server error: " + errorThrown);
                });
        },

        loadPrototypeTypes = function (valuePanel) {
            if (prototypeType) prototypeType.remove();

            if (department.val())
                $.ajax({
                    url: matematici.urls.prototypes.prototypeTypes,
                    type: "post",
                    data: { Company: company.val(), Department: department.val() }
                }).done(function (result) {
                    if (result.Error)
                        matematici.dialog.error("Error", result.Error);
                    else {
                        prototypeType =
                            $("<select />")
                            .append($("<option />").attr("selected", true))
                            .on("change", function () {
                                model.Company = company.val();
                                model.Department = department.val();
                                model.PrototypeType = prototypeType.val();
                                matematici.canClose = false;
                            })
                            .appendTo(valuePanel);
                        for (var index in result)
                            prototypeType.append($("<option />").text(result[index]).attr({ value: result[index], selected: result[index] == model.PrototypeType }));
                    }
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    console.log("Server error", jqXHR, textStatus, errorThrown);
                    alert("Server error: " + errorThrown);
                });
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("PrototypeType");
        if (options.readOnly) {
            $("<input />").attr({ type: "text", readOnly: true }).val(model.Company).appendTo(valuePanel);
            $("<input />").attr({ type: "text", readOnly: true }).val(model.Department).appendTo(valuePanel);
            $("<input />").attr({ type: "text", readOnly: true }).val(model.PrototypeType).appendTo(valuePanel);
        } else
            loadCompanies(valuePanel)
    }

    this.hasChanges = function () {
        return !!(model.Company || model.Department || model.PrototypeType);
    }
}

