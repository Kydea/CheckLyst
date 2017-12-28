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
        data.Type = type;
		data.ItemId = itemId;
		data.Code = instances.instance.getCode();

        $.ajax({
			url: matematici.urls.instances + "/Event",
            type: "post",
            data: data
        }).done(function (result) {
            if (done) {
                done(result);
                done = null;
            } else
                instances.instance.update(result);
        });
    };

    this.done = function (value) {
        done = value;
        return this;
    };
};