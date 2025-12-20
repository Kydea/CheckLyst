matematici.NotificationPollingClient = function (options) {
    var
        configuration = {
            sinceStorageKey: "NotificationPolling.Since",
            channelName: "NotificationPollingChannel",
            leaderStorageKey: "NotificationPolling.Leader",
            epochStorageKey: "NotificationPolling.Epoch",
            enabledStorageKey: "NotificationPolling.Enabled",
            snapshotStorageKey: "Notifications.LastSnapshot"
        },

        enabled = false,

        sinceVersion = 0,

        overloadAttempts = 0,
        lastDeliveredVersion = null,

        diagnostics = {
            isLeader: false,
            sinceVersion: sinceVersion,
            lastSuccessUtc: null,
            lastFailureUtc: null,
            lastFailureText: null,
            consecutiveFailures: 0,
            overloadCount: 0,
            lastSnapshotUtc: null,
            lastSnapshotVersion: null
        },

        coordinator = null,

        onNotificationSnapshot = function (snapshot) {
            if (!snapshot || snapshot.Overloaded || snapshot.TimedOut) return;

            //console.log("notification", snapshot);
            matematici.menu.setNotificationBadge("/Reports/Runs", snapshot.reportCount);
        },

        onDiagnostics = function (state) {
            //console.log("diagiostics", state);
        },

        startCoordinator = function () {
            if (coordinator) return;

            coordinator = new matematici.LongPollingCoordinator({
                channelName: configuration.channelName,
                leaderStorageKey: configuration.leaderStorageKey,
                epochStorageKey: configuration.epochStorageKey,

                heartbeatMilliseconds: configuration.heartbeatMilliseconds || 1000,
                leaderTimeToLiveMilliseconds: configuration.leaderTimeToLiveMilliseconds || 3500,
                watchdogMilliseconds: configuration.watchdogMilliseconds || 800,
                electionJitterMilliseconds: configuration.electionJitterMilliseconds || 250,

                backoffBaseMilliseconds: configuration.backoffBaseMilliseconds || 500,
                backoffMaximumMilliseconds: configuration.backoffMaximumMilliseconds || 30000,

                executeLongPollingOnce: function (signal) {
                    return executePollingOnce(signal);
                },

                onMessageFromLeader: function (snapshot) {
                    if (coordinator.isLeader()) return;
                    handleSnapshot(snapshot);
                    markSuccess(snapshot);
                }
            });

            updateDiagnostics();
        },

        stopCoordinator = function () {
            if (!coordinator) return;
            coordinator.stop();
            coordinator = null;
            updateDiagnostics();
        },

        readLastSnapshot = function () {
            try {
                var s = localStorage.getItem(configuration.snapshotStorageKey);
                return s ? JSON.parse(s) : null;
            } catch {
                return null;
            }
        },
    
        storeLastSnapshot = function (snapshot) {
            if (snapshot == null)
                localStorage.removeItem(configuration.snapshotStorageKey);
            else
                localStorage.setItem(configuration.snapshotStorageKey, JSON.stringify(snapshot));
        },

        executePollingOnce = function (signal) {
            return new Promise(function (resolve, reject) {
                var url = matematici.urls.api + "/Infrastructure/Notifications/Polling?since=" + sinceVersion;

                var jqXhr = $.ajax(url, {
                    type: "GET",
                    suppressErrors: true,
                    suppressLoader: true
                });

                var abortHandler = function () {
                    try { jqXhr.abort(); } catch (e) { }
                };

                if (signal) {
                    if (signal.aborted) {
                        abortHandler();
                        reject(new DOMException("Aborted", "AbortError"));
                        return;
                    }
                    signal.addEventListener("abort", abortHandler, { once: true });
                }

                jqXhr.done(function (snapshot) {
                    if (signal) signal.removeEventListener("abort", abortHandler);

                    try {
                        storeLastSnapshot(snapshot);
                        handleSnapshot(snapshot);
                        markSuccess(snapshot);
                        resolve(snapshot);
                    } catch (e) {
                        markFailure(e);
                        reject(e);
                    }
                });

                jqXhr.fail(function (jqXhrFail, textStatus, errorThrown) {
                    if (signal) signal.removeEventListener("abort", abortHandler);

                    if (textStatus === "abort") {
                        reject(new DOMException("Aborted", "AbortError"));
                        return;
                    }

                    var httpStatus = (jqXhrFail && jqXhrFail.status) ? jqXhrFail.status : 0;
                    var message = "HttpStatus " + httpStatus;

                    var error = new Error(message);
                    error.textStatus = textStatus;
                    error.errorThrown = errorThrown;

                    markFailure(error);
                    reject(error);
                });
            });
        },

        handleSnapshot = function (snapshot) {
            if (snapshot && snapshot.overloaded) {
                overloadAttempts = overloadAttempts + 1;
                diagnostics.overloadCount = diagnostics.overloadCount + 1;
                onNotificationSnapshot(snapshot);

                var wait = Math.min(30000, 1000 * Math.pow(2, Math.min(overloadAttempts, 6)));
                return new Promise(function (resolve) {
                    window.setTimeout(function () { resolve(); }, wait);
                });
            }

            overloadAttempts = 0;

            if (snapshot && typeof snapshot.version !== "undefined") {
                var incomingVersion = Number(snapshot.version);

                if (isFinite(incomingVersion) && incomingVersion >= 0) {
                    if (incomingVersion !== sinceVersion) 
                        sinceVersion = incomingVersion;

                    if (snapshot.timedOut === false) {
                        if (lastDeliveredVersion === incomingVersion) return;
                        lastDeliveredVersion = incomingVersion;
                    }
                }
            }

            updateDiagnostics();
            onNotificationSnapshot(snapshot);
        },

        updateDiagnostics = function () {
            diagnostics.isLeader = coordinator && coordinator.isLeader();
            diagnostics.sinceVersion = sinceVersion;
            onDiagnostics(diagnostics);
        },

        markSuccess = function (snapshot) {
            diagnostics.lastSuccessUtc = new Date().toISOString();
            diagnostics.lastFailureText = null;
            diagnostics.consecutiveFailures = 0;
            diagnostics.lastSnapshotUtc = new Date().toISOString();
            if (snapshot && typeof snapshot.version !== "undefined") diagnostics.lastSnapshotVersion = Number(snapshot.version);
            updateDiagnostics();
        },

        markFailure = function (error) {
            diagnostics.lastFailureUtc = new Date().toISOString();
            diagnostics.lastFailureText = error ? String(error.message || error) : "UnknownError";
            diagnostics.consecutiveFailures = diagnostics.consecutiveFailures + 1;
            updateDiagnostics();
        },

        enable = function () {
            if (enabled) return;
            enabled = true;
            localStorage.setItem(configuration.enabledStorageKey, "1");
            startCoordinator();
        },

        disable = function () {
            if (!enabled) return;
            enabled = false;
            localStorage.removeItem(configuration.enabledStorageKey);
            stopCoordinator();
        },

        initialize = function () {
            var snap = readLastSnapshot();
            if (snap) {
                handleSnapshot(snap);
                sinceVersion = snap.version;
            }
            if (localStorage.getItem(configuration.enabledStorageKey) == "1") enable();
        },

        dispose = function () {
            disable();
            sinceVersion = 0;
            lastDeliveredVersion = null;
            overloadAttempts = 0;
            storeLastSnapshot(null);
            updateDiagnostics();
        };

    this.setEnabled = function (value) {
        if (value) this.enable();
        else this.disable();
    };

    this.isEnabled = function () {
        return enabled;
    };

    this.getSinceVersion = function () {
        return sinceVersion;
    };

    this.enable = enable;
    this.disable = disable;
    this.dispose = dispose;

    initialize();
};