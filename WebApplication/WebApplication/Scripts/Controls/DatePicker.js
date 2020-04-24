matematici.DatePicker = function (node, min, max) {
    var
        datePicker = this,
        needToParse,

        isEnabled = function () {
            return !node.classList.contains("Disabled") && !node.hasAttribute("readonly");
        },

        open = function (e) {
            if (!isEnabled()) return;

            e.preventDefault();
            e.stopPropagation();

            matematici.elements.window.on("resize.datePicker", close);
            node.focus();
            node.select();

            if (matematici.calendar == null)
                matematici.calendar = new matematici.Calendar();
            else if (matematici.calendar.isVisible() && matematici.calendar.getDatePicker() == datePicker)
                return;

            matematici.calendar.setDates(null);
            matematici.calendar.setRange(min, max);
            matematici.calendar.setDatePicker(datePicker);

            var
                bodyOffset = document.body.getBoundingClientRect(),
                offset = node.getBoundingClientRect();
            matematici.calendar.open(offset.top - bodyOffset.top + node.offsetHeight + 5, offset.left - bodyOffset.left);

            setTimeout(function () {
                $(document).on("click.datePicker", close);
            }, 0);
        },

        close = function () {
            matematici.calendar.close();
            matematici.elements.window.off("resize.datePicker");
            $(document).off("click.datePicker");
        },

        parse = function () {
            try {
                var value = node.value;
                if (!!value) {
                    var momentDate = moment(value, matematici.dateFormatter.dateFormat);
                    node.value = momentDate.isValid() ? momentDate.format(matematici.dateFormatter.dateFormat) : "";
                } else
                    node.value = "";
            } catch (error) {
                console.log(error, node.value);
                node.value = "";
            }
            if (matematici.calendar) matematici.calendar.close();
        },

        initialize = function () {
            node.setAttribute("autocomplete", "off");
            node.addEventListener("change", function () {
                if (needToParse) parse();
            });
            node.addEventListener("focus", function () { needToParse = true; });
            node.addEventListener("blur", function () { needToParse = false; });
            node.addEventListener("click", open);

            var i = document.createElement("i");
            i.className = "Icon fal fa-calendar-alt";
            i.addEventListener("click", function (e) {
                e.preventDefault();
                if (matematici.calendar.isVisible() && matematici.calendar.getDatePicker() == datePicker)
                    close();
                else
                    open(e);
            });
            node.parentNode.insertBefore(i, node);

            matematici.DatePicker.map.set(node, datePicker);
            parse();
        };

    this.setValue = function (value) {
        node.value = value;
        parse();
        node.dispatchEvent(new Event('change', { 'bubbles': true }));
    };

    this.getValue = function () {
        return node.value;
    };

    initialize();
};

matematici.DatePicker.map = new WeakMap();
matematici.DatePicker.get = function (node) {
    return matematici.DatePicker.map.get(node);
};

matematici.destroyers.push(function (context) {
    matematici.DatePicker.map = new WeakMap();
    matematici.DatePicker.current = null;
});


