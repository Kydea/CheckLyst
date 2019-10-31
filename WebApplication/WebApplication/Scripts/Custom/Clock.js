matematici.Clock = function () {
	//gestire weak = true con colore rosso e tooltip con errore
	var
		offset = 0,
		weak = true,
		error = null,
		elements = [],
		selectors = [],
		stopped,

        check = function () {
			return $.ajax({
				url: matematici.urls.infrastructure.clock,
                type: "post",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    localTime: new Date(),
                    timeZone: localStorage.timeZone
                })
			});
		},

		update = function () {
			check().done(function (result) {
                serverTime = moment.parseZone(result.serverTime);
                localTime = moment.parseZone(result.localTime);

                var
					milliseconds = (60 - serverTime.seconds() + 1) * 1000;

				weak = false;
				error = null;
				offset = serverTime.diff(new moment());

				//console.log(result);
				//console.log("Server time: ", serverTime);
				//console.log("Local time: ", localTime);
				//console.log("Client time: ", new moment());
				//console.log("offset: ", offset);

				setTimeout(function () {
					if (!stopped)
						update();
				}, Math.max(milliseconds || 0, 10000));

                for (var index = 0; index < selectors.length; ++index) {
                    var local = elements[index].hasClass("LocalTime");
                    elements[index].html(matematici.dateFormatter.both(local ? localTime : serverTime));
                }
			}).fail(function (jqXHR, textStatus, errorThrown) {
				weak = true;
				error = errorThrown;
				offset = 0;

				setTimeout(function () {
					if (!stopped)
						update();
				}, 60 * 1000);
			});
		},

		getTime = function () {
			var date = new Date();
			date.setTime(new Date().getTime() + offset);
			return date;
		};

	return {
		getTime: function () {
			return getTime();
		},

		stop: function () {
			stopped = true;
		},

		resume: function () {
			stopped = false;
			update();
		},

		add: function (selector) {
			selectors.push(selector);
			elements.push($(selector));
		},

		remove: function (selector) {
			for (var index = 0; index < selectors.length; ++index)
				if (selectors[index] == selector) {
					Array.remove(selectors, index);
					Array.remove(elements, index);
				}
		}
	};
};