/// <reference path="./Libraries/JQuery-1.8.1-vsdoc.js"/>

matematici.logo = (function () {
    var
        _sizes = [13, 10, 10, 10, 10, 10, 10, 10, 8, 10, 8],
        _frames = 3,
        _verse = 0,
        _position = 2,
        _frame = 0,
        _letters = null,
        _div = null,

        _setPosition = function () {
            var offset = 0;

            for (var index = 10; index >= 0; index--) {
                _letters.eq(index).css("right", offset + "px");
                offset += index == _position ? _sizes[index] / _frames * _frame : index > _position ? 0 : _sizes[index];
            }

            _div.css("width", offset + "px");
        };

    return {

        init: function () {

            _div = $("div.Matematici");
            _letters = $("div.Matematici span");

            _div.hover(function (e) {
                matematici.logo.open();
            }, function (e) {
                matematici.logo.close();
            });

            _setPosition();

        },

        open: function (time) {
            _verse = 1;
            matematici.AnimationManager.add(matematici.logo);
        },

        close: function (time) {
            _verse = -1;
            matematici.AnimationManager.add(matematici.logo);
        },

        frame: function () {
            if (_verse == 1) {
                if (_frame < _frames - 1)
                    ++_frame;
                else if (_position == 10)
                    _verse = 0;
                else {
                    _frame = 0;
                    ++_position;
                }
            } else if (_verse == -1) {
                if (_frame > 0)
                    --_frame;
                else if (_position == 2)
                    _verse = 0;
                else {
                    _frame = _frames - 1;
                    --_position;
                }
            } else
                matematici.AnimationManager.remove(matematici.logo);

            _setPosition();
        },

        getDiv: function () {
            return _div;
        }
    }
})();