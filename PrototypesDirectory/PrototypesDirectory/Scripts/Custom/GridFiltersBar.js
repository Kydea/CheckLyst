matematici.GridFiltersBar = function (parent, handler) {
    var
        gridFiltersBar = this,
        container,
        rightDiv,
        input,
        icon,
        flags = [],
        keyword,
        timeout,
        keywordPresent = false,
        flagPresent = false,

        toggleIcon = function () {
            icon.toggleClass("fa", keywordPresent).addClass("fal", !keywordPresent);
        },

        callHandler = function () {
            handler(keywordPresent || flagPresent, keyword, flags.map(function (input) { return input.prop("checked"); }));
        },
        
        filter = function (value) {
            if (timeout) clearTimeout(timeout);
            timeout = null;
            if (input.val() == "") value = false;
            keywordPresent = value;
            toggleIcon();
            matematici.loader.show();
            keyword = value && input.val() != "" ? input.val() : null;
            callHandler();
            setTimeout(matematici.loader.hide, 500);
        },

        delayedFilter = function () {
            keywordPresent = true;
            toggleIcon();
            if (timeout) clearTimeout(timeout);
            timeout = setTimeout(function () {
                timeout = null;
                filter(true);
            }, 1000);
        },

        addFlag = function (label, value, alignToRight) {
            var flag = $("<input />").attr({ type: "checkbox" }).prop("checked", value).on("click", function () {
                setTimeout(function () {
                    flagPresent = false;
                    for (var index = 0; !flagPresent && index < flags.length; ++index)
                        if (!flags[index].prop("checked"))
                            flagPresent = true;
                    callHandler();
                }, 0);
            });
            if (flags.length == 0) container.append($("<span />").text("filter for:"));
            (alignToRight ? rightDiv : container).append($("<label />").append(label).prepend(flag));
            flags.push(flag);
        },

        setup = function () {
            parent.append(container = $("<div />").addClass("GridFiltersBar"));
            container.append(rightDiv = $("<div />").addClass("Right"));
            container.append($("<a />").addClass("FilterIcon").attr("href", "#").append(icon = $("<i />").addClass("fal fa-filter")).on("click", function (e) {
                e.preventDefault();
                filter(!keywordPresent);
            }));
            container.append(input = $("<input />").addClass("Keyword").attr({ type: "text" }).on("paste keyup", function () {
                delayedFilter();
            }));
        };

    gridFiltersBar.isFiltering = function () {
        return keywordPresent || flagPresent;
    };

    gridFiltersBar.addFlag = addFlag;

    setup();
}