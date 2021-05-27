reports.GroupsList = function () {
    var
        groupsList = this,
        model = reports.reportEditor.getModel(),

        fill = function (form) {
            console.log(form);

            groupsSelect = $("<select />").appendTo($("div.Control", form)).prop("multiple", true).addClass("FullHeight");
            /*
            for (var index in groups)
                groupsSelect.append($("<option />").text(groups[index]).attr("value", groups[index]));
            groupsSelect.val(result.groups);
            */
            var autoComplete = new matematici.AutoComplete(groupsSelect[0]);
            autoComplete.initialize();
        },

        setup = function () {
            var buttons = [];

            buttons.push({ cancelButton: true, hidden: true });
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