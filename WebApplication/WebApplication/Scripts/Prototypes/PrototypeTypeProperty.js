prototypes.PrototypeTypeProperty = function (model, options) {
    var
        base,
        valuePanel,
        department,
        prototypeType,
        prototypeTypes,
        departments,
        suspended = false,

        loadPrototypeTypes = function () {
            $.ajax({
                type: "post",
                url: matematici.urls.api + "/Infrastructure/PrototypeTypes/GetSyntheticData",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    data: {
                        functionGroup: "Prototypes",
                        function: "Edit"
                    }
                })
            }).done(function (result) {
                prototypeTypes = result;
                setup();
            });
        },

        preparePrototypeTypes = function () {
            departments = [];

            var
                departmentID = null,
                department;

            for (var index in prototypeTypes) {
                var record = prototypeTypes[index];
                if (departmentID != record.department) {
                    departments.push(department = { code: record.department, name: record.departmentName, prototypeTypes: [] });
                    departmentID = record.department;
                }

                department.prototypeTypes.push({
                    id: record.id,
                    name: record.name,
                    lifeCycle: record.lifeCycle
                });
            }
        },

        refreshPrototypeTypes = function (initialValue) {
            prototypeType.empty();
            var departmentIndex = department.prop('selectedIndex');
            suspended = true;
            if (departmentIndex >= 0) {
                var prototypeTypes = departments[departmentIndex].prototypeTypes;
                for (var index in prototypeTypes)
                    prototypeType.append($("<option />").text(prototypeTypes[index].name).attr("value", prototypeTypes[index].id));
                prototypeType.val(initialValue).change();
            } else
                prototypeType.val(null).change();
            suspended = false;

            setTimeout(function () {
                matematici.AutoComplete.get(prototypeType[0]).refresh();
            }, 0);
        },

        setup = function () {
            preparePrototypeTypes();

            var label = $("<label />").appendTo(valuePanel);
            $("<i />").addClass("Icon fal fa-industry").appendTo(label);
            department =
                $("<select />")
                    .attr("data-nullable", "")
                    .on("change", function () {
                        refreshPrototypeTypes(null);
                        if (!!model.department && model.errors) {
                            base.setError(model.errors, "department", null);
                            base.setError(model.errors, "prototype-type");
                        }
                    })
                    .appendTo(label);

            for (var index in departments)
                department.append($("<option />").text(departments[index].name).attr("value", departments[index].code));

            label = $("<label />").appendTo(valuePanel);
            $("<i />").addClass("Icon fal fa-folder").appendTo(label);
            prototypeType =
                $("<select />")
                    .attr("data-nullable", "")
                    .on("change", function () {
                        model.department = department.val();
                        model.departmentName = department.next().val();
                        model.prototypeType = prototypeType.val();
                        model.prototypeTypeName = prototypeType.next().val();

                        if (!options.readOnly) {
                            base.updateModel(model);
                            if (!suspended && (!model.errors || !model.errors.length)) {
                                departmentLoop:
                                for (var departmentIndex in departments)
                                    for (var prototypeTypeIndex in departments[departmentIndex].prototypeTypes) {
                                        var prototypeTypeRecord = departments[departmentIndex].prototypeTypes[prototypeTypeIndex];
                                        if (prototypeTypeRecord.id == model.prototypeType) {
                                            model.lifeCycle = prototypeTypeRecord.lifeCycle;
                                            break departmentLoop;
                                        }
                                    }
                                options.lifeCycleProperty.setValue();
                            }
                        }

                        if (!!model.prototypeType && model.errors)
                            base.setError(model.errors, ["department", "prototype-type"], null);
                    })
                    .appendTo(label);

            department.val(model.department);
            if (!!model.department) refreshPrototypeTypes(model.prototypeType);

            if (options.readOnly) {
                matematici.controls.setEditable(department, false);
                matematici.controls.setEditable(prototypeType, false);
            } 

            new matematici.AutoComplete(department[0]).initialize();
            new matematici.AutoComplete(prototypeType[0]).initialize();
        };


    this.create = function (vp, baseClass) {
        base = baseClass;
        valuePanel = vp;
        valuePanel.addClass("PrototypeType");
        loadPrototypeTypes();
    }

    this.hasChanges = function () {
        return !!(model.department || model.prototypeType);
    }

    this.setError = function (handler) {
        handler(model.errors, ["department", "prototype-type"]);
    };
};