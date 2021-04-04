prototypes.LinkProperty = function (model) {
    var
        base,
        urlRadio, imageRadio, attachmentRadio,
        input, select,
        readOnly = prototypes.prototype.getModel().state != prototypes.Prototype.states.draft,

        fillSelect = function (preserveValue) {
            var filter = imageRadio.is(":checked") ? "Image" : attachmentRadio.is(":checked") ? "Attachment" : null;
            if (filter) {
                var found = false;
                select.empty();
                for (var code in prototypes.prototype.getElements([filter])) {
                    select.append($("<option />").attr({ value: code }).text(code));
                    if (code == model.elementCode) found = true;
                }
            } 
            setTimeout(function () {
                var autoComplete = matematici.AutoComplete.get(select[0]);
                autoComplete.toggle(!!filter);
                if (filter) {
                    if (preserveValue) {
                        if (found)
                            autoComplete.setValue(model.elementCode);
                        else
                            autoComplete.setError(model.elementCode);
                    }
                    autoComplete.refreshValues();
                    autoComplete.refresh();
                } else
                    autoComplete.setValue(null);
            }, 0);
        },

        radioClicked = function () {
            input.toggle(urlRadio.is(":checked"));
            select.toggle(!urlRadio.is(":checked"));
            input.val(null);
            select.val(null);
            model.elementCode = model.elementType = model.url = null;
            fillSelect(false);
            base.setError(model.errors, "elementCode", null);
        },

        change = function () {
            var
                image = imageRadio.is(":checked"),
                attachment = attachmentRadio.is(":checked"),
                url = !image && !attachment;
            model.url = image || attachment ? null : input.val();
            model.elementType = image ? "Image" : attachment ? "Attachment" : null;
            model.elementCode = url ? null : select.val();
            base.updateModel(model);
            base.setError(model.errors, "elementCode", null);
        };

    this.create = function (valuePanel, baseClass) {
        base = baseClass;
        valuePanel.addClass("Link");

        var
            image = model.elementType == "Image",
            attachment = model.elementType == "Attachment",
            url = !image && !attachment;

        $("<label />").text("url").prepend(urlRadio = $("<input />").attr({ type: "radio", name: "ElementType", value: "url" }).attr('disabled', readOnly).prop("checked", url)).appendTo(valuePanel);
        $("<label />").text("image").prepend(imageRadio = $("<input />").attr({ type: "radio", name: "ElementType", value: "image" }).attr('disabled', readOnly).prop("checked", image)).appendTo(valuePanel);
        $("<label />").text("attachment").prepend(attachmentRadio = $("<input />").attr({ type: "radio", name: "ElementType", value: "attachment" }).attr('disabled', readOnly).prop("checked", attachment)).appendTo(valuePanel);

        input = $("<input />")
            .on("change", change)
            .attr({ type: "text", readOnly: readOnly })
            .val(model.url)
            .toggle(url || readOnly);

        select = $("<select />")
            .on("change", change)
            .appendTo(valuePanel);

        if (readOnly)
            input.val(url ? model.url : model.elementCode);
        else {
            urlRadio.on("click", radioClicked);
            imageRadio.on("click", radioClicked);
            attachmentRadio.on("click", radioClicked);
            input.on("change", change);
            fillSelect(true);
        }

        valuePanel.append(input);
    };

    this.hasChanges = function () {
        return model.url || model.elementType;
    };

    this.setError = function(handler) {
        handler(model.errors, "link");
    };
}