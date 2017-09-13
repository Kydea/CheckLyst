instances.Field = function (item, model) {
    var
        input,
        select,
        select2,
        field,
        label,
        element = item.getElement(),
        copyButton,

		validate = function () {
		    if (model.ValueTypeName == "DateTime" || model.ValueTypeName == "Date") {
		        if (!/^$|\d\d\/\d\d\/\d\d\d\d/ig.test(input.val())) {
		            input.val("");
		            return false;
		        }
		    } else if (model.ValueTypeName == "Number") {
		        if (!/^[\d\-\,\.]*$/ig.test(input.val())) {
		            input.val("");
		            return false;
		        }
		    }
		    if (model.ValueTypeName == "DateTime") {
		        return (
					((input.val() || "").trim() != "" && (select.val() || "").trim() != "" && (select2.val() || "").trim() != "")
					|| ((input.val() || "").trim() == "" && (select.val() || "").trim() == "" && (select2.val() || "").trim() == "")
				);
		    } else if (model.ValueTypeName == "Time") {
		        return (
					((select.val() || "").trim() != "" && (select2.val() || "").trim() != "")
					|| ((select.val() || "").trim() == "" && (select2.val() || "").trim() == "")
				);
		    }

		    return true;
		},

		getValue = function () {
		    var value = model.ValuesList ? select.val() || "" : (input.val() || "").trim();
		    if (value != "" && model.ValueTypeName == "DateTime") 
		        value = value + " " + select.val() + ":" + select2.val();
		    else if (model.ValueTypeName == "Time") 
		        value = select.val() + ":" + select2.val();
		    else if (model.ValueTypeName == "Boolean") 
		        value = input.prop("checked") ? "true" : "false";

		    return value;
		},

        next = function () {
            if (model.ValueTypeName != "DateTime" && model.ValueTypeName != "Time") {
                input.datepicker('hide').blur()
                var next = element.next().children("input, select");
                if (next.size() > 0)
                    next.focus();
            }
        },

        change = function (value) {
            if (validate()) {
                new instances.Event(instances.Events.valueChange, model.Id, { Value: value }).done(function (result) {
                    instances.instance.update(result);
                    next();
                }).trigger();
            }
        },

        copyValue = function () {
            new instances.Event(instances.Events.copyValue, model.Id).done(function (result) {
                instances.instance.update(result);
                next();
            }).trigger();
        },

        keyDown = function (e) {
            //f8
            if (e.which == 119) {
                e.preventDefault();
                copyValue();
            }
        },

        setup = function () {
            label = $("<label />");
            if (!item.isTableContent())
                label.appendTo(element);
            input =
                (
                    model.LinesCount == 0 ?
                    $("<input />").attr("type", "text") :
                    $("<textarea />")
                ).addClass("Description").appendTo(element);
            if (model.ValueTypeName == "Number")
            	input.keyfilter(/[\d\-\,\.]/);
            else if (model.ValueTypeName == "Date" || model.ValueTypeName == "DateTime") {
            	input.keyfilter(/[\d\/]/);
            	input.datepicker({
            		beforeShow: function (el) {
            			return !$(el).prop("readonly");
            		}
            	})
            } else if (model.ValueTypeName == "Boolean") {
            	element.addClass(model.ValueTypeName);
            	input.attr("type", "checkbox");
            }
            input.on("change", function () {
            	change(getValue());
            }).on("keydown", function(e) {
                keyDown(e);
            });

            select = $("<select />").appendTo(element);
            select.on("change", function () {
            	change(getValue());
            }).on("keydown", function (e) {
                keyDown(e);
            });

            if (model.ValueTypeName == "DateTime" || model.ValueTypeName == "Time") {
            	element.addClass(model.ValueTypeName);
            	select.append($("<option />").attr("value", "").text(""));
            	for (var i = 0; i < 24; i++) {
            		select.append($("<option />").attr("value", (i < 10 ? "0" : "") + i).text((i < 10 ? "0" : "") + i));
            	}
            	select2 = $("<select />").appendTo(element);
            	select2.on("change", function () {
            		change(getValue());
            	}).on("keydown", function (e) {
            	    keyDown(e);
            	});
                select2.append($("<option />").attr("value", "").text(""));
            	for (var i = 0; i < 60; i++) {
            		select2.append($("<option />").attr("value", (i < 10 ? "0" : "") + i).text((i < 10 ? "0" : "") + i));
            	}
			}

            if (model.ValuesList && model.ValuesList.length == 0)
                model.ValuesList = null;

            if (model.ValuesList)
                for (var index in model.ValuesList)
                    select.append($("<option />").attr("value", model.ValuesList[index]).text(model.ValuesList[index]));
            field = $("<div />").addClass("Input").appendTo(element);
            if (model.LinesCount > 0) {
                element.addClass("MultiLine");
                input.height(24 * model.LinesCount - 10);
                field.height(24 * model.LinesCount - 10);
            }

            if (model.ValuesList)
                element.addClass("List");
            else if (model.ExternalSourceName && model.ExternalSourceColumn) {
                element.addClass("ExternalSource").append($("<i />").addClass("fa fa-link"));
                field.on("click", function () {
                    if (!element.hasClass("Inactive"))
                        new instances.ExternalSources(model.Id, model.ExternalSourceName);
                });
            }

            copyButton =
                $("<i />").addClass("Copy fa fa-copy").attr({ href: "#", title: "F8" })
                    .appendTo(element)
                    .on("mousedown", function () { copyValue(); });
        };

    this.update = function (model) {
        var readOnly = !!model.ReadOnly || !!model.ExternalSourceName && !!model.ExternalSourceColumn;
        label.html(model.Description);
        element.toggleClass("Inactive", model.ReadOnly);
        element.toggleClass("ReadOnly", readOnly);
        input.attr("readonly", readOnly);
        if (model.ValueTypeName == "DateTime") {
        	var index = (model.Value || "").indexOf(" ");
        	if (index >= 0) {
        		input.val(model.Value.substr(0, index));
        		var index2 = model.Value.indexOf(":");
        		if (index2 >= 0) {
        			select.val(model.Value.substr(index + 1, index2 - index - 1));
        			select2.val(model.Value.substr(index2 + 1));
				} else {
        			select.val("");
        			select2.val("");
        		}
        	} else {
        		input.val(model.Value);
        		select.val("");
        		select2.val("");
        	}
        } else if (model.ValueTypeName == "Time") {
        	input.val(model.Value);
        	var index = (model.Value || "").indexOf(":");
        	if (index >= 0) {
        		select.val(model.Value.substr(0, index));
        		select2.val(model.Value.substr(index + 1));
        	} else {
        		select.val("");
        		select2.val("");
        	}
        } else if (model.ValueTypeName == "Boolean") {
        	var b = false;
        	try {
        		b = $.parseJSON((model.Value || "false").toLowerCase());
        	} catch (e) { }
        	input.prop("checked", b);

        	var s = "";
        	try {
        		b = $.parseJSON((model.Value || "").toLowerCase()); s = b.toString();
        	} catch (e) { }
        	input.val(s);
		} else {
        	input.val(model.Value);
        	select.val(model.Value);
        }

        field.html(model.Value).attr("readonly", readOnly);
        field.attr("title", field.text());

        this.setColor(model.Color);
        this.setFontSize(model.FontSize);
    };

    this.setColor = function (value) {
        label.css({ color: model.Color });
    };

    this.setFontSize = function (value) {
        label.css({ fontSize: model.FontSize == 0 ? "" : model.FontSize + "px" });
    };

    setup();
}