matematici.Form = function (selector, options) {
	var
		form = this,
		element = $(selector),
		fieldsSelector = "input, select",
		fields = $(fieldsSelector, element),
		valid = false,
		errors,
		bindResult,

		setup = function () {
			//initial setup

			if (options.applyButton) {
				var
					event = options.applyButton.event,
					handler = options.applyButton.handler;
				options.applyButton = $(options.applyButton.button || options.applyButton);
				if (event)
					options.applyButton.on("click", function () {
						if (options.applyButton.hasClass("ReadOnly")) return;
						saveForm();
						bind(event, handler, prepareRequest({ data: { source: options.applyButton.attr("id") } }));
					});
				else if (handler)
					options.applyButton.on("click", handler);
			}

			element
				.on("keypress change paste", fieldsSelector, function (e) {
					if (e.key != "Tab" && e.which !== 0 && !e.ctrlKey && !e.metaKey && !e.altKey && options.applyButton) {
						matematici.controls.setEditable(options.applyButton, valid);
						if (errors) {
							errors.remove();
							errors = null;
						}
						matematici.controls.setValidationMessages($(this), []);
					}
				})
				.on("change", ".Trigger", function () {
					bind(options.bindEvent, handleResult, prepareRequest({ data: { source: $(this).attr("name") } }));
				});

			fields.each(function () {
				var field = $(this);
				if (field.hasClass("DatePicker"))
					new matematici.DatePicker(field);
			});

			if (options.bindEvent)
				bind(options.bindEvent, handleResult, loadForm() || prepareRequest());
			else
				valid = true;
		},

		getStorageName = function () {
			return "FormData " + location.pathname + "/" + element.attr("id");
		},

		saveForm = function () {
			var storageName = getStorageName();
			try {
				localStorage.setItem(storageName, prepareRequest());
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
				var field = $(this);
				parameters.data[field.attr("name")] =
					field.hasClass("DatePicker") || field.data("type") == "DateTimeOffset" ? field.data("date") :
						field.val();
			});
			return JSON.stringify(parameters);
		},

		setFieldValue = function (field, value) {
			switch (field.data("type")) {
				case "DateTimeOffset":
					if (value == null || value == "")
						field.val("").attr("title", "").data("date", null);
					else {
						var
							date = new Date(value),
							formattedDate = matematici.dateFormatter.format(date, true);
						field.val(formattedDate.date + " " + formattedDate.time).attr("title", formattedDate.offset).data("date", date);
					}
					break;
				default:
					if (field.hasClass("DatePicker"))
						field.data("datePicker").setDate(value)
					else
						field.val(value);
			}

		},

		handleValidationMessages = function (field, data) {
			var validationMessages = data.validationMessages || [];
			if (field)
				matematici.controls.setValidationMessages(field, validationMessages);

			if (validationMessages.length > 0) {
				if (!errors) errors = $("<div />").addClass("Errors").appendTo(element).show();
				if (field)
					errors.append(
						$("<div />").addClass("Error").text(" Some field has errors. Click on error icon of the field for more details.").prepend(
							$("<i />").addClass("fa fa-warning")
						)
					);
				else
					for (var index in data.validationMessages)
						errors.append(
							$("<div />").addClass("Error").text(" " + data.validationMessages[index].message).prepend(
								$("<i />").addClass("fa fa-warning")
							)
						);
			} 
		},

		handleResult = function (data) {
			//updates the form
			fields.each(function () {
				var
					field = $(this),
					fieldData = data.fields[field.attr("name")];

				if (fieldData) {
					var tagName = field[0].tagName;
					if (tagName == "SELECT" && fieldData.options !== undefined) {
						var value = field.val();
						field.empty();
						for (var index in fieldData.options) {
							var option = fieldData.options[index];
							field.append($("<option />").attr("value", option.value).text(option.text));
						}
						if (fieldData.value != null) field.val(fieldData.value);
						var autoComplete = field.data("autoComplete");
						autoComplete.refresh();
						autoComplete.getControl().toggle(!fieldData.visibile);
						matematici.controls.setEditable(autoComplete.getControl(), fieldData.editable && fieldData.options.length > 0);
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
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				data: data
			}).done(function (result) {
				if (event == options.bindEvent) bindResult = result;
				if (options.applyButton && data && data.source && (data.source == options.applyButton.attr("name") || data.source == options.applyButton.attr("id")))
					matematici.controls.setEditable(options.applyButton, false); //by default, the action button is disabled when clicked with success
				if (handler)
					handler(result);
			});
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
			setFieldValue(field, values[field.attr("name")]);
		});
	};

	this.getOptions = function () {
		return options;
	};

	this.handleResult = function (result) {
		handleResult(result);
	};

	this.getBindResult = function () {
		return bindResult;
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