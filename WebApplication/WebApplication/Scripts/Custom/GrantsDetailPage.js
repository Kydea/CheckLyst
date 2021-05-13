matematici.GrantsDetailPage = function (formSelector, page, loadHandler) {
    var
        departments,
        loading = false,
        prototypeFunctions = ["Approve", "Edit", "Preview", "View"],
        instanceFunctions = ["Confirm", "Create", "Delete", "Edit", "Unsign", "View"],
        roles,
        groups,
        formElement = $(formSelector),
        tab,
        detailPage,
        generalRoleSelect,
        groupsSelect,

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

            $("input[type=checkbox]", tab.getItem(1).tab).each(function () {
                var
                    check = $(this),
                    functionGroup = role && role.functionGroups && role.functionGroups[check.data("group")],
                    value = functionGroup && functionGroup.indexOf(check.data("function")) >= 0;

                check.prop("checked", value);
                matematici.controls.setEditable(check, role == null);
            });
        },

        addFunction = function (tabIndex) {
            var
                tabItem = tabIndex && tab.getItem(tabIndex) || tab.getCurrentItem(),
                div = $("<div />").addClass("Function").insertBefore(tabItem.tab.children("div.Clearing")),
                departmentsSelect, prototypeTypesSelect, rolesSelect, functionsSelect,
                departmentsAutoComplete, prototypeTypesAutoComplete, rolesAutoComplete, functionsAutoComplete;

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

            setTimeout(function () {
                if (page != "Roles") {
                    departmentsAutoComplete.refresh();
                    prototypeTypesAutoComplete.refresh();
                    rolesAutoComplete.refresh();
                }
                functionsAutoComplete.refresh();
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
                "function": functionsSelect
            }
        },

        addLevel = function () {
            var
                tabItem = tab.getItem(4),
                div = $("<div />").addClass("Function").insertBefore(tabItem.tab.children("div.Clearing")),
                departmentsSelect, prototypeTypesSelect, prototypeWorkFlowLevelSelect, instanceWorkFlowLevelSelect, signatureLevelSelect,
                departmentsAutoComplete, prototypeTypesAutoComplete, prototypeWorkFlowLevelAutoComplete, instanceWorkFlowLevelAutoComplete, signatureLevelAutoComplete;

            $("<hr />").appendTo(div)

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

            prototypeWorkFlowLevelSelect = $("<select />").addClass("PrototypeWorkFlowLevel").attr("data-nullable", "");
            if (detailPage.isReadOnly()) matematici.controls.setEditable(prototypeWorkFlowLevelSelect, false);
            $("<label />").appendTo(div).append($("<span />").text("Prototype workflow level")).append($("<i />").addClass("Icon fal fa-users")).append(prototypeWorkFlowLevelSelect);
            for (var index in matematici.profiles)
                prototypeWorkFlowLevelSelect.append($("<option />").text(matematici.profiles[index]).attr("value", matematici.profiles[index]));
            prototypeWorkFlowLevelSelect.val(null);
            prototypeWorkFlowLevelAutoComplete = new matematici.AutoComplete(prototypeWorkFlowLevelSelect[0]);
            prototypeWorkFlowLevelAutoComplete.initialize();

            instanceWorkFlowLevelSelect = $("<select />").addClass("InstanceWorkFlowLevel").attr("data-nullable", "");
            if (detailPage.isReadOnly()) matematici.controls.setEditable(instanceWorkFlowLevelSelect, false);
            $("<label />").appendTo(div).append($("<span />").text("Instance workflow level")).append($("<i />").addClass("Icon fal fa-users")).append(instanceWorkFlowLevelSelect);
            for (var index in matematici.profiles)
                instanceWorkFlowLevelSelect.append($("<option />").text(matematici.profiles[index]).attr("value", matematici.profiles[index]));
            instanceWorkFlowLevelSelect.val(null);
            instanceWorkFlowLevelAutoComplete = new matematici.AutoComplete(instanceWorkFlowLevelSelect[0]);
            instanceWorkFlowLevelAutoComplete.initialize();

            signatureLevelSelect = $("<select />").addClass("SignatureLevel").attr("data-nullable", "");
            if (detailPage.isReadOnly()) matematici.controls.setEditable(signatureLevelSelect, false);
            $("<label />").appendTo(div).append($("<span />").text("Signature level")).append($("<i />").addClass("Icon fal fa-users")).append(signatureLevelSelect);
            for (var index in matematici.profiles)
                signatureLevelSelect.append($("<option />").text(matematici.profiles[index]).attr("value", matematici.profiles[index]));
            signatureLevelSelect.val(null);
            signatureLevelAutoComplete = new matematici.AutoComplete(signatureLevelSelect[0]);
            signatureLevelAutoComplete.initialize();

            setTimeout(function () {
                departmentsAutoComplete.refresh();
                prototypeWorkFlowLevelAutoComplete.refresh();
                instanceWorkFlowLevelAutoComplete.refresh();
                signatureLevelAutoComplete.refresh();
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
                prototypeWorkFlowLevelSelect: prototypeWorkFlowLevelSelect,
                instanceWorkFlowLevelSelect: instanceWorkFlowLevelSelect,
                signatureLevel: signatureLevelSelect
            }
        },

        parametersPrepareHandler = function () {
            var data = {
                generalFunctions: [],
                prototypeFunctions: [],
                instanceFunctions: [],
                levels: []
            };

            var generalRole = page == "Roles" ? null : generalRoleSelect.val();
            if (generalRole == null)
                $("input[type=checkbox]:checked", tab.getItem(1).tab).each(function () {
                    var
                        record = {},
                        check = $(this);

                    record.functionGroup = check.data("group");
                    record.function = check.data("function");
                    data.generalFunctions.push(record);
                });
            else
                data.generalFunctions.push({ role: generalRole });

            if (page == "Users")
                data.groups = groupsSelect.val();

            $("div.Function", tab.getItem(2).tab).each(function () {
                var record = {};
                if (page == "Roles")
                    record.functions = $("select.Function", this).val();
                else {
                    record.department = $("select.Department", this).val();
                    record.prototypeType = $("select.PrototypeType", this).val();
                    record.role = $("select.Role", this).val();
                    if (record.role == null) record.functions = $("select.Function", this).val();
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
                }
                data.instanceFunctions.push(record);
            });

            if (page == "Users")
                $("div.Function", tab.getItem(4).tab).each(function () {
                    var record = {};
                    record.department = $("select.Department", this).val();
                    record.prototypeType = $("select.PrototypeType", this).val();
                    record.prototypeWorkFlowLevel = $("select.PrototypeWorkFlowLevel", this).val();
                    record.instanceWorkFlowLevel = $("select.InstanceWorkFlowLevel", this).val();
                    record.signatureLevel = $("select.SignatureLevel", this).val();
                    data.levels.push(record);
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
            groups = detailPage.getSender().getElement().data("groups");

            detailPage.getForm().trigger("Load", function (result) {
                loading = true;
                detailPage.getForm().handleResult(result);

                if (page != "Roles") {
                    generalRoleSelect = $("select.GeneralRole", detailPage.getElement());
                    if (detailPage.isReadOnly()) matematici.controls.setEditable(generalRoleSelect, false);
                    generalRoleSelect.on("change", refreshGeneralFunctions);
                    for (var index in roles.general)
                        generalRoleSelect.append($("<option />").text(index).attr("value", index));
                    var generalAutoComplete = matematici.AutoComplete.get(generalRoleSelect[0]);
                    generalAutoComplete.checkReadOnly();
                    generalAutoComplete.refreshValues();
                    generalAutoComplete.setValue(null);
                }

                if (page == "Users") {
                    groupsSelect = $("<select />").appendTo($("div.Tab.Groups div.Control", detailPage.getElement())).prop("multiple", true).addClass("FullHeight");
                    for (var index in groups)
                        groupsSelect.append($("<option />").text(groups[index]).attr("value", groups[index]));
                    groupsSelect.val(result.groups);
                    var autoComplete = new matematici.AutoComplete(groupsSelect[0]);
                    autoComplete.initialize();
                    if (detailPage.isReadOnly()) autoComplete.lock(true)
                }

                $("input[name=authenticationCode]", detailPage.getElement()).parent().toggleClass("Hidden", !result.showAuthenticationCode);
                $("input[name=password]", detailPage.getElement()).parent().toggleClass("Hidden", !result.showPassword);
                $("input[name=password2]", detailPage.getElement()).parent().toggleClass("Hidden", !result.showPassword);
                if (result.generalFunctions) buildGeneralFunctions(result.generalFunctions);
                if (result.prototypeFunctions) buildFunctions(result.prototypeFunctions, 2);
                if (result.instanceFunctions) buildFunctions(result.instanceFunctions, 3);
                if (page == "Users" && result.levels) buildLevels(result.levels, 4);

                fixQueryParameters(2);
                fixQueryParameters(3);
                fixQueryParameters(4);
                loading = false;
                loadHandler && loadHandler();
            });
        },

        buildGeneralFunctions = function (functions) {
            if (functions.length == 1 && functions[0].role != null) {
                generalRoleSelect.val(functions[0].role);
                refreshGeneralFunctions();
            } else {
                var functionsObject = {};
                for (var index in functions) {
                    var record = functions[index];
                    (functionsObject[record.functionGroup] = functionsObject[record.functionGroup] || {})[record["function"]] = true;
                }
                $("input[type=checkbox]", tab.getItem(1).tab).each(function () {
                    var check = $(this);
                    check.prop("checked", !!(functionsObject[check.data("group")] && functionsObject[check.data("group")][check.data("function")]));
                });
            }
        },

        buildFunctions = function (functions, tabIndex) {
            for (var index in functions) {
                var
                    record = functions[index],
                    row = addFunction(tabIndex);

                if (page != "Roles") {
                    row.department.val(record.department);
                    refreshPrototypeTypes(row.department, row.prototypeType);
                    row.prototypeType.val(record.prototypeType);
                    row.role.val(record.role);
                    refreshFunctions(row.role, row.function, tabIndex);
                }

                if (record.role == null) row["function"].val(record.functions);
            }
        },

        buildLevels = function (levels) {
            for (var index in levels) {
                var
                    record = levels[index],
                    row = addLevel();

                row.department.val(record.department);
                refreshPrototypeTypes(row.department, row.prototypeType);
                row.prototypeType.val(record.prototypeType);
                row.prototypeWorkFlowLevelSelect.val(record.prototypeWorkFlowLevel);
                row.instanceWorkFlowLevelSelect.val(record.instanceWorkFlowLevel);
                row.signatureLevel.val(record.signatureLevel);
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
                    text: "add",
                    hidden: true,
                    id: "Add",
                    handler: function () {
                        var tabIndex = tab.getCurrentItem().index;
                        if (tabIndex == 4)
                            addLevel();
                        else
                            addFunction(tabIndex);
                        fixQueryParameters(tabIndex);
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
                    $("a#Add", matematici.popup.getElement()).toggle(item.index > 1 && item.index < 5 && page != "Roles");
                });
        };

    setup();
}