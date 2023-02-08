matematici.Clock = function () {
	//gestire weak = true con colore rosso e tooltip con errore
	var
		weak = true,
		error = null,
		elements = [],
		selectors = [],
        stopped,
        utcTime,
        localTime,
        clientTime,

        check = function () {
			return $.ajax({
				url: matematici.urls.infrastructure.clock,
                type: "post",
                contentType: "application/json; charset=utf-8",
			});
		},

		update = function () {
			check().done(function (result) {
                utcTime = moment.utc(result.serverTime);
                localTime = moment.parseZone(result.serverTime);
                clientTime = new moment();

                var
					milliseconds = (60 - utcTime.seconds() + 1) * 1000;

				weak = false;
				error = null;

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
                    elements[index].html(matematici.dateFormatter.both(local ? localTime : utcTime));
                }
			}).fail(function (jqXHR, textStatus, errorThrown) {
				weak = true;
				error = errorThrown;
                clientTime = new moment();

				setTimeout(function () {
					if (!stopped)
						update();
				}, 60 * 1000);
			});
		},

        getTime = function () {
            var momentDate = localTime.clone().add(new moment().diff(clientTime), "milliseconds");
            return momentDate;
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
			//update();
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