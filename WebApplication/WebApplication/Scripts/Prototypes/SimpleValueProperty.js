prototypes.SimpleValueProperty = function (model, property, options) {
    var
        input,
        handler,
        check,
        change = function () {
            var value = input.val();
            if (check) 
                for(var index in check)
                    if (!check[index].action.call(this, value)) {
                        new matematici.dialog.error("error", check[index].message);
                        return;
                    }
            model[property] = value;
            matematici.canClose = false;
            if (handler) handler.call(this, model);
        };

    this.setHandler = function (value) {
        handler = value;
        return this;
    };

    this.setCheck = function (value) {
        check = value;
        return this;
    };

    this.create = function (valuePanel) {
        valuePanel.addClass("SimpleValue");
        options = options || {};
        input = $(options.fullHeight ? "<textarea />" : "<input />")
            .attr({ type: "text", maxlength: options.maxLength, readOnly: options.readOnly })
            .val(model[property])
        if (!options.readOnly)
            input.on("change", change);
        if (options.number) input.keyfilter(/[\d\-\,]/);
        valuePanel.append(input);
    };

    this.hasChanges = function () {
        return !!model[property];
    };

    this.setReadOnly = function (value) {
        options.readOnly = value
    };
}

