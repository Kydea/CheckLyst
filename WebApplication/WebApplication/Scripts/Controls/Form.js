matematici.Form = function (selector, options) {
	var
		form = this,
		element = $(selector),
		fieldsSelector = "input[name], select[name], textarea[name]",
		fields = $(fieldsSelector, element),
		valid = false,
		errors,
		fieldErrors = false,
		bindResult,
		changeHandlers = [],

		setup = function () {
			//initial setup

            options = options || {};

			if (options.applyButton) {
				var
					event = options.applyButton.event,
					handler = options.applyButton.handler;
				options.applyButton = $(options.applyButton.button || options.applyButton);
				if (event)
					options.applyButton.on("click", function (e) {
						if (options.applyButton.hasClass("ReadOnly")) return;
						saveForm();
						bind(event, handler, prepareRequest({ data: { source: options.applyButton.attr("id") } }));
					});
				else if (handler)
					options.applyButton.on("click", function () {
						if (!options.applyButton.hasClass("ReadOnly"))
							handler();
					});
			}

			element
				.on("keypress", fieldsSelector, function (e) {
					if (e.key != "Tab" && e.which !== 0 && !e.ctrlKey && !e.metaKey && !e.altKey) 
						onChange($(this));
				})
				.on("change, paste", fieldsSelector, function (e) {
					onChange($(this));
				})
				.on("change", ".Trigger", function () {
					bind(options.bindEvent, handleResult, prepareRequest({ data: { source: $(this).attr("name") } }));
				});

			fields.each(function () {
				var field = $(this);
				if (field.hasClass("DatePicker"))
					new matematici.DatePicker(field);

				if (options.readOnly)
					matematici.controls.setEditable(field, false);

				if (field.data("autoComplete")) field.data("autoComplete").checkReadOnly();
			});

			if (options.bindEvent)
				bind(options.bindEvent, handleResult, loadForm() || prepareRequest());
			else
				valid = true;
		},

		clearFormErrors = function () {
			if (errors) {
				errors.remove();
				errors = null;
				fieldErrors = false;
			}
		},

		clearAllErrors = function () {
			clearFormErrors();
			fields.each(function () {
				matematici.controls.setValidationMessages($(this), []);
			});
		},

		getStorageName = function () {
			return "FormData " + location.pathname + "/" + element.attr("id");
		},

		saveForm = function () {
			var storageName = getStorageName();
			try {
                localStorage.setItem(storageName, prepareRequest());
                localStorage.setItem(storageName + "/AdvancedSearch", options.searchPage && options.searchPage.isAdvancedSearchMode && options.searchPage.isAdvancedSearchMode());
			} catch (error) {
				console.log(error);
				try { localStorage.removeItem(storageName) } catch (error) { }
			}

		},

		loadForm = function () {
			var
				storageName = getStorageName(),
				value;
			try {
                value = localStorage.getItem(storageName);
                var advancedSearch = localStorage.getItem(storageName + "/AdvancedSearch");
                options.searchPage.setAdvancedSearchMode(advancedSearch == "true");
			} catch (error) {
				value = null;
				console.log(error);
				try { localStorage.removeItem(storageName) } catch (error) { }
			}
			return value;
		},

		prepareRequest = function (parameters) {
			parameters = parameters || {};
			parameters.data = parameters.data || {};

			//prepares the request
            fields.each(function () {
                var
                    field = $(this),
                    apply = options.searchPage && options.searchPage.isAdvancedSearchMode && options.searchPage.isAdvancedSearchMode() || field.closest("div.AdvancedSearch").length == 0;
                parameters.data[field.attr("name")] =
                    field.hasClass("DatePicker") || field.data("type") == "DateTimeOffset" ? (apply ? field.data("date") : null) :
					field.attr("type") == "checkbox" ? (apply ? field.prop("checked") : false) :
                    (apply ? field.val() : "");
			});

            parameters.data["advancedSearch"] = options.searchPage && options.searchPage.isAdvancedSearchMode && options.searchPage.isAdvancedSearchMode();

			return JSON.stringify(parameters);
		},

		prettifyObject = function (object, indentation) {
			if (object == null)
				return "";
			else if (typeof object == "object") {
				var value = ""; 
				for (var index in object) 
					if (object[index] != null && object[index] != "")
						value += "\n" + Array(indentation + 1).join(" ") + index + ": " + prettifyObject(object[index], indentation + 4);
				return value;
			} else
				return object;
		},

		setFieldValue = function (field, value) {
			if (field.data("type") == "DateTimeOffset") {
				if (value == null || value == "")
					field.val("").attr("title", "").data("date", null);
				else {
					var
						date = new Date(value),
						formattedDate = matematici.dateFormatter.format(date, true);
					field.val(formattedDate.date + " " + formattedDate.time).attr("title", formattedDate.offset).data("date", date);
				}
			}
			else if (field.data("type") == "JSON")
				try {
					value = prettifyObject(JSON.parse(value), 0);
					field.val(value);
				} catch (error) {
					field.val(value);
				}
			else if (field.attr("type") == "checkbox")
				field.prop("checked", !!value)
			else if (field.hasClass("DatePicker"))
				field.data("datePicker").setDate(value, true);
			else
				field.val(value);
		},

		handleValidationMessages = function (field, data) {
			var validationMessages = data.validationMessages || [];
			if (field)
				matematici.controls.setValidationMessages(field, validationMessages);

			if (validationMessages.length > 0) {
				if (!errors) errors = $("<div />").addClass("Errors").appendTo(element).show();
				if (!field)
					for (var index in data.validationMessages)
						errors.append(
							$("<div />").addClass("Error").text(" " + data.validationMessages[index].message).prepend(
                                $("<i />").addClass("fas fa-exclamation-triangle")
							)
						);
				else if (!fieldErrors) {
					fieldErrors = true;
					errors.append(
						$("<div />").addClass("Error").text("Some field is not valid. Click on the error icon of the fields for more details.").prepend(
                            $("<i />").addClass("fas fa-exclamation-triangle")
						)
					);
				}
			}
		},

		handleResult = function (data) {
			//updates the form
			clearAllErrors();

			$(fieldsSelector, element).each(function () { //i use jquery on fieldsSelector again to include the progammatically added fields
				var
					field = $(this),
					fieldData = data.fields[field.attr("name")];

				if (fieldData) {
					var tagName = field[0].tagName;
					if (tagName == "SELECT" && fieldData.options !== undefined) {
						var
							value = field.val(),
							autoComplete = field.data("autoComplete");
						field.empty();
						for (var index in fieldData.options) {
							var option = fieldData.options[index];
							field.append($("<option />").attr("value", option.value).text(option.text));
						}
						if (fieldData.value != null)
							field.val(fieldData.value);
						else if (autoComplete.isNullable())
							field.val(null);
						autoComplete.refresh();
						autoComplete.getControl().toggle(!fieldData.visibile);
						matematici.controls.setEditable(autoComplete.getControl(), fieldData.editable && fieldData.options.length > 0 && !options.readOnly);
					} else {
						if (fieldData.value != null) setFieldValue(field, fieldData.value);
						field.toggle(!fieldData.visibile);
						matematici.controls.setEditable(field, fieldData.editable);
					}

					handleValidationMessages(field, fieldData);
				}
			});

			handleValidationMessages(null, data);
			valid = data.valid;
			matematici.controls.setEditable(options.applyButton, valid);
		},

		bind = function (event, handler, data) {
			//binds with server
			$.ajax({
				type: "post",
				url: element.attr("action") + "/" + event,
				contentType: "application/json; charset=utf-8",
				data: data
			}).done(function (result) {
				if (event == options.bindEvent) bindResult = result;
				if (options.applyButton && data && data.source && (data.source == options.applyButton.attr("name") || data.source == options.applyButton.attr("id")))
					matematici.controls.setEditable(options.applyButton, false); //by default, the action button is disabled when clicked with success
				if (handler)
					handler(result);
			}).progress(function (e) {
				console.log(e);
			});
		},

		onChange = function(control) {
			clearFormErrors();
			if (options.applyButton)
				matematici.controls.setEditable(options.applyButton, true);

			for (var index in changeHandlers) 
				changeHandlers[index].call();
		};

	this.initialize = function () {
		setup();
	};

	this.trigger = function (event, handler, parameters) {
		//triggers specific events
		bind(event, handler || handleResult, prepareRequest(parameters));
	};

	this.setValues = function (values) {
		fields.each(function () {
			var field = $(this);
			setFieldValue(field, values && values[field.attr("name")]);
		});
	};

	this.getOptions = function () {
		return options;
	};

	this.handleResult = handleResult;

	this.onChange = onChange;

	this.addChangeHandler = function (handler) {
		changeHandlers.push(handler);
	};

	this.getBindResult = function () {
		return bindResult;
	};

	this.getElement = function () {
		return element;
	};
};

/*
    binds a form with the api in DataEngine

    usage: 
        new matematici.Form(selector, options) - note: invoke initialize explicitely or use the construtor in initializers

		selector - type: String, description: the selector of the html form to bind
		options - type: object, description: a PlainObjects:
            applyButton - type: htmlString or Element or Array or jQuery, description: the element(s) to toggle when a field change its value
		    or:
            applyButton - type: object, description: the element(s) to toggle when a field change its value
				button: - type: htmlString or Element or Array or jQuery, description: the element(s) to toggle when a field change its value
				handler: - type: function, description: the function to call on click
		    or:
            applyButton - type: object, description: the element(s) to toggle when a field change its value
				button: - type: htmlString or Element or Array or jQuery, description: the element(s) to toggle when a field change its value
				event: - type: string, description: the event to use with bind on click
				handler: - type: function, description: the function to call on bind result
*/