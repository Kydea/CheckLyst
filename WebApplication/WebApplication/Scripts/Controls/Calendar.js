matematici.Calendar = function (selector) {

    var
        calendar = this,
        dates = [],
        attributes = [],
        min,
        max,
        elements = {
            datePicker: null,
            calendar: null,
            month: null,
            previous: null,
            next: null,
            table: null
        },
        values = {
            year: null,
            month: null,
            date: null
        },

        element,
        clickHandler,
        popup = false,

        getCurrentDate = function () {
            var today = new Date();
            today.setUTCHours(0, 0, 0, 0);
            return today;
        },

        setup = function () {
            if (selector == null) {
				element = $("<div />").appendTo(matematici.elements.body).addClass("Calendar-Popup");
                popup = true;
            } else
                element = $(selector);

            element
                .addClass("Calendar")
                .append(elements.month = $("<span />").addClass("Month"))
				.append(elements.previous = $("<a />").addClass("Arrow PreviousMonth").attr("href", "#").append($("<i />").addClass("fa fa-caret-left")))
				.append(elements.next = $("<a />").addClass("Arrow NextMonth").attr("href", "#").append($("<i />").addClass("fa fa-caret-right")))
                .append(elements.table = $("<table />").addClass("Plain")
                    .append($("<tr />")
                        .append($("<th />").text(matematici.Calendar.translations.it.dayInitials[1]))
                        .append($("<th />").text(matematici.Calendar.translations.it.dayInitials[2]))
                        .append($("<th />").text(matematici.Calendar.translations.it.dayInitials[3]))
                        .append($("<th />").text(matematici.Calendar.translations.it.dayInitials[4]))
                        .append($("<th />").text(matematici.Calendar.translations.it.dayInitials[5]))
                        .append($("<th />").addClass("Holyday").text(matematici.Calendar.translations.it.dayInitials[6]))
                        .append($("<th />").addClass("Holyday").text(matematici.Calendar.translations.it.dayInitials[0]))
                    )
                    .append($("<tr />")
                        .append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />").addClass("Holyday").data("Holyday", true)).append($("<td />").addClass("Holyday").data("Holyday", true))
                    )
                    .append($("<tr />")
                        .append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />").addClass("Holyday").data("Holyday", true)).append($("<td />").addClass("Holyday").data("Holyday", true))
                    )
                    .append($("<tr />")
                        .append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />").addClass("Holyday").data("Holyday", true)).append($("<td />").addClass("Holyday").data("Holyday", true))
                    )
                    .append($("<tr />")
                        .append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />").addClass("Holyday").data("Holyday", true)).append($("<td />").addClass("Holyday").data("Holyday", true))
                    )
                    .append($("<tr />")
                        .append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />").addClass("Holyday").data("Holyday", true)).append($("<td />").addClass("Holyday").data("Holyday", true))
                    )
                    .append($("<tr />")
                        .append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />")).append($("<td />").addClass("Holyday").data("Holyday", true)).append($("<td />").addClass("Holyday").data("Holyday", true))
                    )
                )
                .on("click", "td.Active", function () {
                    var item = $(this);
					if (clickHandler) clickHandler(item);
					var value = matematici.dateFormatter.parse(values.year, values.month, item.text(), true).toDate();
					if (elements.datePicker) elements.datePicker.setDate(value, true);
                    if (popup) calendar.close();
                })
                .on("click", "a.PreviousMonth.Active", function () {
                    calendar.setMonth(values.month - 1);
                })
                .on("click", "a.NextMonth.Active", function () {
                    calendar.setMonth(values.month + 1);
                })
                .on("click", function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });

            if (!popup) calendar.setDate(getCurrentDate());
        }

    this.setMonth = function (month, year) {
        elements.table.stop(true).fadeTo(300, 0, function () {
            var today = getCurrentDate();
            year = year || values.year;
            if (month == 13) {
                month = 1;
                ++year;
            }
            if (month == 0) {
                month = 12;
                --year;
            }
            values.year = month == 13 ? year + 1 : month == 0 ? year - 1 : year;
            values.month = month == 13 ? 1 : month == 0 ? 12 : month;
			elements.month.text(matematici.Calendar.translations.it.months[values.month - 1] + (year == new Date().getUTCFullYear() ? "" : " " + year));
			elements.previous.toggleClass("Active", !min || year > min.getUTCFullYear() || year == min.getUTCFullYear() && month > min.getUTCMonth() + 1);
			elements.next.toggleClass("Active", !max || year < max.getUTCFullYear() || year == max.getUTCFullYear() && month < max.getUTCMonth() + 1);

			var
				date = new Date(Date.UTC(year, month - 1, 1)),
                rows = $("tr:not(:first-child)", elements.table).hide(),
                dayOfWeek = (date.getUTCDay() + 6) % 7, //0: monday .. 6: sunday
                datesIndex = 0;

            for (var rowIndex = 0; rowIndex <= rows.size() && date.getUTCMonth() + 1 == month; ++rowIndex) {
                row = rows.eq(rowIndex).show();
                $("td", row).removeClass();

                for (var columnIndex = 0; columnIndex < 7 ; ++columnIndex) {
                    var cell = row.children().eq(columnIndex).toggleClass("Today", date.getTime() == today.getTime());
					if (rowIndex == 0 && columnIndex < dayOfWeek || date.getUTCMonth() + 1 > month || date.getUTCFullYear() > year)
                        cell.empty().removeClass("Active");
                    else {
                        while (dates && datesIndex < dates.length && date > dates[datesIndex])
                            ++datesIndex;
                        var match = !dates || datesIndex < dates.length && date.getTime() == dates[datesIndex].getTime();
                        cell.text(date.getUTCDate()).toggleClass("Active", match).toggleClass("Holyday", cell.data("Holyday") == true);
                        if (match && attributes && attributes[datesIndex]) {
                            if (attributes[datesIndex].cssClass) cell.addClass(attributes[datesIndex].cssClass);
                            for (index in attributes[datesIndex].data)
                                cell.data(index, attributes[datesIndex].data[index]);
                        }
                        date = new Date(date.setDate(date.getDate() + 1));
                    }
                }
            }
            elements.table.fadeTo(300, 1);
        });
    }

	this.setDate = function (value) {
        values.date = value;
        calendar.setMonth(value.getUTCMonth() + 1, value.getUTCFullYear());
    }

    this.setDatePicker = function (value) {
		elements.datePicker = value;
		calendar.setDate(elements.datePicker.getDate() || getCurrentDate());
    };

    this.open = function (top, left) {
        element
            .css("top", top)
            .css("left", Math.max(10, Math.min(left, matematici.elements.window.width() - element.outerWidth() - 10)))
            .stop(true)
            .show();
    };

    this.setDates = function (value) {
        dates = value;
    };

    this.setAttributes = function (value) {
        attributes = value;
    };

    this.setClickHandler = function (value) {
        clickHandler = value;
    };

    this.setRange = function (a, b) {
        min = a;
        max = b;
    };

    this.getDatePicker = function (value) {
        return elements.datePicker;
    };

    this.close = function () {
        elements.table.stop(true).fadeTo(0, 0);
        element.hide();
    };

    this.isVisible = function () {
        return element.is(":visible");
    }

    setup();
};

matematici.Calendar.translations = {
    it: {
        months: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
        dayInitials: ["s", "m", "t", "w", "t", "f", "s"],
        days: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    }
}

