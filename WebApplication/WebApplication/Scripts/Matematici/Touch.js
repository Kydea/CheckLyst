matematici.touch = function () {
    var enabled = false;
    return {
        init: function () {
            $(".HiddenWithTouch").hide();
            $(".VisibleWithTouch").show();
            enabled = true;
        },

        isEnabled: function () {
            return enabled;
        }
    }
}();