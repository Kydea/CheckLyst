matematici.WorkFlowSignature = function (selector, options) {
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
				accepted: accepted,
				prototype: options.prototype,
				version: options.version,
				rowVersion: options.rowVersion,
				action: options.action,
				validityStartType: $("input.ValidityStartType:checked", element).val(),
				validityStart: validityStart.val(),
				validityEndType: $("input.ValidityEndType:checked", element).val(),
				validityEnd: validityEnd.val(),
				comment: $("textarea#Comment", element).val(),
				username: tokenManager.getUserCode(),
				password: $("input#Password", element).val()
			};
		},

		setup = function () {
			$("h3", element).text(options.action);
			$(".Username", element).val(options.code);

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

			matematici.popup.addCloseButton();
		};

	setup();
}