matematici.dependenciesDialog = function () {
    return {
        show: function (dependingRelations, dependentRelations) {
            var content = $("<div />");

            if ((!dependingRelations || dependingRelations.length == 0) && (!dependentRelations || dependentRelations.length == 0))
                content.append("(no dependency found)");
            else {
                if (dependingRelations && dependingRelations.length > 0) {
                    content.append($("<h3 />").html("contained in this element"));
                    for (var dependency in dependingRelations)
                        content.append($("<p />").html(dependingRelations[dependency].description));
                }
                if (dependentRelations && dependentRelations.length > 0) {
                    content.append($("<h3 />").html("containing this element"));
                    for (var dependency in dependentRelations)
                        content.append($("<p />").html(dependentRelations[dependency].description));
                }
            }
            matematici.dialog.show("Dependencies", content);
        }
    }
}();