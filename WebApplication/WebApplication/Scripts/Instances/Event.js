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
	skipCountDown: "SkipCountDown",
	quit: "Quit"
};

instances.Event = function (type, itemId, data, accessToken, suppressError) {
	var
		done,
		error;

    this.trigger = function () {
        data = data || {};
        data.type = type;
		data.itemId = itemId;
		data.code = instances.instance.getCode();

		$.ajax({
			url: matematici.urls.instances.api + "/Event",
			type: "post",
			data: data,
			accessToken: accessToken,
			suppressErrors: !!suppressError
		}).done(function (result) {
			if (done) {
				done(result);
				done = null;
			} else
				instances.instance.update(result);
		}).error(function () {
			if (error) error();
		});
    };

    this.done = function (value) {
        done = value;
        return this;
    };

	this.error = function (value) {
		error = value;
		return this;
	};
};