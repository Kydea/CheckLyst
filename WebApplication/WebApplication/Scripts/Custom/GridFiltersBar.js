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
        filtering = false,

        toggleIcon = function () {
            icon.toggleClass("fa", filtering).addClass("fal", !filtering);
        },

        callHandler = function () {
            handler(filtering, keyword, flags.map(function (input) { return input.prop("checked"); }));
        },
        
        filter = function (value) {
            if (timeout) clearTimeout(timeout);
            timeout = null;
            if (input.val() == "") value = false;
            filtering = value;
            toggleIcon();
            matematici.loader.show();
            keyword = value && input.val() != "" ? input.val() : null;
            callHandler();
            setTimeout(matematici.loader.hide, 500);
        },

        delayedFilter = function () {
            filtering = true;
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
                filter(!filtering);
            }));
            container.append(input = $("<input />").addClass("Keyword").attr({ type: "text" }).on("paste keyup", function () {
                delayedFilter();
            }));
        };

    gridFiltersBar.isFiltering = function () {
        return filtering;
    };

    gridFiltersBar.addFlag = addFlag;

    setup();
}