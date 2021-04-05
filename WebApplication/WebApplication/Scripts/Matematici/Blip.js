matematici.blip = function () {

    var
        element,
        container,
        callback,
        countdownInterval,

        show = function (options) {
            if (element) {
                element.removeClass().addClass("Blip").show();
                container.empty()
            } else {
                matematici.elements.body.addClass("Blip");
                element = $("<div />")
                    .addClass("Blip")
                    .appendTo(matematici.elements.body)
                    .on("click", close);
                container = $("<div />").addClass("Container").appendTo(element);
                element.fadeIn(300);
            }

            if (options.cssClass) element.addClass(options.cssClass)
            if (options.icon) container.append($("<i />").addClass("fal fa-" + options.icon));

            callback = options.callback;
            if (options.timeout) setTimeout(close, options.timeout + 300);

        },

        close = function () {
            if (element)
                element.fadeOut(300, "linear", function () {
                    element.remove();
                    matematici.elements.body.removeClass("Blip");
                    if (callback) callback();
                    callback = element = null;
                });
        };

    return {
        show: function (options) {
            show(options);
        },

        ok: function (callback) {
            show({
                icon: "check", callback: callback
            });
        },

        ko: function (callback) {
            show({
                cssClass: "Error", icon: "exclamation", callback: callback
            });
        },

        warning: function () {
            show({
                cssClass: "Warning", icon: "exclamation", timeout: 100
            });
        },

        countdown: function (milliseconds, callback) {
            var finalDate = new Date(new Date().getTime() + milliseconds);
            var
                time,
                clearCountdownInterval = function () {
                    if (countdownInterval) clearInterval(countdownInterval);
                    countdownInterval = null;
                },
                updateContent = function () {
                    var seconds = Math.floor((finalDate - new Date()) / 1000);
                    time.text(Math.max(seconds, 0));
                    if (seconds <= 0) {
                        clearCountdownInterval();
                        callback();
                    }
                };

            clearCountdownInterval();
            countdownInterval = setInterval(updateContent, 500);
            show({ cssClass: "Countdown", callback: clearCountdownInterval });
            container
                .append($("<span>").addClass("Label").text("automatic logout in"))
                .append(time = $("<span>").addClass("Time"))
                .append($("<span>").addClass("Label").text("seconds"));
            updateContent();
        },

        close: close
    };
}();