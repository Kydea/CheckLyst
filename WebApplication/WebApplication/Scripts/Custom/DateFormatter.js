matematici.dateFormatter = {
	format: function (dateTime) {
        var
            momentDate = dateTime, //moment(dateTime); //.tz(local && localStorage.timeZone || 'UTC'),
			valid = momentDate.isValid(),

			date = function () {
				return valid ? momentDate.format(matematici.dateFormatter.dateFormat) : null;
			},

			time = function () {
				return valid ? momentDate.format(matematici.dateFormatter.timeFormat) : null;
			},

            hours = function () {
                return valid ? momentDate.format("HH") : null;
            },

            minutes = function () {
                return valid ? momentDate.format("mm") : null;
            },

            seconds = function () {
                return valid ? momentDate.format("ss") : null;
            },

			offset = function () {
				var offset = valid ? momentDate.utcOffset() / 60 : 0;
				return valid ? "UTC" + (offset == 0 ? "" : offset >= 0 ? " +" : " -") + (offset > 9 || offset < -9 || offset == 0 ? "" : "0") + (offset == 0 ? "" : Math.abs(offset) + ":00") : "";
			};

		return {
			date: date(),
            time: time(),
            hours: hours(),
            minutes: minutes(),
            seconds: seconds(),
			offset: offset(),
			valid: valid
		}
	},

    parse: function (yearOrDate, monthOrTime, day) {
        var m = 
            day != null ? moment(yearOrDate + "-" + monthOrTime + "-" + day, matematici.dateFormatter.dateFormat) : //year, month, day
                monthOrTime != null ? moment(yearOrDate + " " + monthOrTime, matematici.dateFormatter.dateFormat + " " + matematici.dateFormatter.timeFormat) : //date, time
                    moment(yearOrDate, matematici.dateFormatter.dateFormat); //date
        return m;
	},

	date: function (dateTime, local) {
		return matematici.dateFormatter.format(dateTime, local).date;
	},

	time: function (dateTime, local) {
		return matematici.dateFormatter.format(dateTime, local).time;
	},

    seconds: function (dateTime, local) {
        return matematici.dateFormatter.format(dateTime, local).seconds;
    },

	both: function (dateTime, local) {
		var format = matematici.dateFormatter.format(dateTime, local);
		return format.date + " " + format.time + " " + format.offset;
	},

	dateFormat: "YYYY-MM-DD",

    timeFormat: "HH:mm"
}