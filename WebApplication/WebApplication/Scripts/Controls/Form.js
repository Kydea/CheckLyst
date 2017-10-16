matematici.Form = function (selector, options) {
	var
		form = this,
		element = $(selector),
		fieldsSelector = "input, select",
		fields = $(fieldsSelector, element),
		valid = false,

		setup = function () {
			//initial setup

			if (options.applyButton) {
				var
					event = options.applyButton.event,
					handler = options.applyButton.handler;
				options.applyButton = $(options.applyButton.button || options.applyButton);
				if (event)
					options.applyButton.on("click", function () {
						bind(event, handler, { data: { source: options.applyButton.attr("id") } })
					})
				else if (handler)
					options.applyButton.on("click", handler);
			}

			element
				.on("keypress change paste", fieldsSelector, function (e) {
					if (e.key != "Tab" && e.which !== 0 && !e.ctrlKey && !e.metaKey && !e.altKey && options.applyButton)
						matematici.controls.setEditable(options.applyButton, valid);
				})
				.on("change", ".Trigger", function () {
					bind("Bind", handleResult, { data: { source: $(this).attr("name") } });
				});

			fields.each(function () {
				var field = $(this);
				if (field.hasClass("DatePicker"))
					matematici.controls.toggle(field, true);
			});

			bind("Bind", handleResult);
		},

		prepareRequest = function (parameters) {
			if (!parameters.data) parameters.data = {};

			//prepares the request
			fields.each(function () {
				var field = $(this);
				parameters.data[field.attr("name")] = field.val();
			});
			return JSON.stringify(parameters);
		},

		handleResult = function (data) {
			//updates the form
			fields.each(function () {
				var
					field = $(this),
					fieldData = data.fields[field.attr("name")];

				if (fieldData) {
					if (fieldData.options !== undefined) {
						var value = field.val();
						field.empty();
						for (var index in fieldData.options) {
							var option = fieldData.options[index];
							field.append($("<option />").attr("value", option.value).text(option.text));
						}

						matematici.controls.setEditable(field, fieldData.options.length > 0);
					}

					/*
					if (fieldData.Value !== undefined) field.val(fieldData.Value);
					field.toggle(!fieldData.Visibility);
					matematici.controls.setEditable(field, fieldData.Editable);
					*/
					//TODO field warnings and errors
				}
			});

			valid = data.valid;
			matematici.controls.setEditable(options.applyButton, valid);
			//TODO form warnings and errors
		},

		bind = function (event, handler, parameters) {
			//binds with server
			$.ajax({
				type: "post",
				url: element.attr("action") + "/" + event,
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				data: prepareRequest(parameters || {})
			}).done(function (result) {
				if (options.applyButton && data && data.source && (data.source == options.applyButton.attr("name") || data.source == options.applyButton.attr("id")))
					matematici.controls.setEditable(options.applyButton, false); //by deafult, the action button is disabled when clicked with success
				if (handler)
					handler(result);
			});
		};

	this.initialize = function () {
		setup();
	}

	this.trigger = function (event, handler, parameters) {
		//triggers specific events
		bind(event, handler, parameters);
	}
}

/*
    binds a form with the api in DataEngine

    usage: 
        new matematici.Form(selector, options)

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