matematici.DatePicker = function (selector) {
	var
		datePicker = this,
		element = $(selector),
		date,
		calendar = null,
		min,
		max,
		needToParse = false,

		isEnabled = function () {
			return !element.hasClass("Disabled") && !element.prop("readonly");
		},

		getCurrentDate = function () {
			var today = new Date();
			today.setHours(0, 0, 0, 0);
			return today;
		},

		createCalendar = function () {
			calendar = matematici.DatePicker.calendar = new matematici.Calendar();
		},

		open = function (e) {
			if (!isEnabled()) return;

			e.preventDefault();
			e.stopPropagation();

			element.focus().select();

			if (calendar == null)
				createCalendar();
			else if (calendar.isVisible() && calendar.getDatePicker() == datePicker)
				return;

			calendar.setDates(null);
			calendar.setRange(min, max);
			calendar.setDatePicker(datePicker);
			var offset = element.offset();
			calendar.open(offset.top + element.outerHeight() + 5, offset.left);

			setTimeout(function () {
				$(document).on("click.datePicker", close);
			}, 0);
		},

		setDate = function (value, local) {
			var formattedDate = matematici.dateFormatter.format(value, !!local);
			date = formattedDate.valid ? moment(value).toDate() : null;
			element
				.data("date", date)
				.val(formattedDate.date)
				.change();
		},

		close = function () {
			calendar.close();
			$(document).off("click.datePicker");
		},

		parse = function () {
			try {
				var value = element.val();
				if (!!value) {
					var momentDate = moment(value, matematici.dateFormatter.dateFormat);
					date = momentDate.isValid() ? momentDate.toDate() : null;
					element.val(momentDate.isValid() ? momentDate.format(matematici.dateFormatter.dateFormat) : "");
				} else {
					date = null;
					element.val("");
				}
				calendar.close();
			}
			catch (error) {
				console.log(error, date, element.val());
				date = null;
				element.val("");
			}
			element.data("date", date)
		},

		setup = function () {
			if (!matematici.DatePicker.calendar) createCalendar();
			calendar = matematici.DatePicker.calendar

			var
				minAttribute = element.data("min"),
				maxAttribute = element.data("max");
			if (minAttribute) min = new Date(minAttribute);
			if (maxAttribute) max = new Date(maxAttribute);

			element
				.data("datePicker", datePicker)
				.on("change", function () {
					if (needToParse) parse();
				})
				.on("focus", function () { needToParse = true; })
				.on("blur", function () { needToParse = false; })
				.on("click", open);

			$("<i />")
				.addClass("Icon fi flaticon-calendar")
				.insertBefore(element)
				.on("click", function (e) {
					e.preventDefault();
					if (calendar.isVisible() && calendar.getDatePicker() == datePicker) close();
					else open(e);
				});

			matematici.elements.window.on("resize", function () {
				close();
			});

			parse();
		};

	this.setDate = function (value, local) {
		setDate(value, local);
	};

	this.getDate = function () {
		return date;
	};

	setup();
};
