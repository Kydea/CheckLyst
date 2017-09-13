matematici.client = function () {
    var
        handlers = [],

        trigger = function () {
        	var id = window.name; 
            console.log("Client id: " + id);
            for (var index in handlers)
                handlers[index](id);
        },

        set = function (id) {
            window.name = id;
            trigger();
        }

        get = function () {
            if (!!window.name)
                trigger();
            else
                $.ajax({
                    url: matematici.urls.client,
                    type: "post"
                }).done(function (result) {
                    set(result.Id);
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    console.log(jqXHR, textStatus, errorThrown);
                    matematici.dialog.error("server error", "Client unique identifier unavailable");
                });
        };

    if (matematici.appClientId == "")
        get();
    else
        set(parseInt(matematici.appClientId));

    return {
        addHandler: function (handler) {
        	var id = window.name;
            if (!!id)
                handler(id);
            else
                handlers.push(handler)
        },

        createNew: function(handler) {
        	window.name = "";
        	if (handler) matematici.client.addHandler(handler);
        	get();
        },

        getId: function () {
        	return window.name;
        }
    }
}()