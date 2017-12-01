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
                        .append($("<option />").attr("selected", !model.company))
                        .on("change", function () { loadDepartments(valuePanel); })
                        .appendTo(valuePanel);
                    for (var index in result)
                        company.append($("<option />").text(result[index]).attr({ value: result[index], selected: result[index] == model.company }));

                    if (model.company)
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
                            department.append($("<option />").text(result[index]).attr({ value: result[index], selected: result[index] == model.department }));

                        if (model.department)
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
                                model.company = company.val();
                                model.department = department.val();
                                model.prototypeType = prototypeType.val();
                                matematici.canClose = false;
                            })
                            .appendTo(valuePanel);
                        for (var index in result)
                            prototypeType.append($("<option />").text(result[index]).attr({ value: result[index], selected: result[index] == model.prototypeType }));
                    }
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    console.log("Server error", jqXHR, textStatus, errorThrown);
                    alert("Server error: " + errorThrown);
                });
        };

    this.create = function (valuePanel) {
        valuePanel.addClass("PrototypeType");
        if (options.readOnly) {
            $("<input />").attr({ type: "text", readOnly: true }).val(model.company).appendTo(valuePanel);
            $("<input />").attr({ type: "text", readOnly: true }).val(model.department).appendTo(valuePanel);
            $("<input />").attr({ type: "text", readOnly: true }).val(model.prototypeType).appendTo(valuePanel);
        } else
            loadCompanies(valuePanel)
    }

    this.hasChanges = function () {
        return !!(model.company || model.department || model.prototypeType);
    }
}

