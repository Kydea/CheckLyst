instances.Field = function (item, model) {
    var
        input,
        select,
        select2,
        field,
		label,
		span,
        element = item.getElement(),
		copyButton,
		readOnly = false,

		validate = function () {
		    if (model.valueTypeName == "DateTime" || model.valueTypeName == "Date") {
		        if (!/^$|\d\d\/\d\d\/\d\d\d\d/ig.test(input.val())) {
		            input.val("");
		            return false;
		        }
		    } else if (model.valueTypeName == "Number") {
				if (!/^[\d\-\.]*$/ig.test(input.val())) {
		            input.val("");
		            return false;
		        }
		    }
		    if (model.valueTypeName == "DateTime") {
		        return (
					((input.val() || "").trim() != "" && (select.val() || "").trim() != "" && (select2.val() || "").trim() != "")
					|| ((input.val() || "").trim() == "" && (select.val() || "").trim() == "" && (select2.val() || "").trim() == "")
				);
		    } else if (model.valueTypeName == "Time") {
		        return (
					((select.val() || "").trim() != "" && (select2.val() || "").trim() != "")
					|| ((select.val() || "").trim() == "" && (select2.val() || "").trim() == "")
				);
		    }

		    return true;
		},

		getValue = function () {
		    var value = model.valuesList ? select.val() || "" : (input.val() || "").trim();
		    if (value != "" && model.valueTypeName == "DateTime") 
		        value = value + " " + select.val() + ":" + select2.val();
		    else if (model.valueTypeName == "Time") 
				value = select.val() != "" && select2.val() != "" ? select.val() + ":" + select2.val() : "";
		    else if (model.valueTypeName == "Boolean") 
		        value = input.prop("checked") ? "true" : "false";

		    return value;
		},

        next = function () {
			if (model.valueTypeName != "DateTime" && model.valueTypeName != "Time")
				setTimeout(function () {
					$("input:visible", element.next()).focus();
				}, 0);
        },

		change = function (value) {
			if (validate()) {
                new instances.Event(instances.Events.valueChange, model.id, { value: value }).done(function (result) {
                    instances.instance.update(result);
                    next();
                }).trigger();
            }
        },

		valueCopy = function (e) {
			alert("Value copy");
			alert("Id: " + model.id);
			if (e) {
				e.preventDefault();
				e.stopPropagation();
			}
			new instances.Event(instances.Events.valueCopy, model.id).done(function (result) {
                instances.instance.update(result);
                next();
            }).trigger();
        },

        keyDown = function (e) {
            //f8
            if (e.which == 119) {
                e.preventDefault();
				valueCopy();
			}

			if (e.key == ',') {
				e.preventDefault();
				matematici.dialog.error("Error", "Invalid decimal separator, use dot instead of comma");
			}
        },

        setup = function () {
			label = $("<label />");
			span = $("<span />").addClass("Label").appendTo(label);
            if (!item.isTableContent())
				label.appendTo(element);

            input =
                (
                    model.linesCount == 0 ?
                    $("<input />").attr("type", "text") :
                    $("<textarea />")
				).addClass("Description").appendTo(item.isTableContent() ? element : label);
            if (model.valueTypeName == "Number")
            	input.keyfilter(/[\d\-\,\.]/);
            else if (model.valueTypeName == "Date" || model.valueTypeName == "DateTime") {
				input.keyfilter(/[\d\/]/);
				new matematici.DatePicker(input);
            } else if (model.valueTypeName == "Boolean") {
            	element.addClass(model.valueTypeName);
            	input.attr("type", "checkbox");
            }
            input.on("change", function () {
            	change(getValue());
            }).on("keydown", function(e) {
                keyDown(e);
            });

			if (model.valuesList && model.valuesList.length == 0)
				model.valuesList = null;

			if (model.valuesList || model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
				select = $("<select />").appendTo(item.isTableContent() ? element : label).data("nullable", true);
				select.on("change", function () {
					change(getValue());
				}).on("keydown", function (e) {
					keyDown(e);
				});
			}
            if (model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
            	element.addClass(model.valueTypeName);
            	for (var i = 0; i < 24; i++) {
            		select.append($("<option />").attr("value", (i < 10 ? "0" : "") + i).text((i < 10 ? "0" : "") + i));
            	}
				select2 = $("<select />").appendTo(item.isTableContent() ? element : label).data("nullable", true);
            	select2.on("change", function () {
            		change(getValue());
            	}).on("keydown", function (e) {
            	    keyDown(e);
            	});
            	for (var i = 0; i < 60; i++) {
            		select2.append($("<option />").attr("value", (i < 10 ? "0" : "") + i).text((i < 10 ? "0" : "") + i));
            	}
			}
			if (model.valueTypeName == "Date")
				element.addClass(model.valueTypeName);

            if (model.valuesList && model.valuesList.length == 0)
                model.valuesList = null;

            if (model.valuesList)
                for (var index in model.valuesList)
                    select.append($("<option />").attr("value", model.valuesList[index]).text(model.valuesList[index]));
			field = $("<div />").addClass("Input").appendTo(item.isTableContent() ? element : label);
            if (model.linesCount > 0) {
                element.addClass("MultiLine");
				input.height(36 * model.linesCount);
				field.height(36 * model.linesCount);
            }

            if (model.valuesList)
                element.addClass("List");
            else if (model.externalSourceName && model.externalSourceColumn) {
				element.addClass("ExternalSource").append($("<i />").addClass("fi flaticon-link-o"));
                field.on("click", function () {
                    if (!element.hasClass("Inactive"))
                        new instances.ExternalSources(model.id, model.externalSourceName);
                });
            }

            copyButton =
				$("<a />").addClass("Copy fi flaticon-clone-o").attr({ href: "#", title: "F8" })
					.on("mousedown", valueCopy);

			if (!item.isTableContent() || !item.getParent().isSubRow())
				copyButton.appendTo(item.isTableContent() ? element : label)

			if (select) new matematici.AutoComplete(select).initialize();
			if (select2) new matematici.AutoComplete(select2).initialize();
        };

    this.update = function (model) {
        readOnly = !!model.readOnly || !!model.externalSourceName && !!model.externalSourceColumn;
        span.html(model.description);
        element.toggleClass("Inactive", model.readOnly);
        element.toggleClass("ReadOnly", readOnly);
        input.attr("readonly", readOnly);
        if (model.valueTypeName == "DateTime") {
        	var index = (model.rawValue || "").indexOf(" ");
        	if (index >= 0) {
				input.val(model.rawValue.substr(0, index));
				var index2 = model.rawValue.indexOf(":");
        		if (index2 >= 0) {
					select.val(model.rawValue.substr(index + 1, index2 - index - 1));
					select2.val(model.rawValue.substr(index2 + 1));
				} else {
        			select.val("");
        			select2.val("");
        		}
        	} else {
				input.val(model.rawValue);
        		select.val("");
        		select2.val("");
        	}
        } else if (model.valueTypeName == "Time") {
			input.val(model.rawValue);
			var index = (model.rawValue || "").indexOf(":");
        	if (index >= 0) {
				select.val(model.rawValue.substr(0, index));
				select2.val(model.rawValue.substr(index + 1));
        	} else {
        		select.val("");
        		select2.val("");
        	}
        } else if (model.valueTypeName == "Boolean") {
        	var b = false;
        	try {
				b = $.parseJSON((model.rawValue || "false").toLowerCase());
        	} catch (e) { }
        	input.prop("checked", b);

        	var s = "";
        	try {
				b = $.parseJSON((model.rawValue || "").toLowerCase()); s = b.toString();
        	} catch (e) { }
        	input.val(s);
		} else {
			input.val(model.rawValue);
			if (select) select.val(model.rawValue);
        }

		field.html(model.rawValue).attr("readonly", readOnly);
        field.attr("title", field.text());

        this.setColor(model.color);
		this.setFontSize(model.fontSize);
    };

    this.setColor = function (value) {
        label.css({ color: model.color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.fontSize == 0 ? "" : model.fontSize + "px" });
    };

    setup();
}