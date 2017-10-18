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
						saveForm();
						bind(event, handler, prepareRequest({ data: { source: options.applyButton.attr("id") } }));
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
					bind("Bind", handleResult, prepareRequest({ data: { source: $(this).attr("name") } }));
				});

			fields.each(function () {
				var field = $(this);
				if (field.hasClass("DatePicker"))
					matematici.controls.toggle(field, true);
			});

			bind("Bind", handleResult, loadForm() || prepareRequest());
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
					if (field[0].tagName == "SELECT" && fieldData.options !== undefined) {
						var value = field.val();
						field.empty();
						for (var index in fieldData.options) {
							var option = fieldData.options[index];
							field.append($("<option />").attr("value", option.value).text(option.text));
						}
					}

					if (fieldData.value != null) field.val(fieldData.value);
					field.toggle(!fieldData.visibile);
					matematici.controls.setEditable(field, fieldData.editable && (field[0].tagName != "SELECT" || fieldData.options.length > 0));
					//TODO field warnings and errors
				}
			});

			valid = data.valid;
			matematici.controls.setEditable(options.applyButton, valid);
			//TODO form warnings and errors
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
		bind(event, handler, prepareRequest(parameters));
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