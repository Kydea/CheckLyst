reports.GroupsList = function () {
    var
        groupsList = this,
        model = reports.reportEditor.getModel(),
        groupsSelect,
        groupsAutoComplete,
        modified = false,
        form,
        restrictedInput,

        toggleGroups = function () {
            var locked = !restrictedInput.prop("checked");
            groupsAutoComplete.lock(locked || !reports.reportEditor.isEditable());
            if (locked) groupsAutoComplete.setValue(null);
        },

        confirm = function () {
            if (modified) {
                matematici.canClose = false;
                if (model.fields == null) model.fields = {};
                model.fields.restricted = { value: restrictedInput.prop("checked") ? "true" : "false" };
                model.groups = groupsSelect.val();
            }
        },

        fill = function (f) {
            form = f;
            restrictedInput = $("input[name=restricted]").prop("checked", model.fields && model.fields.restricted && model.fields.restricted.value == "true");

            matematici.controls.setEditable(restrictedInput, reports.reportEditor.isEditable());
            form
                .on("toggle", "select", function () {
                    modified = true;
                })
                .on("change", "input", function () {
                    toggleGroups();
                    modified = true;
                });
            $.ajax({
                type: "post",
                url: matematici.urls.api + "/AccessControl/Groups/GetSyntheticData"
            }).done(function (result) {
                groupsSelect = $("<select />").appendTo($("div.Control", form)).prop("multiple", true).addClass("FullHeight");
                for (var index in result)
                    groupsSelect.append($("<option />").text(result[index]).attr("value", result[index]));
                groupsSelect.val(model.groups);
                groupsAutoComplete = new matematici.AutoComplete(groupsSelect[0]);
                groupsAutoComplete.initialize();

                toggleGroups();
            });
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true });
            if (reports.reportEditor.isEditable())
                buttons.push({ text: "confirm", icon: "save", handler: confirm });

            matematici.popup.show({
                url: matematici.urls.reports.groupsList,
                data: groupsList,
                sender: groupsList,
                buttons: buttons,
                width: 770,
            });
        };

    groupsList.fill = fill;
    setup();
};