matematici.Form = function (selector) {
	var
		form = this,
		element = $(selector),
		fields = $("input", element),

		setup = function () {
			//initial setup
			element
				.on("change", ".Trigger", getFormValues)
				.on("click", ".List", getFieldValues(this));

			fields.each(function () {
				var field = $(this);
				if (field.hasClass("DropDownList"))
					new matematici.DropDownList(field);
				else if (field.hasClass("DatePicker"))
					new matematici.DatePicker(field);
			});
			getFormValues();
		},

		getFieldValues = function (field) {
			send("GetFieldValues", function (result) {
				bind(result);
			}, { name: $(field).attr("name") });
		},

		getFormValues = function() {
			send("GetFormValues", function (result) {
				bind(result);
			});
		},

		prepareData = function (data) {
			//prepares data to send
			data = data || {};
			fields.each(function () {
				var field = $(this);
				data[field.attr("name")] = field.val();
			});
			JSON.stringify(data);
		},

		bind = function (data) {
			//binds result to fields
			fields.each(function () {
				var
					field = $(this),
					fieldData = data[field.attr("name")];
				if (fieldData) {
					if (fieldData.value) field.val(field);
					if (fieldData.visible) field.toggle(fieldData.visibility);

					if (fieldData.editable)
						field.removeProp("readonly");
					else
						field.prop("readonly", "readonly");

					//TODO field warnings and errors
				}
			});
			//TODO form warnings and errors
		},

		send = function (event, handler, data) {
			//communicates with server
			$.ajax({
				url: element.attr("action") + "/" + event,
				type: "post",
				data: prepareData(data)
			}).done(function (result) {
				handler(result);
			});
		};

    setup();
}


/*
    binds a form with the api in DataEngine

    usage: 
        new matematici.Form(selector)

		selector - type: String, description: the selector of the html form to bind
 */