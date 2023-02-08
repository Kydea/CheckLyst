prototypes.Property = function (propertyPanel, type, caption, information, handler) {
    var
        property = this,
        selected = false,
        element,
        value,
        valueParser,
        errors = [],

        updateInformation = function () {
            var informationPanel = propertyPanel.getInformation().empty();
            for (var index in errors)
                informationPanel.append($("<p />").addClass("Error").text(errors[index]).prepend($("<i />").addClass("fal fa-exclamation-triangle")));
            informationPanel.append(information ? information : "");
        },

        setup = function () {
            $("<tr />").appendTo(propertyPanel.getPropertyList())
                .append($("<td />")
                    .append(element =
                        $("<a />")
                            .text(caption)
                            .prepend($("<i />").addClass("fa fa-caret-right"))
                            .attr("href", "#")
                            .on("click", function (e) {
                                e.preventDefault();
                                propertyPanel.setCurrentProperty(property);
                            })
                    )
                )
                .append($("<td />")
                    .append(value =
                        $("<a />")
                            .attr("href", "#")
                            .on("click", function (e) {
                                e.preventDefault();
                                propertyPanel.setCurrentProperty(property);
                            })
                    )
                );

            property.setValue();
        };


    this.getCaption = function () {
        return caption;
    };

    this.resize = function () {
        prototypes.prototype.getElement().css("padding-bottom", element.height());
    };

    this.select = function () {
        selected = true;
        element.addClass("Selected");
        type.create(propertyPanel.getPropertyValue().removeClass().addClass("PropertyValue").empty(), this);
        updateInformation();

        $("select", propertyPanel.getPropertyValue()).each(function () {
            var autoComplete = new matematici.AutoComplete(this);
            autoComplete.initialize();
            setTimeout(function () {
                autoComplete.refresh();
            }, 0);
        });
    };

    this.unselect = function () {
        type.destroy && type.destroy();
        selected = false;
        element.removeClass("Selected");
        $("select", propertyPanel.getPropertyValue()).each(function () {
            matematici.AutoComplete.get(this).destroy();
        });

        propertyPanel.getPropertyValue().empty();
        propertyPanel.getInformation().empty();
    };

    this.getType = function () {
        return type;
    };

    this.updateModel = function (model) {
        matematici.canClose = false;
        property.setValue();
        if (handler) handler.call(this, model);
    };

    this.setValue = function () {
        element.toggleClass("Changed", !!type.hasChanges())

        value.empty();
        if (!valueParser || !valueParser(value, property)) {
            if (type.setValue)
                type.setValue(value, property);
            else if (type.hasChanges())
                value.text("...");
        }
        if (type.setError) type.setError(property.setError);
    };

    this.setValueParser = function (value) {
        valueParser = value;
        return this;
    };

    this.setError = function (errorList, properties, errorMessage) {
        errors = [];
        if (errorList) {
            if (!Array.isArray(properties))
                properties = [properties];

            for (var propertyIndex in properties) {
                var property = properties[propertyIndex];
                for (var index = errorList.length - 1; index >= 0; --index) {
                    var error = errorList[index];
                    if (error.property == property) {
                        if (errorMessage === null)
                            errorList.splice(index, 1);
                        else {
                            if (errorMessage !== undefined) error.message = errorMessage;
                            errors.push(error.message);
                        }
                    }
                }
                if (errors.length == 0 && !!errorMessage) {
                    var error = { property: property, message: errorMessage };
                    errorList.push(error);
                    errors.push(error.message);
                }
            }
        }

        element.toggleClass("Error", errors.length > 0);

        if (selected) updateInformation();
    };

    this.setup = setup;
}

