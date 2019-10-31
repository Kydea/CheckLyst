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

    parse: function (yearOrDate, monthOrTime, day, local) {
        var m = 
            day != null ? moment.tz(yearOrDate + "-" + monthOrTime + "-" + day, matematici.dateFormatter.dateFormat, local && localStorage.timeZone || 'UTC') : //year, month, day
                monthOrTime != null ? moment.tz(yearOrDate + " " + monthOrTime, matematici.dateFormatter.dateFormat + " " + matematici.dateFormatter.timeFormat, local && localStorage.timeZone || 'UTC') : //date, time
                    moment.tz(yearOrDate, matematici.dateFormatter.dateFormat, local && localStorage.timeZone || 'UTC'); //date
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