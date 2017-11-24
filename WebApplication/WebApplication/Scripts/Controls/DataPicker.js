matematici.DatePicker = function (selector) {
	var
		datePicker = this,
		element = $(selector),
		date,
		enabled = !element.hasClass("Disabled") && !element.prop("readonly"),
		calendar = null,
		min,
		max,

		getCurrentDate = function () {
			var today = new Date();
			today.setHours(0, 0, 0, 0);
			return today;
		},

		createCalendar = function () {
			calendar = matematici.DatePicker.calendar = new matematici.Calendar();
		},

		open = function (e) {
			if (!enabled) return;

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

		setDate = function (value) {
			var formattedDate = matematici.dateFormatter.format(value, true);
			date = formattedDate.valid ? value : null;
			element.data("date", date);
			element.val(formattedDate.date);
		},

		close = function () {
			calendar.close();
			$(document).off("click.datePicker");
		},

		parse = function () {
			try {
				var value = element.val();

				date = !value ? null : moment(value, matematici.dateFormatter.dateFormat);
				element.val(date && date.format("L") || "");
			}
			catch (error) {
				matematici.dialog.error("Error", "invalid date");
				console.log(error, date, element.val());
				date = null;
				element.val("");
			}
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
				.on("change", parse)
				.on("click", open);

			$("<i />")
				.addClass("fi flaticon-calendar")
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

	this.setDate = function (value) {
		setDate(value);
	};

	this.getDate = function () {
		return date;
	};

	setup();
};
