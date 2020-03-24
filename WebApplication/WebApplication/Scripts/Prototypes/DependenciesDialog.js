matematici.dependenciesDialog = function () {
    return {
        show: function (list) {
            var content = $("<div />");

            if (!list || list.length == 0)
                content.append("(no dependency found)");
            else
                for (var dependency in list)
                    content.append($("<div />").html(list[dependency].description));
            matematici.dialog.show("dipendenze espressione", content);
        }
    }
}();