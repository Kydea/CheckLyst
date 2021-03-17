prototypes.Property = function (propertyPanel, type, caption, information, handler) {
    var
        property = this,
        selected = false,
        element,
        value,
        errors = [],

        updateInformation = function () {
            var informationPanel = propertyPanel.getInformation();
            informationPanel.html(information ? information : "");
            for (var index in errors)
                informationPanel.append($("<p />").addClass("Error").text(errors[index]).prepend($("<i />").addClass("fal fa-exclamation-triangle")));

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
    }

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
        if (type.setValue)
            type.setValue(value, property);
        else if (type.hasChanges())
            value.text("...");

        if (type.setError) type.setError(property.setError);
    };

    this.setError = function (errorList, properties) {
        errors = [];
        if (errorList) {
            if (!Array.isArray(properties))
                properties = [properties];

            for (var propertyIndex in properties) {
                var property = properties[propertyIndex];
                for (var errorIndex in errorList) {
                    var error = errorList[errorIndex];
                    if (error.property == property)
                        errors.push(error.message);
                }
            }
        }

        element.toggleClass("Error", model.errors && errors.length > 0);

        if (selected) updateInformation();
    };

    this.setup = setup;
}

