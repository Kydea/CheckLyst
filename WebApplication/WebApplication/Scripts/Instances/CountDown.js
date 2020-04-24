instances.CountDown = function (node, dateString, itemId) {
    var
        parent = $(node),
        countDown = this,
        element,
        interval,
        date = moment.parseZone(dateString),
        lastHours = null, //for debug

        update = function () {
            var
                serverTime = matematici.clock.getTime(),
                milliseconds = date - serverTime,
                seconds = parseInt(milliseconds / 1000 % 60),
                minutes = parseInt((milliseconds - seconds * 1000) / 1000 / 60 % 60),
                hours = parseInt((milliseconds - seconds * 1000 - minutes * 60000) / 1000 / 60 / 60);

            // \/ debug \/
            if (lastHours != hours)
                console.log({ serverTime: serverTime, date: date, milliseconds: milliseconds, seconds: seconds, minutes: minutes, hours: hours });
            lastHours = hours;
            // /\ debug /\
             
            if (milliseconds < -1000) { //after an extra delay of 1s
                new instances.Event(instances.Events.countDown, itemId).trigger();
                clear();
            } else {
                $("span.Interval", element).html("<i class='fa fa-long-arrow-down'></i>" + matematici.dateFormatter.both(serverTime) + "<br />" + matematici.dateFormatter.both(date));
				$("span.Time", element).text(milliseconds >= 0 ? (hours > 0 ? hours + ":" : "") + (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds) : "00:00");
            }
        },

        clear = function () {
            if (interval) clearInterval(interval);
            element.remove();
			parent.data("CountDown", null).removeClass("CountDownOn");
        },

        setup = function () {
            var oldCountDown = parent.removeClass("CountDownOn").data("CountDown");
			if (oldCountDown)  oldCountDown.clear();

			if (date > matematici.clock.getTime()) {
				parent.data("CountDown", countDown).addClass("CountDownOn");
				element = $("<div />").addClass("CountDown").appendTo(parent)
					.append($("<span />").addClass("Interval"))
					.append($("<span />").addClass("Time"));
				var prototypeState = instances.instance.getVersionState();
                if (prototypeState == prototypes.Prototype.states.draft || prototypeState == prototypes.Prototype.states.simulation)
					element.append($("<a />").addClass("Button Icon").attr("href", "#").append($("<i />").attr("title", "Skip").addClass("Icon fa fa-caret-right")).on("click", function (e) {
                        e.preventDefault();
                        new instances.Event(instances.Events.skipCountDown, itemId).trigger();
                        clear();
                    }));

                update();
                interval = setInterval(update, 1000);
            }
        };

    this.clear = function () {
        clear();
    };

    setup();
};