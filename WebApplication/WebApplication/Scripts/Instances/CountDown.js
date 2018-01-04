instances.CountDown = function (parent, dateString, itemId) {
    var
        countDown = this,
        element,
        interval,
        date = new Date(dateString),

        update = function () {
            var
                serverTime = matematici.clock.getTime(),
                milliseconds = date - serverTime,
                seconds = parseInt(milliseconds / 1000 % 60),
                minutes = parseInt((milliseconds - seconds * 1000) / 60 / 1000 % 60),
                hours = parseInt((milliseconds - seconds * 1000 - minutes * 60000) / 60 / 60 / 1000 % 60);

            //console.log(milliseconds, seconds, minutes, hours, date, matematici.clock.getTime());

            if (milliseconds < -1000) { //after an extra delay of 1s
                new instances.Event(instances.Events.countDown, itemId).trigger();
                clear();
            } else {
                $("span.Time", element).text(milliseconds >= 0 ? (hours > 0 ? hours + ":" : "") + (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds) : "00:00");
                $("span.Interval", element).html("<i class='fa fa-long-arrow-down'></i>" + matematici.dateFormatter.both(serverTime, true) + "<br />" + matematici.dateFormatter.both(date, true));
            }
        },

        clear = function () {
            if (interval) clearInterval(interval);
            element.remove();
            parent.data("CountDown", null);
        },

        setup = function () {
            var oldCountDown = parent.data("CountDown");
            if (oldCountDown) oldCountDown.clear();

            if (date > matematici.clock.getTime()) {
                parent.data("CountDown", countDown);
                element = $("<div />").addClass("CountDown").appendTo(parent)
                    .append($("<span />").addClass("Time"))
                    .append($("<span />").addClass("Interval"));
				var prototypeState = instances.instance.getVersionState();
				console.log(prototypeState);
                if (prototypeState == prototypes.Prototype.states.draft || prototypeState == prototypes.Prototype.states.simulation)
					element.append($("<a />").addClass("Button Icon").attr("href", "#").append($("<i />").addClass("fi flaticon-arrow-right")).on("click", function (e) {
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