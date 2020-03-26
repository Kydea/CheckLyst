matematici.dependenciesDialog = function () {
    return {
        show: function (dependingNodes, dependentNodes) {
            var content = $("<div />");

            if ((!dependingNodes || dependingNodes.length == 0) && (!dependentNodes || dependentNodes.length == 0))
                content.append("(no dependency found)");
            else {
                if (dependingNodes && dependingNodes.length > 0) {
                    content.append($("<h3 />").html("contained in this element"));
                    for (var dependency in dependingNodes)
                        content.append($("<p />").html(dependingNodes[dependency].description));
                }
                if (dependentNodes && dependentNodes.length > 0) {
                    content.append($("<h3 />").html("containing this element"));
                    for (var dependency in dependentNodes)
                        content.append($("<p />").html(dependentNodes[dependency].description));
                }
            }
            matematici.dialog.show("Dependencies", content);
        }
    }
}();