prototypes.Property = function (propertyPanel, type, caption, information) {
	var
		property = this,
		element,

		setup = function () {
			element =
				$("<a />")
					.text(caption)
					.prepend($("<i />").addClass("fi flaticon-arrow-right"))
					.attr("href", "#")
					.appendTo(propertyPanel.getPropertyList())
					.toggleClass("Changed", !!type.hasChanges())
					.on("click", function (e) {
						e.preventDefault();
						e.stopPropagation();
						propertyPanel.setCurrentProperty(property);
					});
		};


	this.getCaption = function () {
		return caption;
	}

	this.resize = function () {
		prototypes.prototype.getElement().css("padding-bottom", element.height());
	};

	this.select = function () {
		element.addClass("Selected");
		type.create(propertyPanel.getPropertyValue().removeClass().addClass("PropertyValue").empty());
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
			$(this).data("autoComplete").destroy();
		});

		propertyPanel.getPropertyValue().empty();
		propertyPanel.getInformation().empty();
	};

	this.getElement = function () {
		return element;
	};

	this.getType = function () {
		return type;
	};

	this.setup = setup;
}

