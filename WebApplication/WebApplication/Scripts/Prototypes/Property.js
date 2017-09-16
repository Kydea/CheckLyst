prototypes.Property = function (propertyPanel, type, caption, information) {
    var
        property = this,
        element,

        setup = function () {
            element =
                $("<a />")
                    .text(caption)
                    .prepend($("<i />").addClass("fa fa-caret-right"))
                    .attr("href", "#")
                    .appendTo(propertyPanel.getPropertyList())
                    .toggleClass("Changed", !!type.hasChanges())
                    .on("click", function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        propertyPanel.setCurrentProperty(property);
                    });
        }

    this.resize = function () {
        prototypes.prototype.getElement().css("padding-bottom", element.height());
    };

    this.select = function () {
        element.addClass("Selected");
        type.create(propertyPanel.getPropertyValue().removeClass().addClass("PropertyValue").empty());
        propertyPanel.getInformation().html(information ? information : "");
    };

    this.unselect = function () {
        element.removeClass("Selected");
        propertyPanel.getPropertyValue().empty();
        propertyPanel.getInformation().empty();
    };

    this.getElement = function () {
        return element;
    };
    
    this.getType = function () {
        return type;
    };

    setup();
}

