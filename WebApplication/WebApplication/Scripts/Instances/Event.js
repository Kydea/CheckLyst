instances.Events = {
    setup: "Setup",
    refresh: "Refresh",
    pageChange: "PageChange",
    valueChange: "ValueChange",
	valueCopy: "ValueCopy",
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

instances.Event = function (type, itemId, data, accessToken) {
    var done;

    this.trigger = function () {
        data = data || {};
        data.type = type;
		data.itemId = itemId;
		data.code = instances.instance.getCode();

        $.ajax({
			url: matematici.urls.instances.api + "/Event",
            type: "post",
			data: data,
			accessToken: accessToken
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