matematici.Form = function (selector) {
	var
		form = this,
		element = $(selector),
		fields = $("input, select", element),

		setup = function () {
			//initial setup
			element
				.on("change", ".Trigger", function () { 
					bind($(this).attr("name"));
				});

			fields.each(function () {
				var field = $(this);
				if (field.hasClass("DatePicker"))
					new matematici.DatePicker(field);
			});
			bind();
		},

		prepareRequest = function (source) {
			//prepares the request
			var data = { Source: source };
			fields.each(function () {
				var field = $(this);
				data[field.attr("name")] = field.val();
			});
			return JSON.stringify({ data: data });
		},

		handleResult = function (data) {
			//updates the form
			fields.each(function () {
				var
					field = $(this),
					fieldData = data.Fields[field.attr("name")];
				if (fieldData) {
					if (fieldData.Options !== undefined) {
						var value = field.val();
						field.empty();
						for (var index in fieldData.Options) {
							var option = fieldData.Options[index];
							field.append($("<option />").attr("value", option.Value).text(option.Text));
						}

						console.log(field.attr("name"), !!value, $("option", field).val(), value || $("option", field).val());

						if (fieldData.Options.length == 0)
							field.prop({ readonly: true, disabled: true });
						else
							field.val(value || $("option", field).val());
					}
					/*
					if (fieldData.Value !== undefined) field.val(fieldData.Value);
					field.toggle(!fieldData.Visibility);
					if (fieldData.Editable)
						field.removeAttr("readonly").removeAttr("disabled");
					else
						field.prop({ readonly: true, disabled: true });
					*/
					//TODO field warnings and errors
				}
			});
			//TODO form warnings and errors
		},

		bind = function (source, handler) {
			//binds with server
			$.ajax({
				type: "post",
				url: element.attr("action"),
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				data: prepareRequest(source)
			}).done(function (result) {
				handleResult(result);
			});
		};

	this.initialize = function () {
		setup();
	}
}


/*
    binds a form with the api in DataEngine

    usage: 
        new matematici.Form(selector)

		selector - type: String, description: the selector of the html form to bind
 */