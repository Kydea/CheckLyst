/// needs: Extensions.js

matematici.AnimationManager = function () {

    var
        _animations = [],
        _lastTime = 0,
        _vendors = ['ms', 'moz', 'webkit', 'o'],
        _enabled = true,

		_fpsCount = 0, _lastFpsCount,

        _frame = function (time) {

            /*
            var d = new Date(time);

        	if (_lastFpsCount != undefined && d.getHours() == _lastFpsCount.getHours() && d.getMinutes() == _lastFpsCount.getMinutes() && d.getSeconds() != _lastFpsCount.getSeconds()) {
        		console.log('fps', _fpsCount, d, time);
        		_fpsCount = 0;
        	}

        	_fpsCount++;
        	_lastFpsCount = d;
            */

        	for (var index in _animations)
                _animations[index].frame(time);
            if (_enabled) requestAnimationFrame(_frame);
        };

    for (var index = 0; index < _vendors.length && !window.requestAnimationFrame; ++index)
        window.requestAnimationFrame = window[_vendors[index] + 'RequestAnimationFrame'];

    if (!window.requestAnimationFrame)
        window.requestAnimationFrame = function (callback) {
            var currentTime = new Date().getTime();
            var timeToCall = Math.max(0, 16 - (currentTime - _lastTime));
            
            var id = window.setTimeout(callback, timeToCall, currentTime + timeToCall);
            _lastTime = currentTime + timeToCall;
            return id;
        };

    requestAnimationFrame(_frame);

    return {
        add: function (animation) {
            for (var index = 0; index < _animations.length; ++index)
                if (_animations[index] == animation)
                    return;
            _animations.push(animation);
        },

        remove: function (animation) {
            for (var index = 0; index < _animations.length; ++index)
                if (_animations[index] == animation)
                    Array.remove(_animations, index);
            return false;
        },

        toggle: function (value) {
            if (value == _enabled) return;

            _enabled = value;
            if (_enabled) requestAnimationFrame(_frame);
        }
    }
}();