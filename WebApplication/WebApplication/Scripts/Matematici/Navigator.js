matematici.navigator = (function () {

    var
        basePath = null,
        lastPath = null,
        fullSupport = window.history.pushState !== undefined,
        landing = true,
        suspended = false,
        callData = '';


    return {

        setBasePath: function (value) {
            basePath = value;
            return this;
        },

        getBasePath: function () {
            return basePath;
        },

        setLastPath: function (value) {
            lastPath = value;
            if (
                value
                && value.toLowerCase() != "/"
                && value.toLowerCase().indexOf("/?") != 0
                && value.toLowerCase().indexOf("/security/login") == -1
                && value.toLowerCase().indexOf("/error") == -1
                && value.toLowerCase().indexOf("/dashboard") == -1
            ) {
                localStorage.redirectUrl = value;
                localStorage.redirectTime = new Date();
            }
            return this;
        },

        getLastPath: function () {
            return lastPath;
        },

        isSuspended: function () {
            return suspended;
        },

        setSuspended: function (value) {
            suspended = value;
            return this;
        },

        buildUrl: function (path, relative) {
            if (relative) path = basePath + path;
            return path; // + (path.indexOf("?") >= 0 ? "&" : "?") + "d=" + new Date().getTime();
        },

        navigate: function (path, relative) {
            var path = matematici.navigator.buildUrl(path, relative);
            $.address.value(path);
        },

        open: function (path, relative, nocache, data) {
            if (data != undefined) callData = data;

            var actionNavigate = function () { matematici.navigator.navigate(path, relative, nocache); }

            if (matematici.canClose)
                actionNavigate();
            else
                matematici.dialog.show("warning", "Warning: any unsaved change will be lost.", [
                    { text: "save", icon: "check", handler: function () { if (matematici.save) matematici.save(actionNavigate); } },
                    { text: "discard the changes", icon: "exclamation", handler: actionNavigate },
                    { text: "cancel", hidden: true, icon: "remove", cancelButton: true }
                ]);
        },

        setUrl: function (path, relative) {
            if (window.history && window.history.replaceState)
                window.history.replaceState({}, '', matematici.navigator.buildUrl(path, relative));
            else {
                matematici.navigator.setSuspended(true);
                matematici.navigator.open(path, relative, nocache);
            }
        },

        defaultHandler: function (callback) {
            if (callback !== undefined) callback.call();
        },

        initializeEventHandlers: function () {
            matematici.events.onPageLoad =
                matematici.events.onPageUnload =
                matematici.events.onPageResize =
                matematici.events.onPageDestroy =
                matematici.navigator.defaultHandler;
        },

        checkEventHandlers: function () {
            if (matematici.events.onPageLoad === undefined) matematici.events.onPageLoad = matematici.navigator.defaultHandler;
            if (matematici.events.onPageUnload === undefined) matematici.events.onPageUnload = matematici.navigator.defaultHandler;
            if (matematici.events.onPageResize === undefined) matematici.events.onPageResize = matematici.navigator.defaultHandler;
            if (matematici.events.onPageDestroy === undefined) matematici.events.onPageDestroy = matematici.navigator.defaultHandler;
        },

        setBaseCSSClass: function (value) {
            matematici.elements.body.addClass(value);
            return matematici.navigator;
        },

        resetForm: function () {
            var form = $("form", matematici.elements.content);
            if (form.size() > 0) {
                form.unbind();
                form.data('validator', null);
                $.validator.unobtrusive.parse(matematici.elements.content);
                form.validate(form.data("unobtrusiveValidation").options);
            }
        },

        handlePageLoad: function (result, callback) {
            if (result !== null)
                matematici.elements.content.html(result);

            for (var index in matematici.initializers)
                matematici.initializers[index].call(null, matematici.elements.content);

            matematici.navigator.resetForm();

            matematici.events.onPageLoad.call(null, callback);
        },

        handleAjaxError: function (e) {
            if (e.status == 401 || e.status == 498) { //we use the status 498 to mean invalid token (as arcgis)
                if (matematici.appMode)
                    alert("Access denied to the following resource: " + location.href);
                else if (localStorage.accessToken && window.location && window.location.href.toLowerCase().indexOf("/dashboard") == -1) {
                    console.log("Access denied to the following resource: " + location.href);
                    window.location = matematici.urls.dashboard;
                } else 
                    window.location = matematici.urls.accessControl.login;

                return true;
            }
        },

        changePage: function (e) {
            //console.log("change", e.path, fullSupport, landing);
            if (!fullSupport || !landing) {
                //console.log("ajaxing " + e.path);

                matematici.loader.show();
                this.setLastPath(e.value);

                //highlight current menu
                $(".MenuItem", matematici.elements.menu).removeClass("Current");
                $("ul.SubMenu li", matematici.elements.menu).removeClass("Current");
                if (e.path != "/")
                    $("a", matematici.elements.menu).each(function () {
                        var a = $(this);
						if (a.attr("href") && a.attr("href").indexOf(e.path) >= 0) {
                            a.parent().addClass("Current").closest(".MenuItem").addClass("Current");
                            return true;
                        }
                    });

                $("document, html").animate({ scrollTop: 0 }, 500, function () {
                    matematici.events.onPageUnload(function () {

                        if (landing)
                            matematici.loader.show();

                        matematici.elements.content.stop(true).fadeTo(500, 0, function () {
                            $.ajax(e.value, {
                                type: 'POST',
                                data: callData,
                                cache: false
                            }).done(function (result, status, request) {
                                data = ''
                                var appMode = matematici.elements.body.hasClass("AppMode");
                                matematici.elements.body.removeClass();
                                if (appMode) matematici.elements.body.addClass("AppMode");
                                for (var index in matematici.destroyers)
                                    matematici.destroyers[index].call(null, matematici.elements.content);
                                matematici.events.onPageDestroy();
                                matematici.navigator.initializeEventHandlers();
                                matematici.navigator.handlePageLoad(result, function () {
                                    matematici.elements.content.stop(true).fadeTo(500, 1);
                                    matematici.loader.hide();
                                    matematici.loader.checkCurtain();
                                });
                            }).always(function () {
                            }).fail(function (e) {
                                if (!matematici.navigator.handleAjaxError(e))
                                    window.location = basePath + "Error";
                            });
                        });
                    });
                });
            }

            landing = false;
        }
    };
})();

$.address
    .state("/")
    .externalChange(function (e) {
        //console.log("external change " + e.path);
    }).internalChange(function (e) {
        //console.log("internal change " + e.path);
    }).change(function (e) {
        //console.log("change " + e.path);
        if (!matematici.navigator.isSuspended())
            matematici.navigator.changePage(e);
        matematici.navigator.setSuspended(false);
    });

$(document).on("click", "a:not(.Standard)[target!='_blank']", function (e) {
    var
        element = $(this),
        url = element == null ? null : element.attr("href");
    if (url != null && url.indexOf("#") == 0) {
        if (url == "#")
            e.preventDefault();
        else {
            var
                top = parseInt(url.substring(1)),
                offset = url == "#" + top ? { top: top } : $(url).offset();
            if (offset != null && offset.top != null) {
                matematici.elements.document.animate({ 'scrollTop': offset.top }, 500);
                matematici.navigator.setSuspended(true);
                if (offset.top == 0) {
                    //evita #0 nell'indirizzo
                    e.preventDefault();
                    matematici.navigator.open(matematici.navigator.getLastPath());
                }
            }
        }
    } else if (!e.ctrlKey && url != null && url.indexOf("http://") != 0 && url.indexOf("https://") != 0) {
        e.preventDefault();
        matematici.navigator.open(url);
    }
});

$(window).on("resize", function () {
    matematici.events.onPageResize();
});