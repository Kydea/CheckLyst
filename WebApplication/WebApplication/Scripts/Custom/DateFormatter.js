matematici.dateFormatter = {
	format: function (dateTime, local) {
		var
			momentDate = moment(dateTime).tz(local && localStorage.timeZone || 'UTC'),
			valid = momentDate.isValid(),

			date = function () {
				return valid ? momentDate.format(matematici.dateFormatter.dateFormat) : null;
			},

			time = function () {
				return valid ? momentDate.format(matematici.dateFormatter.timeFormat) : null;
			},

			offset = function () {
				var offset = valid && local ? momentDate.utcOffset() / 60 : 0;
				return valid && local ? "UTC " + (offset > 0 ? "+" : "-") + (offset > 9 || offset < -9 ? "" : "0") + Math.abs(offset) + ":00" : "";
			};

		return {
			date: date(),
			time: time(),
			offset: offset(),
			valid: valid
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
	},

	dateFormat: "DD/MM/YYYY",

	timeFormat: "HH:mm"
}