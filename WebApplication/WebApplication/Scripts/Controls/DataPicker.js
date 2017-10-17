/// <reference path="../_references.js" />

matematici.DatePicker = function (selector) {
	var
		datePicker = this,
		element = $(selector).data("_datepicker", datePicker),
		input = $("input", element),
		date,
		enabled = true,
		dates = null,
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

			input.focus().select();

			if (calendar == null)
				createCalendar();
			else if (calendar.isVisible() && calendar.getDatePicker() == datePicker)
				return;

			calendar.setDates(dates);
			calendar.setRange(min, max);
			calendar.setDatePicker(datePicker);
			var offset = input.offset();
			calendar.open(offset.top + input.outerHeight() + 5, offset.left);

			setTimeout(function () {
				$(document).on("click.datePicker", close);
			}, 0);
		},

		close = function () {
			calendar.close();
			$(document).off("click.datePicker");
		},

		parse = function () {
			try {
				var value = input.val();

				date = !value ? null : $.datepicker.parseDate("dd/mm/yy", value);
				if (date == null)
					input.val("");
				else
					input.val(date.format("dd/MM/yyyy"));
			}
			catch (error) {
				matematici.dialog.error("Errore", "data non valida, usare il formato gg/mm/aaaa");
				console.log(error, date, input.val());
				date = null;
				input.val("");
			}
		},

		setup = function () {
			if (!matematici.DatePicker.calendar) createCalendar();
			calendar = matematici.DatePicker.calendar

			var datesContainer = element.children("span.Dates");
			if (datesContainer.size() == 1) {
				dates = [];
				datesContainer.children().each(function () {
					var
						span = $(this),
						date = new Date(parseInt(span.attr("data-year")), parseInt(span.attr("data-month")) - 1, parseInt(span.attr("data-day")));
					dates.push(date);
					if (!min || min > date) min = date;
					if (!max || max < date) max = date;
				});
				datesContainer.remove();
			}

			var limitsContainer = element.children("span.Limits");
			if (limitsContainer.size() == 1) {
				span = limitsContainer.children().eq(0);
				min = new Date(parseInt(span.attr("data-year")), parseInt(span.attr("data-month")) - 1, parseInt(span.attr("data-day")));
				span = limitsContainer.children().eq(1);
				max = new Date(parseInt(span.attr("data-year")), parseInt(span.attr("data-month")) - 1, parseInt(span.attr("data-day")));
				limitsContainer.remove();
			}

			input
				.on("change", function (e) {
					parse();
				})
				.on("click", function (e) {
					open(e);
				});

			element
				.on("click", "a", function (e) {
					e.preventDefault();
					if (calendar.isVisible() && calendar.getDatePicker() == datePicker) close();
					else open(e);
				});

			matematici.elements.window.on("resize", function () {
				close();
			});

			parse();
		};

	this.set = function (value) {
		input.val(matematici.dateFormatter.date(value, false)).change();
	};

	this.getDate = function () {
		return date;
	};

	setup();
};
