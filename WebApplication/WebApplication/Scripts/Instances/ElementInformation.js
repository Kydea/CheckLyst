instances.ElementInformation = function (parent) {
    var
        element,
        panel,

        click = function (e) {
            e.stopPropagation();
            e.preventDefault();

            if (instances.ElementInformation.current && instances.ElementInformation.current != element) instances.ElementInformation.current.removeClass("Active");
            element.toggleClass("Active");
            if (element.hasClass("Active")) instances.ElementInformation.current = element;
        },

        setup = function () {
            element = $("<div />").addClass("Information").appendTo(parent).on("click", click);
            element.siblings().eq(0).filter("label").css("cursor", "pointer").on("click", click);
            panel = $("<div />").addClass("Panel").appendTo(element).on("click", click);
        },

        addItem = function(icon, value) {
            if (value != null) panel.append($("<div />").append($("<span />").text(value).prepend($("<i />").addClass("fa fa-fw fa-" + icon))));
        };

    this.update = function (model) {
        panel.empty();
        addItem("tag", model.Id + (model.Code ? " - " + model.Code : ""));
        addItem(model.Valid ? "thumbs-o-up" : "thumbs-o-down", model.Valid ? "valid" : model.ValidityMessage || "not valid");
        if (!model.ReadOnly) addItem("pencil", "editable");
        addItem("calculator", model.ValueFormula);
        addItem("check-square-o", model.SignatureInformation);
        addItem("link", (model.ExternalSourceName ? model.ExternalSourceName + "." + model.ExternalSourceColumn : null));
        if (model.History && model.History.length > 0)
            panel.append($("<div />").append($("<a />").attr("href", "#").text("history").prepend($("<i />").addClass("fa fa-fw fa-history")).on("click", function (e) {
                new matematici.CheckListGrid(model.History);
            })));
    }

    setup();
}