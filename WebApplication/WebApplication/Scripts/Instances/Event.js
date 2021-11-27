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
	quit: "Quit",
	uploadAttachment: "UploadAttachment",
    sendEmail: "SendEmail",
    import: "Import",
    attachInstance: "AttachInstance",
    refreshAttachedInstance: "RefreshAttachedInstance"
};

instances.Event = function (type, itemId, data, accessToken, suppressError, fileUpload) {
	var
		done,
		error;

	this.trigger = function () {
		var options = {
			url: matematici.urls.instances.api + "/Event",
			type: "post",
			accessToken: accessToken,
			suppressErrors: !!suppressError
		};

		if (fileUpload) {
			data = data || new FormData();
			data.append("type", type);
			data.append("itemId", itemId);
			data.append("code", instances.instance.getCode());

			options.cache = false;
			options.contentType = false;
			options.processData = false;
		} else {
			data = data || {};
			data.type = type;
			data.itemId = itemId;
			data.code = instances.instance.getCode();
		}

		options.data = data;

		$.ajax(options).done(function (result) {
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