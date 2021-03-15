matematici.dependenciesDialog = function () {
    var
        renderDependency = function (dependency) {
            return $("<p />").append($("<a />").attr("href", "#").data("dependency", dependency).html(dependency.description));
        },

        navigate = function (e) {
            e.preventDefault();
            var dependency = $(this).data("dependency");
            matematici.dialog.close();
            setTimeout(function () {
                switch (dependency.className) {
                    case "expression":
                        var expressionsList = matematici.expressionsList || new prototypes.ExpressionsList();
                        expressionsList.select(dependency.objectCode);
                        break;
                    case "external source":
                        new prototypes.SourcesList();
                        break;
                    case "checklist":
                        prototypes.prototype.select(null, prototypes.prototype);
                        break;
                    case "page":
                        prototypes.prototype.selectById("Page", dependency.objectID);
                        break;
                    default:
                        prototypes.prototype.selectById("ItemId", dependency.objectID);
                        break;
                }
            }, 0);
        };

    return {
        show: function (title, dependingRelations, dependentRelations, handler) {
            var content = $("<div />").on("click", "a", navigate);
            if ((!dependingRelations || dependingRelations.length == 0) && (!dependentRelations || dependentRelations.length == 0))
                content.append("(no dependency found)");
            else {
                if (dependingRelations && dependingRelations.length > 0) {
                    content.append($("<h3 />").html("contained in this element"));
                    for (var dependency in dependingRelations)
                        content.append(renderDependency(dependingRelations[dependency]));
                }
                if (dependentRelations && dependentRelations.length > 0) {
                    content.append($("<h3 />").html("containing this element"));
                    for (var dependency in dependentRelations)
                        content.append(renderDependency(dependentRelations[dependency]));
                }
            }
            matematici.dialog.show("Dependencies - " + title, content, { text: "ok", icon: "check", handler: handler });
            matematici.dialog.setWidth(800);
        }
    }
}();