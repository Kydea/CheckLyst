matematici.Clock = function () {
    //gestire weak = true con colore rosso e tooltip con errore
    var
        error = null,
        elements = [],
        selectors = [],
        stopped,
        utcTime,
        localTime,
        clientTime,
        checkButton,
        timeout,

        toggleWarning = function (value) {
            if (value) {
                if (!checkButton) {
                    checkButton = document.createElement("a");
                    checkButton.classList.add("NoConnectionWarning");
                    checkButton.setAttribute("title", "Connection problems detected. Click to try again.");
                    checkButton.addEventListener("click", function (e) {
                        e.preventDefault();
                        update();
                    })
                    var i = document.createElement("i");
                    i.classList.add("fa", "fas", "fa-wifi-slash");
                    checkButton.appendChild(i);
                }
                document.body.appendChild(checkButton)
            } else if (checkButton) {
                checkButton.remove();
                checkButton = null;
            }
        },

        check = function () {
            return $.ajax({
                url: matematici.urls.infrastructure.clock,
                type: "post",
                contentType: "application/json; charset=utf-8",
                suppressErrors: true
            });
        },

        updateNotifications = function () {
            $.ajax({
                url: matematici.urls.api + "/Instances/List/CountTasks",
                type: "post",
                contentType: "application/json; charset=utf-8",
                suppressErrors: true
            }).done(function (result) { matematici.menu.setNotificationBadge("/Dashboards/Tasks", result); });
            $.ajax({
                url: matematici.urls.api + "/Reports/Run/Count",
                type: "post",
                contentType: "application/json; charset=utf-8",
                suppressErrors: true
            }).done(function (result) { matematici.menu.setNotificationBadge("/Reports/Runs", result); });
        },

        update = function () {
            if (timeout) {
                clearTimeout(timeout);
                timeout = null;
            }

            check().done(function (result) {
                utcTime = moment.utc(result.serverTime);
                localTime = moment.parseZone(result.serverTime);
                clientTime = new moment();

                var
                    milliseconds = (60 - utcTime.seconds() + 1) * 1000;

                toggleWarning(false);
                error = null;

                timeout = setTimeout(function () {
                    if (!stopped)
                        update();
                }, Math.max(milliseconds || 0, 10000));

                for (var index = 0; index < selectors.length; ++index) {
                    var local = elements[index].hasClass("LocalTime");
                    elements[index].html(matematici.dateFormatter.both(local ? localTime : utcTime));
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                toggleWarning(true);
                error = errorThrown;
                setTimeout(function () {
                    if (!stopped)
                        update();
                }, 60 * 1000);
            });

            updateNotifications();
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
            update();
        },

        add: function (selector) {
            selectors.push(selector);
            elements.push($(selector));
        },

        log: function () {
            console.log("UTC time: ", utcTime.toString());
            console.log("Local time: ", localTime.toString());
            console.log("Client time: ", clientTime.toString());
            console.log("Estimated time: ", matematici.clock.getTime().toString());
        },

        remove: function (selector) {
            for (var index = 0; index < selectors.length; ++index)
                if (selectors[index] == selector) {
                    Array.remove(selectors, index);
                    Array.remove(elements, index);
                }
        },

        update: update,

        updateNotifications: updateNotifications
    };
};