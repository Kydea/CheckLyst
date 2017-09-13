instances.Events = {
    setup: "Setup",
    refresh: "Refresh",
    pageChange: "PageChange",
    valueChange: "ValueChange",
    copyValue: "CopyValue",
    up: "Up",
    down: "Down",
    add: "Add",
    remove: "Remove",
    signature: "Signature",
    signatureCheck: "SignatureCheck",
    signatureCancelation: "SignatureCancelation",
    externalSourceRowChange: "ExternalSourceRowChange",
    countDown: "CountDown",
    unlockPage: "UnlockPage",
    skipCountDown: "SkipCountDown"
};

instances.Event = function (type, itemId, data) {
    var done;

    this.trigger = function () {
        data = data || {};
        data.ClientId = matematici.client.getId();
        data.Type = type;

        if (typeof itemId === "object") {
        	data.Prototype = itemId.prototype;
        	data.Version = itemId.version;
        	data.Year = itemId.year;
        	data.Number = itemId.number;
        } else 
        	data.ItemId = itemId;

        $.ajax({
            url: matematici.urls.instances.event,
            type: "post",
            data: data
        }).done(function (result) {
            if (done) {
                done(result);
                done = null;
            } else
                instances.instance.update(result);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.log("Instance error", jqXHR, textStatus, errorThrown);
            alert("Instance error: " + errorThrown);
        });
    };

    this.done = function (value) {
        done = value;
        return this;
    };
};