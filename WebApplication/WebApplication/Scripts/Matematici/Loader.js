matematici.loader = (function () {

    var
        _once = false,
        _count = 0,
        _callback = null,
        _percentage = 0, _in = 0, _out = 0,
        _center = $("<div />").addClass("Center").hide(),
        _image = $("<div />").addClass("Image"),
        _background = $("<div />").addClass("Background").appendTo(_center).append(_image),
        _div = $("<div />").attr("id", "Loader").hide().appendTo($(document.body)).append(_center),
        _percentageLabel = $("<div />").addClass("Percentage").appendTo(_background),
        _cancelButton = $("<a />").addClass("CancelButton").attr({ href: "#", title: "cancel operation" }).appendTo(_background).append($("<i />").addClass("fa fa-times")),//.hide(),
        _cancelHandler,

        _show = function () {
            _div.show();
            _image.activity({ color: '#ffffff', segments: 14, length: 1, space: 8, speed: 1, steps: 15, opacity: 0 }).show();
            _center.show();
        },

        _setPercentage = function () {
            _percentage = Math.max(_percentage, (_out / _in * 100).toFixed());
            if (isNaN(_percentage) || _in < 3) _percentage = 0;
            _background.toggleClass("Percentage", _percentage > 0)
            _percentageLabel.text(_percentage === 0 ? "" : _percentage + "%");
        },

        _cancelButtonClick = function (e) {
            e.preventDefault();
            _cancelHandler && _cancelHandler();
        },

        _checkInitialCurtain = function () {
            if (_count <= 0 && !_once) {
                _once = true;
                $("div#InitialCurtain").fadeOut(2000, function () {
                    $(this).remove();
                });
                _cancelButton.on("click", _cancelButtonClick);
            }
        },

        _refreshComponents = function () {
            setTimeout(function () {
                $("select.AutoComplete").each(function (index) {
                    var autoComplete = matematici.AutoComplete.get(this);
                    if (autoComplete) autoComplete.refresh();
                });

                $(".TabContainer").each(function (index) {
                    $(this).data("tab").resize();
                });

                $(".AutoCenter").each(function () {
                    $(this).data("autoCenter").center();
                });
            }, 0);
            return;
        },

        _checkCurtain = function () {
            if (_count <= 0) {
                $("div#InitialCurtain").fadeOut(2000);
                matematici.elements.content.stop(true).fadeTo(500, 1);
            }
        };

    return {

        getImages: function (paths, id) {
            var _loadedImages = {};
            if (paths.length > 0) {
                _count += paths.length;
                _in += paths.length;
                _show();

                //console.log(new Error("loader += " +  paths.length + ": " + _count));

                for (var index = 0; index < paths.length; ++index)
                    _loadedImages[id === undefined ? index : id[index]] = $("<img />")
                        .on("load", function () {
                            //console.log("image loaded");
                            matematici.loader.hide();
                        })
                        .on("error", function (e) {
                            console.log("error loading image", this, index, e);
                            matematici.loader.hide();
                        }).attr({ src: paths[index] });
            }
            return _loadedImages;
        },

        loadImages: function (paths, id) {
            this.getImages(paths, id);
            return matematici.loader;
        },

        loadContent: function (selector) {
            var paths = [];
            $("img.Preload", selector).each(function () {
                paths.push($(this).attr("src"));
            });
            this.getImages(paths);
            return matematici.loader;
        },

        show: function () {
            if (_count == 0) {
                _cancelHandler = null;
                //_cancelButton.hide();
            }
            ++_count;
            ++_in;
            $("body").addClass("LinksDisabled");
            _show();

            _setPercentage();

            //console.log(new Error("loader++: " + _count));
            return matematici.loader;
        },

        hide: function () {
            if (false && _count < 0)
                console.log("warning: hide have been called when loading was not showed");

            if (--_count <= 0) {

                _percentage = 0;
                _in = 0;
                _out = 0;

                $("body").removeClass("LinksDisabled");

                _image.activity(false);
                _center.hide();
                _div.hide();

                _checkInitialCurtain();
                _refreshComponents()

                if (_callback !== null) {
                    _callback.call();
                    _callback = null;
                }

                _cancelHandler = null;
            }

            ++_out;
            _setPercentage();

            //console.log(new Error("loader--: " + _count));
            return matematici.loader;
        },

        setCallback: function (callback) {
            _callback = callback;
            return matematici.loader;
        },

        getCount: function () {
            return _count;
        },

        isVisible: function () {
            return _count > 0;
        },

        checkInitialCurtain: function () {
            _checkInitialCurtain();
        },

        checkCurtain: function () {
            _checkCurtain();
        },

        setCancelHandler: function (value) {
            if (_count > 0) {
                _cancelHandler = value;
                _cancelButton.show();
            }
        }
    };
})();