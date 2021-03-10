prototypes.Property = function (propertyPanel, type, caption, information, handler) {
    var
        property = this,
        element,
        value,

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
        element.addClass("Selected");
        type.create(propertyPanel.getPropertyValue().removeClass().addClass("PropertyValue").empty(), this);
        propertyPanel.getInformation().html(information ? information : "");

        $("select", propertyPanel.getPropertyValue()).each(function () {
            var autoComplete = new matematici.AutoComplete(this);
            autoComplete.initialize();
            setTimeout(function () {
                autoComplete.refresh();
            }, 0);
        });
    };

    this.unselect = function () {
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
            type.setValue(value);
        else if (type.hasChanges())
            value.text("...");
    };

    this.setup = setup;
}

