matematici.ChangeStateSignature = function (selector, options) {
	var
		self = this,
		element = $(selector),
		concurrencyException = false,
		validityStart = $("input#ValidityStart", element).keyfilter(/[\d\/]/).datepicker({
			beforeShow: function (element) { return !$(element).prop("readonly"); }
		}),
		validityStartMessage = $("div#ValidityStartMessage", element).hide(),
		now = matematici.clock.getTime(),
		validityEnd = $("input#ValidityEnd", element).keyfilter(/[\d\/]/).datepicker({
			beforeShow: function (element) { return !$(element).prop("readonly"); },
			beforeShowDay: function (date) { return [date > now]; }
		}),
		validityEndMessage = $("div#ValidityEndMessage", element).hide(),
		error = $("div.FormError", element).hide(),

		onDateTypeChanged = function (e, dateInput, messagePanel) {
			var type = parseInt($(this).val()) || 0;
			switch (type) {
				case 0:
				case 1:
					dateInput.prop("readonly", true).hide();
					dateInput.prop("readonly", true).hide();
					messagePanel.show();
					$("span", messagePanel).hide().filter("[data-value='" + type + "']").show();
					break;
				case 2:
					dateInput.prop("readonly", false).show();
					messagePanel.hide();
					break;
			}
		},

		createModel = function (accepted) {
			return {
				Accepted: accepted,
				Prototype: options.Prototype,
				Version: options.Version,
				Action: options.Action,
				SignatureId: options.SignatureId,
				RowVersion: options.RowVersion,
				ValidityStartType: $("input.ValidityStartType:checked", element).val(),
				ValidityStart: validityStart.val(),
				ValidityEndType: $("input.ValidityEndType:checked", element).val(),
				ValidityEnd: validityEnd.val(),
				Comment: $("textarea#Comment", element).val(),
				Username: options.Code,
				Password: $("input#Password", element).val()
			};
		};

	element
        .on("click", "a#Confirm:not(.ReadOnly)", function () {
        	if (options.completeHandler)
        		options.completeHandler.call(self, createModel(true));
        })
        .on("click", "a#Reject:not(.ReadOnly)", function () {
        	if (options.completeHandler)
        		options.completeHandler.call(self, createModel(false));
        })
        .on("click", "a#Cancel", function () {
        	matematici.popup.close();
        })
		.on("change", "input.ValidityStartType", function (e) {
			onDateTypeChanged.call(this, e, validityStart, validityStartMessage);
		})
		.on("change", "input.ValidityEndType", function (e) {
			onDateTypeChanged.call(this, e, validityEnd, validityEndMessage);
		});

	onDateTypeChanged.call($("input.ValidityStartType:checked", element), null, validityStart, validityStartMessage);
	onDateTypeChanged.call($("input.ValidityEndType:checked", element), null, validityEnd, validityEndMessage);

	this.showError = function (value) {
		error.show().html(value);
		return this;
	}

	this.setConcurrencyException = function (value) {
		concurrencyException = value;
		$("a#Reject, a#Confirm", element).toggleClass("ReadOnly", value);

		return this;
	}
}