instances.ElementInformation = function (parent) {
    var
        element,
        panel,
        model,

        addItem = function (icon, value) {
            var span = $("<span />").html(value);
            span.attr({ title: span.text() });
            if (value != null) panel.append($("<div />").append(span.prepend($("<i />").addClass("fa fa-fw fa-" + icon))));
        },

        setContent = function () {
            if (panel)
                panel.empty();            
            else
                panel = $("<div />").addClass("Panel").appendTo(element).on("click", click);

            addItem("tags", model.id + (model.code ? " - " + model.code : ""));
            addItem(model.valid ? "thumbs-up" : "thumbs-down", model.valid ? "valid" : model.validityMessage || "not valid");
            if (!model.readOnly) addItem("edit", "editable");
            var formulaParser = $("<span />").html(model.value);
            addItem("calculator", model.valueFormulaString ? model.valueFormulaString + ": <strong>" + formulaParser.text() + "</strong>" : null);
            addItem("link", (model.externalSourceName ? model.externalSourceName + "." + model.externalSourceColumn : null));
            if (model.history && model.history.length > 0)
                panel.append($("<div />").append($("<a />").attr("href", "#").text("history").prepend($("<i />").addClass("fa fa-fw fa-list-ul")).on("click", function (e) {
                    var columns =
                        model.typeName == "Signature" ? [
                            { name: "dateTime", label: "Date/Time", type: "DateTimeOffset" },
                            { name: "user", label: "User" },
                            { name: "signature", label: "Signature" },
                            { name: "note", label: "Note" },
                            { name: "action", label: "Action" },
                        ] : [
                            { name: "dateTime", label: "Date/Time", type: "DateTimeOffset" },
                            { name: "user", label: "User" },
                            { name: "value", label: "Value", type: "Boolean" }
                        ];
                    //back compatbility before icon change for signature history
                    new matematici.CheckListGrid(model.history, null, null, columns, function (checkListGrid, rowIndex, row) {
                        row.action = row.action && row.action.replace("fi fi-fw flaticon-check", "fal fa-check").replace("fi fi-fw flaticon-trash-o", "fal fa-trash");
                    });
                })));
            if (model.typeName == "Field" && !model.readOnly)
                panel.append($("<div />").append($("<a />").attr("href", "#").text("clear value").prepend($("<i />").addClass("fa fas fa-power-off")).on("click", function (e) {
                    new instances.Event(instances.Events.valueChange, model.id, { value: null }).trigger();
                })));
        },

        click = function (e) {
            e.stopPropagation();
            e.preventDefault();

            setContent();
            if (instances.ElementInformation.current && instances.ElementInformation.current != element) instances.ElementInformation.current.classList.remove("Active");
            element.classList.toggle("Active");
            if (element.classList.contains("Active")) instances.ElementInformation.current = element;
        },

        setup = function () {
            element = document.createElement("div");
            element.className = "Information";
            element.addEventListener("click", click);
            var label = parent.childNodes[0];
            if (label.nodeName == "LABEL") {
                span = label.querySelector("span.Label");
                if (span) {
                    span.style.cursor = "pointer";
                    span.addEventListener("click", click);
                }

            }
            parent.appendChild(element);
        };

    this.update = function (m) {
        model = m;
        panel = null;
    }

    setup();
}