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
			element.siblings().eq(0).filter("label").children("span.Label").css("cursor", "pointer").on("click", click);
            panel = $("<div />").addClass("Panel").appendTo(element).on("click", click);
        },

        addItem = function(icon, value) {
            if (value != null) panel.append($("<div />").append($("<span />").text(value).prepend($("<i />").addClass("fi fi-fw flaticon-" + icon))));
        };

    this.update = function (model) {
        panel.empty();
		addItem("tags-o", model.id + (model.code ? " - " + model.code : ""));
        addItem(model.valid ? "thumbs-up-o" : "thumbs-down-o", model.valid ? "valid" : model.validityMessage || "not valid");
		if (!model.readOnly) addItem("edit-o", "editable");
        addItem("calculator", model.valueFormula);
        addItem("check", model.signatureInformation);
        addItem("link-0", (model.externalSourceName ? model.externalSourceName + "." + model.externalSourceColumn : null));
        if (model.history && model.history.length > 0)
			panel.append($("<div />").append($("<a />").attr("href", "#").text("history").prepend($("<i />").addClass("fi fi-fw flaticon-list-ul")).on("click", function (e) {
                new matematici.CheckListGrid(model.history);
            })));
    }

    setup();
}