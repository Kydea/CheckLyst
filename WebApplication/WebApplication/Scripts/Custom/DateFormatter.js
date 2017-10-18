matematici.dateFormatter = {
	format: function (dateTime, local) {
		var
			momentDate = !local ? null : localStorage.timeZone ? moment(dateTime).tz(localStorage.timeZone) : moment(dateTime),

			date = function () {
				var
					year = local ? momentDate.year() : dateTime.getUTCFullYear(),
					month = 1 + (local ? momentDate.month() : dateTime.getUTCMonth()),
					day = local ? momentDate.date() : dateTime.getUTCDate();
				return (day < 10 ? "0" : "") + day + "/" + (month < 10 ? "0" : "") + month + "/" + year;
			},

			time = function () {
				var
					hours = local ? momentDate.hours() : dateTime.getUTCHours(),
					minutes = local ? momentDate.minutes() : dateTime.getUTCMinutes();
				return (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
			},

			offset = function () {
				var offset = local ? momentDate.utcOffset() / 60 : 0;
				return local ? "UTC " + (offset > 0 ? "+" : "-") + (offset > 9 || offset < -9 ? "" : "0") + Math.abs(offset) + ":00" : "";
			};

		return {
			date: date(),
			time: time(),
			offset: offset()
		}
	},

	date: function (dateTime, local) {
		return matematici.dateFormatter.format(dateTime, local).date;
	},

	time: function (dateTime, local) {
		return matematici.dateFormatter.format(dateTime, local).time;
	},

	both: function (dateTime, local) {
		var format = matematici.dateFormatter.format(dateTime, local);
		return format.date + " " + format.time + (local ? " " + format.offset : "");
	}
}