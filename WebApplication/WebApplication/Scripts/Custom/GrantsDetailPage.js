matematici.GrantsDetailPage = function (formSelector, page, loadHandler) {
    var
        departments,
        loading = false,
        prototypeFunctions = ["Approve", "Edit", "Preview", "View"],
        instanceFunctions = ["Confirm", "Create", "Delete", "Edit", "Unsign", "View"],
        roles,
        formElement = $(formSelector),
        tab,
        detailPage,
        generalRoleSelect,


        fixQueryParameters = function (tabIndex) {
            steps = $("div.Function", tab.getItem(tabIndex).tab).each(function (index) {
                $(this).children("hr").toggle(index > 0);
            });

            resize();
        },

        resize = function () {
            tab.resize();
            setTimeout(function () {
                matematici.popup.center();
            }, 500);
        },

        refreshPrototypeTypes = function (departmentSelect, prototypeTypeSelect) {
            prototypeTypeSelect.empty();
            var prototypeTypes = departments[departmentSelect.prop('selectedIndex')].prototypeTypes;
            for (var index in prototypeTypes)
                prototypeTypeSelect.append($("<option />").text(prototypeTypes[index].name).attr("value", prototypeTypes[index].id));
            prototypeTypeSelect.val("");
            setTimeout(function () {
                matematici.AutoComplete.get(prototypeTypeSelect[0]).refresh();
            }, 0);
        },

        refreshFunctions = function (rolesSelect, functionsSelect, tabIndex) {
            if (tabIndex == undefined)
                tabIndex = tab.getCurrentItem().index;

            var
                roleName = rolesSelect.val(),
                role = null;

            if (roleName != null)
                role = roles[tabIndex == 1 ? "general" : tabIndex == 2 ? "prototypes" : "instances"][roleName];

            functionsSelect.val(role && role.functions);
            setTimeout(function () {
                var autoComplete = matematici.AutoComplete.get(functionsSelect[0]);
                autoComplete.lock(roleName != null);
                autoComplete.refresh();
            }, 0);
        },

        refreshGeneralFunctions = function () {
            var
                roleName = generalRoleSelect.val(),
                role = null;

            if (roleName != null)
                role = roles.general[roleName];

            $("input[type=checkbox]:checked", tab.getItem(1).tab).each(function () {
                var
                    record = {},
                    check = $(this);

                record.functionGroup = check.data("group");
                record.function = check.data("function");
                //check.prop("checked", role && role.indexOf())
            });
        },

        addFunction = function (tabIndex) {
            var
                tabItem = tabIndex && tab.getItem(tabIndex) || tab.getCurrentItem(),
                div = $("<div />").addClass("Function").insertBefore(tabItem.tab.children("div.Clearing")),
                departmentsSelect, prototypeTypesSelect, rolesSelect, functionsSelect, workFlowLevelSelect, signatureLevelSelect,
                departmentsAutoComplete, prototypeTypesAutoComplete, rolesAutoComplete, functionsAutoComplete, workFlowLevelAutoComplete, signatureLevelAutoComplete;

            $("<hr />").appendTo(div)

            if (page != "Roles") {
                departmentsSelect = $("<select />").addClass("Department");
                if (detailPage.isReadOnly()) matematici.controls.setEditable(departmentsSelect, false);
                $("<label />").appendTo(div).append($("<span />").text("Department")).append($("<i />").addClass("Icon fal fa-industry")).append(departmentsSelect).addClass("Clearing");
                for (var index in departments)
                    departmentsSelect.append($("<option />").text(departments[index].name).attr("value", departments[index].code));
                departmentsAutoComplete = new matematici.AutoComplete(departmentsSelect[0]);
                departmentsAutoComplete.initialize();
                departmentsSelect.on("change", function () {
                    refreshPrototypeTypes(departmentsSelect, prototypeTypesSelect);
                });

                prototypeTypesSelect = $("<select />").addClass("PrototypeType").attr("data-nullable", "(all)");
                if (detailPage.isReadOnly()) matematici.controls.setEditable(prototypeTypesSelect, false);
                $("<label />").appendTo(div).append($("<span />").text("Prototype type")).append($("<i />").addClass("Icon fal fa-folder")).append(prototypeTypesSelect);
                prototypeTypesAutoComplete = new matematici.AutoComplete(prototypeTypesSelect[0]);
                prototypeTypesAutoComplete.initialize();
                refreshPrototypeTypes(departmentsSelect, prototypeTypesSelect);
            }

            if (page != "Roles") {
                rolesSelect = $("<select />").addClass("Role").attr("data-nullable", "(custom)");
                if (detailPage.isReadOnly()) matematici.controls.setEditable(rolesSelect, false);
                $("<label />").appendTo(div).append($("<span />").text("Role")).append($("<i />").addClass("Icon fal fa-cogs")).append(rolesSelect).addClass("Width150");
                var list = tabItem.index == 2 ? roles.prototypes : roles.instances;
                for (var index in list)
                    rolesSelect.append($("<option />").text(index).attr("value", index));
                rolesAutoComplete = new matematici.AutoComplete(rolesSelect[0]);
                rolesAutoComplete.initialize();
                rolesAutoComplete.setValue(null);
                rolesSelect.on("change", function () {
                    refreshFunctions(rolesSelect, functionsSelect, tabIndex);
                });
            }

            functionsSelect = $("<select />").addClass("Function").prop("multiple", true);
            if (detailPage.isReadOnly()) matematici.controls.setEditable(functionsSelect, false);
            $("<label />").appendTo(div).append($("<span />").text("Function")).append($("<i />").addClass("Icon fal fa-cogs")).append(functionsSelect).addClass("Width150");
            var list = tabItem.index == 2 ? prototypeFunctions : instanceFunctions;
            for (var index in list)
                functionsSelect.append($("<option />").text(list[index]).attr("value", list[index]));
            functionsAutoComplete = new matematici.AutoComplete(functionsSelect[0]);
            functionsAutoComplete.initialize();

            if (page == "Users") {
                workFlowLevelSelect = $("<select />").addClass("WorkFlowLevel").attr("data-nullable", "");
                if (detailPage.isReadOnly()) matematici.controls.setEditable(workFlowLevelSelect, false);
                $("<label />").appendTo(div).append($("<span />").text("Workflow level")).append($("<i />").addClass("Icon fal fa-users")).append(workFlowLevelSelect);
                for (var index in matematici.profiles)
                    workFlowLevelSelect.append($("<option />").text(matematici.profiles[index]).attr("value", matematici.profiles[index]));
                workFlowLevelSelect.val(null);
                workFlowLevelAutoComplete = new matematici.AutoComplete(workFlowLevelSelect[0]);
                workFlowLevelAutoComplete.initialize();
                if (tabItem.index == 3) {
                    signatureLevelSelect = $("<select />").addClass("SignatureLevel").attr("data-nullable", "");
                    if (detailPage.isReadOnly()) matematici.controls.setEditable(signatureLevelSelect, false);
                    $("<label />").appendTo(div).append($("<span />").text("Signature level")).append($("<i />").addClass("Icon fal fa-users")).append(signatureLevelSelect);
                    for (var index in matematici.profiles)
                        signatureLevelSelect.append($("<option />").text(matematici.profiles[index]).attr("value", matematici.profiles[index]));
                    signatureLevelSelect.val(null);
                    signatureLevelAutoComplete = new matematici.AutoComplete(signatureLevelSelect[0]);
                    signatureLevelAutoComplete.initialize();
                }
            }

            setTimeout(function () {
                if (page != "Roles") {
                    departmentsAutoComplete.refresh();
                    rolesAutoComplete.refresh();
                }
                functionsAutoComplete.refresh();
                if (page == "Users") {
                    workFlowLevelAutoComplete.refresh();
                    if (tabItem.index == 3)
                        signatureLevelAutoComplete.refresh();
                }
            }, 0);

            if (!detailPage.isReadOnly() && page != "Roles")
                $("<a />").attr("href", "#").addClass("Button Icon").append($("<i />").addClass("Icon fa fa-minus")).on("click", function () {
                    div.remove();
                    fixQueryParameters(tabItem.index);
                }).appendTo(div);

            if (!loading)
                fixQueryParameters(tabItem.index);

            return {
                department: departmentsSelect,
                prototypeType: prototypeTypesSelect,
                role: rolesSelect,
                "function": functionsSelect,
                workFlowLevel: workFlowLevelSelect,
                signatureLevel: signatureLevelSelect
            }
        },

        parametersPrepareHandler = function () {
            var data = {
                generalFunctions: [],
                prototypeFunctions: [],
                instanceFunctions: []
            };

            $("input[type=checkbox]:checked", tab.getItem(1).tab).each(function () {
                var
                    record = {},
                    check = $(this);

                record.functionGroup = check.data("group");
                record.function = check.data("function");
                data.generalFunctions.push(record);
            });

            $("div.Function", tab.getItem(2).tab).each(function () {
                var record = {};
                if (page == "Roles") 
                    record.functions = $("select.Function", this).val();
                else {
                    record.department = $("select.Department", this).val();
                    record.prototypeType = $("select.PrototypeType", this).val();
                    record.role = $("select.Role", this).val();
                    if (record.role == null) record.functions = $("select.Function", this).val();

                    if (page == "Users") 
                        record.workFlowLevel = $("select.WorkFlowLevel", this).val();
                }
                data.prototypeFunctions.push(record);
            });

            $("div.Function", tab.getItem(3).tab).each(function () {
                var record = {}
                if (page == "Roles")
                    record.functions = $("select.Function", this).val();
                else {
                    record.department = $("select.Department", this).val();
                    record.prototypeType = $("select.PrototypeType", this).val();
                    record.role = $("select.Role", this).val();
                    if (record.role == null) record.functions = $("select.Function", this).val();

                    if (page == "Users") {
                        record.workFlowLevel = $("select.WorkFlowLevel", this).val();
                        record.signatureLevel = $("select.SignatureLevel", this).val();
                    }
                }
                data.instanceFunctions.push(record);
            });

            return { data: data };
        },

        preparePrototypeTypes = function (data) {
            departments = [];

            var
                departmentID = null,
                department;

            for (var index in data) {
                var record = data[index];
                if (departmentID != record.department) {
                    departments.push(department = { code: record.department, name: record.departmentName, prototypeTypes: [] });
                    departmentID = record.department;
                }

                department.prototypeTypes.push({
                    id: record.id,
                    name: record.name
                });
            }
        },

        applyConstraintToPrototypeGrant = function (autoComplete, value, on) {
            var values = autoComplete.getValue();
            if (on) {
                if (value == "Approve") {
                    Array.addElement(values, "Preview");
                    Array.addElement(values, "View");
                } else if (value == "Edit") {
                    Array.addElement(values, "Preview");
                    Array.addElement(values, "View");
                } else if (value == "Preview") {
                    Array.addElement(values, "View");
                } else
                    values = null;
            } else {
                if (value == "View")
                    values = [];
                else if (value == "Preview") {
                    Array.removeElement(values, "Approve");
                    Array.removeElement(values, "Edit");
                } else
                    values = null;
            }

            if (values)
                autoComplete.setValue(values);
        },

        applyConstraintToInstanceGrant = function (autoComplete, value, on) {
            var values = autoComplete.getValue();
            if (on) {
                if (value == "Edit" || value == "Confirm" || value == "Delete")
                    Array.addElement(values, "View");
                else if (value == "Create" || value == "Unsign") {
                    Array.addElement(values, "View");
                    Array.addElement(values, "Edit");
                } else
                    values = null;
            } else {
                if (value == "View")
                    values = [];
                else if (value == "Edit") {
                    Array.removeElement(values, "Create");
                    Array.removeElement(values, "Unsign");
                } else
                    values = null
            }

            if (values)
                autoComplete.setValue(values);
        },

        initHandler = function () {
            matematici.controls.setEditable($("input[name=code]", detailPage.getElement()), detailPage.isNew());

            tab.getItem(1).tab
                .on("click", "input[data-function=Edit]", function () {
                    var check = $(this);
                    setTimeout(function () {
                        if (check.is(":checked"))
                            check.closest(".GeneralFunction").find("input[data-function=View]").prop("checked", true);
                    }, 0);
                })
                .on("click", "input[data-function=View]", function () {
                    var check = $(this);
                    setTimeout(function () {
                        if (!check.is(":checked"))
                            check.closest(".GeneralFunction").find("input[data-function=Edit]").prop("checked", false);
                    }, 0);
                });

            tab.getItem(2).tab.on("toggle", "select.AutoComplete", function (e, value, on) {
                applyConstraintToPrototypeGrant(matematici.AutoComplete.get(this), value, on);
            });

            tab.getItem(3).tab.on("toggle", "select.AutoComplete", function (e, value, on) {
                applyConstraintToInstanceGrant(matematici.AutoComplete.get(this), value, on);
            });
        },

        completeHandler = function () {
            if (page != "Roles") preparePrototypeTypes(detailPage.getSender().getElement().data("prototypeTypes"));
            roles = detailPage.getSender().getElement().data("roles");

            detailPage.getForm().trigger("Load", function (result) {
                loading = true;
                detailPage.getForm().handleResult(result);

                $("input[name=authenticationCode]", detailPage.getElement()).parent().toggleClass("Hidden", !result.showAuthenticationCode);
                $("input[name=password]", detailPage.getElement()).parent().toggleClass("Hidden", !result.showPassword);
                $("input[name=password2]", detailPage.getElement()).parent().toggleClass("Hidden", !result.showPassword);
                if (result.generalFunctions) buildGeneralFunctions(result.generalFunctions);
                if (result.prototypeFunctions) buildFunctions(result.prototypeFunctions, 2);
                if (result.instanceFunctions) buildFunctions(result.instanceFunctions, 3);

                generalRoleSelect = $("select.GeneralRole", detailPage.getElement());
                generalRoleSelect.on("change", refreshGeneralFunctions);
                for (var index in roles.general) 
                    generalRoleSelect.append($("<option />").text(index).attr("value", index));
                var generalAutoComplete = matematici.AutoComplete.get(generalRoleSelect[0]);
                generalAutoComplete.refreshValues();
                generalAutoComplete.setValue(null);

                fixQueryParameters(2);
                fixQueryParameters(3);
                loading = false;
                loadHandler && loadHandler();
            });
        },

        buildGeneralFunctions = function (functions) {
            var functionsObject = {};
            for (var index in functions) {
                var record = functions[index];
                (functionsObject[record.functionGroup] = functionsObject[record.functionGroup] || {})[record["function"]] = true;
            }
            $("input[type=checkbox]", tab.getItem(1).tab).each(function () {
                var check = $(this);
                check.prop("checked", !!(functionsObject[check.data("group")] && functionsObject[check.data("group")][check.data("function")]));
            });
        },

        buildFunctions = function (functions, tabIndex) {
            for (var index in functions) {
                var
                    record = functions[index],
                    row = addFunction(tabIndex);

                if (page != "Roles") {
                    row.department.val(record.department);
                    refreshPrototypeTypes(row.department, row.prototypeType);
                    row.role.val(record.role);
                    refreshFunctions(row.role, row.function, tabIndex);
                    row.prototypeType.val(record.prototypeType);
                }

                if (page == "Users") {
                    row.workFlowLevel.val(record.workFlowLevel);
                    if (tabIndex == 3)
                        row.signatureLevel.val(record.signatureLevel);
                }

                if (record.role == null) row["function"].val(record.functions);
            }
        },

        setup = function () {
            tab = new matematici.Tab(formSelector + " div.TabContainer", {
                fixedHeight: true
            });

            detailPage = new matematici.DetailPage({
                formSelector: formSelector,
                initHandler: initHandler,
                parametersPrepareHandler: parametersPrepareHandler,
                completeHandler: completeHandler,
                width: "inherit",
                buttons: [{
                    icon: "plus",
                    text: "add function",
                    hidden: true,
                    id: "AddFunction",
                    handler: function () {
                        addFunction();
                        fixQueryParameters(tab.getCurrentItem().index);
                        return true;
                    }
                }]
            });

            formElement.on("change", "input[type=checkbox], select", function () {
                detailPage.getForm().onChange($(this));
            });

            if (detailPage.isReadOnly()) {
                $("div.Buttons a.Button", detailPage.getElement()).hide();
                $("input[name], select[name]", detailPage.getElement()).each(function () {
                    var field = $(this);
                    matematici.controls.setEditable(field, false);
                });
                $("input[type=checkbox]", detailPage.getElement()).prop("disabled", true);
            } else
                tab.addHandler(function (item) {
                    $("a#AddFunction", matematici.popup.getElement()).toggle(item.index > 1 && page != "Roles");
                });
        };

    setup();
}