matematici.WorkFlowSignature = function (options) {
	var
		workFlowSignature = this,

		element,

		validityStartType,
		validityStart,
		validityStartMessage,

		validityEndType,
		validityEnd,
		validityEndMessage,

		validityStartChanged = function () {
			var value = validityStartType.filter(":checked").val();
			validityStartMessage.toggle(value != "2");
			validityStart.toggle(value == "2").prev().toggle(value == "2");
			validityStart.change();
		},

		validityEndChanged = function () {
			var value = validityEndType.filter(":checked").val();
			$("span", validityEndMessage.toggle(value != "2")).hide().filter("[data-value='" + value + "']").show();
			validityEnd.toggle(value == "2").prev().toggle(value == "2");
			validityEnd.change();
		},

		getModel = function (approved) {
			return {
				approved: approved,
				applicationStep: options.applicationStep,
				validFromNow: options.model ? validityStartType.filter(":checked").val() == "1" : false,
				validFrom: options.model && validityStartType.filter(":checked").val() == "2" ? validityStart.data("date") && validityStart.data("date").toJSON() : null,
				validUntilNow: options.model ? validityEndType.filter(":checked").val() == "1" : false,
				validUntil: options.model && validityEndType.filter(":checked").val() == "2" ? validityEnd.data("date") && validityEnd.data("date").toJSON() : null,
				comment: $("textarea#Comment", element).val(),
				password: $("input#Password", element).val(),
				modifiedBy: options.concurrencyInformation.modifiedBy,
				modifiedOn: options.concurrencyInformation.modifiedOn
			};
		},

		completeHandler = function (approved) {
			var
				model = getModel(approved),
				error = false;

			$("label.Error input, label.Error textarea", element).each(function () {
				matematici.controls.setValidationMessages($(this), []);
			});

			if (options.model) {
				if (!model.validFromNow && !validityStart.data("date")) {
					matematici.controls.setValidationMessages(validityStart, [{ message: "Specify a date" }])
					error = true;
				}
				if (validityEndType.filter(":checked").val() == "2" && !validityEnd.data("date")) {
					matematici.controls.setValidationMessages(validityEnd, [{ message: "Specify a date" }])
					error = true;
				}
				if (!error && !model.validFromNow && validityEndType.filter(":checked").val() == "2" && validityEnd.data("date") <= validityStart.data("date")) {
					matematici.controls.setValidationMessages(validityEnd, [{ message: "Validity end is before validity start" }])
					error = true;
				}
			}

			if (!model.comment && !model.approved) {
				matematici.controls.setValidationMessages($("textarea#Comment", element), [{ message: "The comment is mandatory for the rejections" }])
				error = true;
			}
			if (!model.password) {
				matematici.controls.setValidationMessages($("input#Password", element), [{ message: "The password is mandatory" }])
				error = true;
			}
			if (error) return;

			$.ajax({
				url: matematici.urls.api + "/WorkFlows/Applications/Sign",
				type: "post",
				data: model
			}).done(options.done);
		},

		setup = function () {
			matematici.popup.show({
				cssClass: "Signature",
				width: "500px",
				title: options.title,
				ajax: {
					url: matematici.urls.workflows.signature,
					type: "POST",
					callback: function () {
						console.log(options);
						$("#Reason", element).val(options.reason);
						$("#Username", element).val(matematici.tokenManager.getUserCode());

						var firstSignature = true;
						for (var row in options.rows)
							if (options.rows[row].approved != null)
								firstSignature = false;

						validityStart = $("input#ValidityStart", element);
						validityStartMessage = $("div#ValidityStartMessage", element);
						validityEnd = $("input#ValidityEnd", element);
						validityEndMessage = $("div#ValidityEndMessage", element);

						if (options.model) {
							validityStartType = $("input.ValidityStartType", element);
							new matematici.DatePicker(validityStart);

							validityStartType.eq(options.model.validFrom ? 1 : 0).prop("checked", true);
							if (options.model.validFrom)
								validityStart.data("datePicker").setDate(options.model.validFrom);
							if (options.model.state != prototypes.Prototype.states.approval) {
								matematici.controls.setEditable(validityStart, false);
								validityStartType.prop("disabled", true);
							}

							validityEndType = $("input.ValidityEndType", element);
							new matematici.DatePicker(validityEnd);

							validityEndType.eq(options.model.state != prototypes.Prototype.states.approval ? 1 : options.model.validUntil ? 2 : 0).prop("checked", true);
							if (options.model.validUntil)
								validityEnd.data("datePicker").setDate(options.model.validUntil);
							if (options.model.state == prototypes.Prototype.states.approval)
								validityEndType.eq(1).parent().hide();

							if (firstSignature)
								element = $(matematici.popup.getElement())
									.on("change", "input.ValidityStartType", validityStartChanged)
									.on("change", "input.ValidityEndType", validityEndChanged);
							else {
								matematici.controls.setEditable(validityStartType, false);
								matematici.controls.setEditable(validityStart, false);
								matematici.controls.setEditable(validityEndType, false);
								matematici.controls.setEditable(validityEnd, false);
							}

							validityStartChanged();
							validityEndChanged();
						} else {
							validityStart.parent().hide();
							validityStartMessage.hide();
							validityEnd.parent().hide();
							validityEndMessage.hide();
						}
					}
				},
				buttons: [
					{ id: "Confirm", icon: "check", text: "Confirm", handler: function () { completeHandler(true); return true; } },
					{ id: "Reject", icon: "trash-o", text: "Reject", handler: function () { completeHandler(false); return true;} },
					{ hidden: true, cancelButton: true }
				]
			});
		};

	setup();
}