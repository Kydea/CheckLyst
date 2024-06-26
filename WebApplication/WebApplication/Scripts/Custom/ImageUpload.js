matematici.ImageUpload = function (model) {
    var
        videoElement,
        canvasElement,
        currentStream,

        getVideoDevices = function () {
            return navigator.mediaDevices.enumerateDevices().then(devices => {
                return devices.filter(device => device.kind === 'videoinput');
            });
        },

        startDevice = function (deviceId) {
            closeStream();
            const constraints = {
                video: {
                    deviceId: deviceId ? { exact: deviceId } : undefined
                }
            };

            navigator.mediaDevices.getUserMedia(constraints)
                .then(stream => {
                    currentStream = videoElement.srcObject = stream;
                })
                .catch(error => {
                    matematici.dialog.error("Video stream error", error || "(unspecified error)");
                });
        },

        upload = function (image) {
            var data = new FormData();
            data.append("attachment", image);
            new instances.Event(instances.Events.uploadAttachment, model.id, data, undefined, undefined, true).done(function (result) {
                instances.instance.update(result);
                closeStream();
                matematici.popup.close();
            }).trigger();
        },

        initialize = function () {
            videoElement = document.createElement("video");
            canvasElement = document.createElement("canvas");

            var
                content = document.createElement("div"),
                deviceSelect = document.createElement("select"),
                context = canvasElement.getContext('2d');

            getVideoDevices().then(videoDevices => {
                var selectedDevice = localStorage["ScannerDevice"];
                videoDevices.forEach(device => {
                    const option = document.createElement('option');
                    option.value = device.deviceId;
                    option.text = device.label || `Camera ${deviceSelect.length + 1}`;
                    if (selectedDevice == device.deviceId) option.setAttribute("selected", "selected");
                    deviceSelect.appendChild(option);
                });
                setTimeout(function () {
                    matematici.AutoComplete.get(deviceSelect).refresh();
                    startDevice(deviceSelect.value);
                }, 1000);
            });

            deviceSelect.addEventListener("change", function () {
                startDevice(deviceSelect.value);
                localStorage["CaptureDevice"] = deviceId;
            })
            content.appendChild(deviceSelect);

            videoElement.setAttribute("autoplay", true);
            videoElement.style.setProperty("width", "320px");
            videoElement.style.setProperty("margin-top", "20px")
            content.appendChild(videoElement);

            canvasElement.style.setProperty("display", "none");
            content.appendChild(canvasElement);

            var buttons = [{
                text: "capture",
                icon: "camera",
                handler: function () {
                    var
                        width = videoElement.videoWidth,
                        height = videoElement.videoHeight;
                    canvasElement.width = width;
                    canvasElement.height = height;  

                    context.drawImage(videoElement, 0, 0, width, height);
                    canvasElement.toBlob(function (blob) {
                        //upload(blob);
                    }, 'image/jpeg');
                    return true;
                }
            }, {
                hidden: true,
                cancelButton: true,
                handler: closeStream
            }];

            matematici.popup.show({
                title: "camera upload",
                content: content,
                cssClass: "ImageViewer",
                buttons: buttons
            });

            matematici.popup.setWidth(360);
        },

        closeStream = function () {
            if (currentStream) {
                currentStream.getTracks().forEach(track => track.stop());
                videoElement.srcObject = null;
                currentStream = null;
            }
        };

    initialize();
};