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
				if (!/^$|\d\d\d\d\-\d\d\-\d\d/ig.test(input.val())) {
		            input.val("");
		            return false;
		        }
			} else if (model.valueTypeName == "Number") {
				var value = input.val();
				for (var index = 0; index < value.length; ++index)
					if (!(
						value[index] >= "0" && value[index] <= "9"
						|| value[index] == "-"
						|| value[index] == localStorage.decimalSeparator
					)) {
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
			if (!model.valuesList) {
                if (value != "" && model.valueTypeName == "DateTime") {
                    var momentDate = matematici.dateFormatter.parse(value, select.val() + ":" + select2.val(), null, true);
                    value = matematici.dateFormatter.date(momentDate, false) + " " + matematici.dateFormatter.time(momentDate, false);
                } else if (model.valueTypeName == "Time")
					value = select.val() != "" && select2.val() != "" ? select.val() + ":" + select2.val() : "";
				else if (model.valueTypeName == "Boolean")
					value = input.prop("checked") ? "true" : "false";
			}
		    return value;
		},

        next = function () {
			if (model.valueTypeName != "DateTime" && model.valueTypeName != "Time")
				setTimeout(function () {
					$("input:visible", element.next()).eq(0).focus();
				}, 0);
        },

        change = function (value) {
            if (model.valuesList || validate()) {
				new instances.Event(instances.Events.valueChange, model.id, { value: value }).done(function (result) {
					instances.instance.update(result);
					next();
				}).trigger();
			} 
        },

		valueCopy = function (e) {
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

			if (model.valueTypeName == "Number" && (e.key == ',' || e.key == '.') && e.key != localStorage.decimalSeparator) {
				e.preventDefault();
				matematici.dialog.error("Error", "Invalid decimal separator, use the character " + localStorage.decimalSeparator + " instead");
			}
        },

        setup = function () {
			label = $("<label />");
			span = $("<span />").addClass("Label").appendTo(label);
            if (!item.isTableContent())
				label.appendTo(element);

            input = (
                model.linesCount < 2 ?
					$("<input />").attr("type", "text") :
					$("<textarea />")
			).addClass("Description").appendTo(item.isTableContent() ? element : label);

			select = select2 = $("<select />");

			if (!model.valueFormulaString) {
				if (!model.valuesList) {
                    if (model.valueTypeName == "Number")
                        input.keyfilter(/[\d\-\,\.]/);
                    else if (model.valueTypeName == "Date" || model.valueTypeName == "DateTime") {
                        input.keyfilter(/[\d\-]/);
                        new matematici.DatePicker(input);
                    } else if (model.valueTypeName == "Boolean") {
                        element.addClass(model.valueTypeName);
                        input.attr("type", "checkbox");
                    }
                    else
                        input.addClass("Scan");

					input.on("change", function () {
						change(getValue());
					}).on("keydown", function (e) {
						keyDown(e);
					});
				}

				if (model.valuesList || model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
					select = $("<select />").appendTo(item.isTableContent() ? element : label).data("nullable", true);
					select.on("change", function () {
						change(getValue());
					}).on("keydown", function (e) {
						keyDown(e);
					});
				}

				if (model.valuesList) {
					element.addClass("List");
					for (var index in model.valuesList)
						select.append($("<option />").attr("value", model.valuesList[index]).text(model.valuesList[index]));
				} else if (model.externalSourceName && model.externalSourceColumn) {
					var
						icon = $("<i />").addClass("ExternalSourceLink fa fa-link"),
						call = function () {
							if (!element.hasClass("Inactive"))
								new instances.ExternalSources(model.id, model.externalSourceName);
						};
					element.addClass("ExternalSource").append(icon);
					input.on("click", call);
					icon.on("click", call);
				} else if (model.valueTypeName == "DateTime" || model.valueTypeName == "Time") {
					element.addClass(model.valueTypeName);
					for (var i = 0; i < 24; i++) 
						select.append($("<option />").attr("value", (i < 10 ? "0" : "") + i).text((i < 10 ? "0" : "") + i));
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
				else if (model.valueTypeName == "Date")
					element.addClass(model.valueTypeName);
			}

			field = $("<div />").addClass("Input").appendTo(item.isTableContent() ? element : label);
            if (model.linesCount > 0) 
				element.addClass("MultiLine");
			if (model.linesCount > 1) {
				input.outerHeight(20 * model.linesCount + 12);
				field.outerHeight(20 * model.linesCount + 12);
            }

            copyButton =
				$("<a />").addClass("Copy fal fa-clone").attr({ href: "#", title: "F8" })
					.on("mousedown", valueCopy);

			if (!item.isTableContent() || !item.getParent().isSubRow())
				copyButton.appendTo(item.isTableContent() ? element : label)

			if (select) new matematici.AutoComplete(select).initialize();
			if (select2) new matematici.AutoComplete(select2).initialize();
        };

	this.update = function (model) {
        readOnly = !!model.readOnly || !!model.externalSourceName;
        span.html(model.descriptionValue);
        element.toggleClass("Inactive", model.readOnly);
        element.toggleClass("ReadOnly", readOnly);
		input.attr("readonly", readOnly);
		if (model.valuesList)
			select.val(model.rawValue)
		else if (model.valueFormulaString)
			input.val(model.rawValue);
        else if (model.valueTypeName == "DateTime") {
            if (!model.rawValue || model.rawValue == "") {
                input.val("");
                select.val("");
                select2.val("");
            } else {
                var
                    index = model.rawValue.indexOf(" "),
                    date = model.rawValue.substr(0, index),
                    time = model.rawValue.substr(index + 1),
                    m = matematici.dateFormatter.parse(date, time, null, false),
                    formattedDate = matematici.dateFormatter.format(m, true);
                input.val(formattedDate.date);
                select.val(formattedDate.hours);
                select2.val(formattedDate.minutes);
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

            input.prop("disabled", readOnly);
		} else 
			input.val(model.rawValue);

        var rawValue = model.rawValue;
        field.html(readOnly && rawValue ? rawValue.split('\n').join("<br />") : rawValue).attr("readonly", readOnly);
        field.attr("title", rawValue);

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