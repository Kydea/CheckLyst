matematici.dateFormatter = {
    both: function (dateTime, local) {
        return matematici.dateFormatter.date(dateTime, local) + " " + matematici.dateFormatter.time(dateTime, local) + " " + matematici.dateFormatter.offset(dateTime, local);
    },

    date: function (dateTime, local) {
		var
			momentDate = local ? moment(dateTime).tz(localStorage.timeZone) : null, 
			year = local ? momentDate.year() : dateTime.getUTCFullYear(),
			month = 1 + (local ? momentDate.month() : dateTime.getUTCMonth()),
			day = local ? momentDate.date() : dateTime.getUTCDate();
        return (day < 10 ? "0" : "") + day + "/" + (month < 10 ? "0" : "") + month + "/" + year;
    },

    time: function (dateTime, local) {
        var
			momentDate = local ? moment(dateTime).tz(localStorage.timeZone) : null, 
			hours = local ? momentDate.hours() : dateTime.getUTCHours(),
			minutes = local ? momentDate.minutes() : dateTime.getUTCMinutes();
        return (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
    },

    offset: function (dateTime, local) {
		var offset = moment(dateTime).tz(localStorage.timeZone).utcOffset() / 60;
        return local ? "UTC " + (offset > 0 ? "-" : "+") + (offset > 9 || offset < -9 ? "" : "0") + Math.abs(offset) + ":00" : "";
	},

	format: function (dateTime, local) {
		return {
			date: matematici.dateFormatter.date(dateTime, local),
			time: matematici.dateFormatter.time(dateTime, local),
			offset: matematici.dateFormatter.offset(dateTime, local)
		}
	}
}