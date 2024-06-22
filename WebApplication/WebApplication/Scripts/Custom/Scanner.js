matematici.Scanner = function (handler) {
    var
        videoElement,
        codeReader = new ZXing.BrowserQRCodeReader(),

        startDevice = function (selectedDeviceId) {
            codeReader.decodeFromVideoDevice(selectedDeviceId, videoElement, (result, error) => {
                if (result) {
                    destroy();
                    handler(result.text);
                    matematici.popup.close();
                }
            });
        },

        destroy = function () {
            codeReader.reset();
        },

        initialize = function () {
            var
                content = document.createElement("div"),
                deviceSelect = document.createElement("select");

            content.appendChild(deviceSelect);
            videoElement = document.createElement("video");
            videoElement.style.setProperty("margin-top", "20px")
            videoElement.style.setProperty("width", "300px")
            content.appendChild(videoElement);

            matematici.popup.show({
                title: "scanner",
                content: content,
                buttons: [
                    {
                        text: "close",
                        icon: "times",
                        handler: destroy
                    },
                    {
                        hidden: true,
                        cancelButton: true,
                        handler: destroy
                    }
                ]
            });

            matematici.popup.setWidth(340);
            codeReader.getVideoInputDevices()
                .then((videoInputDevices) => {
                    if (videoInputDevices.length == 0)
                        throw new Error("no cam found");
                    else {
                        deviceSelect.addEventListener("change", function (e) {
                            var deviceId = deviceSelect.value;
                            codeReader.reset();
                            startDevice(deviceId);
                            localStorage["ScannerDevice"] = deviceId;
                        });

                        var selectedDevice = parseInt(localStorage["ScannerDevice"]);
                        if (selectedDevice == null || isNaN(selectedDevice) || selectedDevice > videoInputDevices.length) selectedDevice = videoInputDevices[0].deviceId;
                        for (var index in videoInputDevices) {
                            var
                                option = document.createElement("option"),
                                id = videoInputDevices[index].deviceId,
                                label = videoInputDevices[index].label;
                            option.setAttribute("value", id);
                            if (selectedDevice == id) option.setAttribute("selected", "selected");
                            option.innerText = label == "" || label == null ? "device " + index : label;
                            deviceSelect.appendChild(option);
                        }
                        matematici.AutoComplete.get(deviceSelect).refresh();
                        startDevice(selectedDevice);
                    }
                })
                .catch((error) => {
                    matematici.dialog.error("Error", error || "(unspecified error)");
                    console.error(error);
                });
        };

    initialize();
}