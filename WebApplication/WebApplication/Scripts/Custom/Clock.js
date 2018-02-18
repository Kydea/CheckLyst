matematici.Clock = function (_stopped) {
	//gestire weak = true con colore rosso e tooltip con errore
	var
		offset = 0,
		weak = true,
		error = null,
		elements = [],
		selectors = [],
		stopped = _stopped,

		refresh = function () {
			var
				serverTime = getTime(),
				localTime = new Date();

			for (var index = 0; index < selectors.length; ++index) {
				var local = elements[index].hasClass("LocalTime");
				elements[index].html(matematici.dateFormatter.both(local ? localTime : serverTime, local));
			}
		},

		check = function () {
			return $.ajax({
				url: matematici.urls.infrastructure.clock,
				type: "post"
			});
		},

		logout = function (redirect) {
			matematici.dialog.close();
			matematici.popup.close();
			matematici.canClose = true;
			matematici.navigator.open(redirect, false, true);
		},

		update = function () {
			check().done(function (result) {
				var
					serverTime = new Date(result.serverTime),
					milliseconds = (60 - serverTime.getSeconds() + 1) * 1000;

				weak = false;
				error = null;
				offset = serverTime.getTime() - new Date().getTime();

				console.log(result);
				console.log("Server time: " + serverTime);
				console.log("Client time: " + new Date());
				console.log("offset: " + offset);

				setTimeout(function () {
					if (!stopped)
						update();
				}, Math.max(milliseconds || 0, 10000));

				refresh();
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

	//if(!stopped) update();

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
			refresh();
		},

		refresh: function () {
			refresh();
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