matematici.dateFormatter = {
    both: function (dateTime, local) {
        return matematici.dateFormatter.date(dateTime, local) + " " + matematici.dateFormatter.time(dateTime, local) + " " + matematici.dateFormatter.offset(dateTime, local);
    },

    date: function (dateTime, local) {
        var
            year = local ? dateTime.getFullYear() : dateTime.getUTCFullYear(),
            month = 1 + (local ? dateTime.getMonth() : dateTime.getUTCMonth()),
            day = local ? dateTime.getDate() : dateTime.getUTCDate();
        return (day < 10 ? "0" : "") + day + "/" + (month < 10 ? "0" : "") + month + "/" + year;
    },

    time: function (dateTime, local) {
        var
            hours = local ? dateTime.getHours() : dateTime.getUTCHours(),
            minutes = local ? dateTime.getMinutes() : dateTime.getUTCMinutes();
        return (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
    },

    offset: function (dateTime, local) {
        var offset = dateTime.getTimezoneOffset() / 60;
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