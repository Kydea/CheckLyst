matematici.LongPollingCoordinator = function (configuration, executeLongPollingOnce, onMessageFromLeader) {
    var
        dataEnabled,

        tabIdentifier,
        leaderIdentifier,
        leaderEpoch,

        broadcastChannel,
        heartbeatIntervalId,
        watchdogIntervalId,

        electionTimeoutId,
        retryTimeoutId,

        abortController,
        retryAttempt,

        postTakeoverCheckTimeoutId,


        readJson = function (value) {
            try { return JSON.parse(value); } catch (e) { return null; }
        },

        writeJson = function (value) {
            return JSON.stringify(value);
        },

        nowMilliseconds = function () {
            return Date.now();
        },

        readLeaderRecord = function () {
            var raw = localStorage.getItem(configuration.leaderStorageKey);
            if (!raw) return null;
            return readJson(raw);
        },

        writeLeaderRecord = function (record) {
            localStorage.setItem(configuration.leaderStorageKey, writeJson(record));
        },

        readEpochNumber = function () {
            return Number(localStorage.getItem(configuration.epochStorageKey) || "0");
        },

        writeEpochNumber = function (epoch) {
            localStorage.setItem(configuration.epochStorageKey, String(epoch));
        },

        increaseEpochNumber = function () {
            var next = readEpochNumber() + 1;
            writeEpochNumber(next);
            return next;
        },

        isLeaderRecordExpired = function (record) {
            return (nowMilliseconds() - record.ts) > configuration.leaderTimeToLiveMilliseconds;
        },

        isCurrentTabLeader = function () {
            return leaderIdentifier === tabIdentifier;
        },

        adoptLeaderIfBetter = function (newLeaderIdentifier, newLeaderEpoch) {
            var better =
                (newLeaderEpoch > leaderEpoch) ||
                (newLeaderEpoch === leaderEpoch && newLeaderIdentifier < (leaderIdentifier || "\uffff"));

            if (!better) return;

            leaderIdentifier = newLeaderIdentifier;
            leaderEpoch = newLeaderEpoch;

            if (isCurrentTabLeader()) {
                startHeartbeat();
                if (dataEnabled) startLongPollingLoop();
            } else {
                stopLongPolling();
                stopHeartbeat();
            }
        },

        scheduleElectionAttempt = function (delayMilliseconds) {
            var delay = (typeof delayMilliseconds === "number")
                ? delayMilliseconds
                : Math.floor(Math.random() * configuration.electionJitterMilliseconds);

            if (electionTimeoutId) window.clearTimeout(electionTimeoutId)
            electionTimeoutId = window.setTimeout(attemptElection, delay);
        },

        attemptElection = function () {
            var currentLeader = readLeaderRecord();
            if (currentLeader && !isLeaderRecordExpired(currentLeader)) {
                adoptLeaderIfBetter(currentLeader.leaderId, currentLeader.epoch);
                return;
            }

            var nextEpoch = increaseEpochNumber();
            var record = { leaderId: tabIdentifier, epoch: nextEpoch, ts: nowMilliseconds() };

            writeLeaderRecord(record);

            if (broadcastChannel)
                broadcastChannel.postMessage({ type: "LeaderTakenOver", leaderId: tabIdentifier, epoch: nextEpoch, ts: record.ts });

            leaderIdentifier = tabIdentifier;
            leaderEpoch = nextEpoch;

            startHeartbeat();
            if (dataEnabled) startLongPollingLoop();
            schedulePostTakeoverCheck();
        },

        startHeartbeat = function () {
            if (heartbeatIntervalId) return;

            heartbeatIntervalId = window.setInterval(function () {
                if (!isCurrentTabLeader()) return;

                var record = { leaderId: tabIdentifier, epoch: leaderEpoch, ts: nowMilliseconds() };
                writeLeaderRecord(record);

                if (broadcastChannel)
                    broadcastChannel.postMessage({ type: "LeaderHeartbeat", leaderId: record.leaderId, epoch: record.epoch, ts: record.ts });
            }, configuration.heartbeatMilliseconds);
        },

        stopHeartbeat = function () {
            if (!heartbeatIntervalId) return;
            window.clearInterval(heartbeatIntervalId);
            heartbeatIntervalId = null;
        },

        startWatchdog = function () {
            if (watchdogIntervalId) return;

            watchdogIntervalId = window.setInterval(function () {
                var currentLeader = readLeaderRecord();
                if (!currentLeader || isLeaderRecordExpired(currentLeader)) scheduleElectionAttempt();
            }, configuration.watchdogMilliseconds);
        },

        stopWatchdog = function () {
            if (!watchdogIntervalId) return;
            window.clearInterval(watchdogIntervalId);
            watchdogIntervalId = null;
        },

        stopLongPolling = function () {
            if (abortController) try { abortController.abort(); } catch (e) { }
            abortController = null;
            retryAttempt = 0;

            if (retryTimeoutId) window.clearTimeout(retryTimeoutId);
            retryTimeoutId = null;
        },

        startLongPollingLoop = function () {
            var execute;

            execute = function () {
                if (!dataEnabled || !isCurrentTabLeader()) return;

                abortController = new AbortController();

                Promise.resolve()
                    .then(function () { return executeLongPollingOnce(abortController.signal); })
                    .then(function (payload) {
                        if (!isCurrentTabLeader()) return;

                        onMessageFromLeader(payload);

                        if (broadcastChannel) {
                            broadcastChannel.postMessage({ type: "LeaderMessage", epoch: leaderEpoch, payload: payload });
                        }

                        retryAttempt = 0;
                        execute();
                    })
                    .catch(function (error) {
                        if (!isCurrentTabLeader()) return;
                        if (error && error.name === "AbortError") return;

                        var base = Math.min(configuration.backoffMaximumMilliseconds, configuration.backoffBaseMilliseconds * Math.pow(2, retryAttempt++));
                        var jitter = base * (0.2 + Math.random() * 0.2);
                        var wait = Math.floor(base + jitter);

                        if (retryTimeoutId) window.clearTimeout(retryTimeoutId);
                        retryTimeoutId = window.setTimeout(execute, wait);
                    });
            };

            execute();
        },

        onStorageEvent = function (event) {
            if (!event || event.key !== configuration.leaderStorageKey) return;

            var record = readLeaderRecord();
            if (!record) return;

            adoptLeaderIfBetter(record.leaderId, record.epoch);
        },

        onBroadcastMessage = function (event) {
            var message = event && event.data;
            if (!message || !message.type) return;

            if (message.type === "NotificationPollingEnabledChanged" && message.sourceTabId !== tabIdentifier) {
                setDataEnabled(message.enabled, true);
                return;
            }

            if (message.type === "LeaderHeartbeat" || message.type === "LeaderTakenOver") {
                adoptLeaderIfBetter(message.leaderId, message.epoch);
                return;
            }

            if (message.type === "LeaderMessage") {
                if (isCurrentTabLeader()) return;
                if (message.epoch < leaderEpoch) return;
                onMessageFromLeader(message.payload);
            }
        },

        onVisibilityChange = function () {
            if (document.visibilityState === "visible") {
                scheduleElectionAttempt(0);
            }
        },

        schedulePostTakeoverCheck = function () {
            if (postTakeoverCheckTimeoutId) window.clearTimeout(postTakeoverCheckTimeoutId);

            postTakeoverCheckTimeoutId = window.setTimeout(function () {
                if (!isCurrentTabLeader()) return;

                var record = readLeaderRecord();
                if (!record) return;

                if (record.leaderId !== tabIdentifier || Number(record.epoch) !== Number(leaderEpoch)) {
                    adoptLeaderIfBetter(record.leaderId, record.epoch);
                }
            }, configuration.postTakeoverCheckMilliseconds);
        },

        setDataEnabled = function (value, silent) {
            value = !!value;
            if (dataEnabled === value) return;
            dataEnabled = value;

            if (!dataEnabled)
                stopLongPolling();
            else if (isCurrentTabLeader())
                startLongPollingLoop();

            if (!silent && broadcastChannel)
                broadcastChannel.postMessage({ type: "NotificationPollingEnabledChanged", enabled: dataEnabled, sourceTabId: tabIdentifier });
        },

        initialize = function () {
            tabIdentifier = (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : String(Math.random()).slice(2);
            leaderIdentifier = null;
            leaderEpoch = 0;
            retryAttempt = 0;

            if (typeof BroadcastChannel !== "undefined") {
                broadcastChannel = new BroadcastChannel(configuration.channelName);
                broadcastChannel.onmessage = onBroadcastMessage;
            } else {
                broadcastChannel = null;
            }

            window.addEventListener("storage", onStorageEvent);
            document.addEventListener("visibilitychange", onVisibilityChange);

            startWatchdog();
            scheduleElectionAttempt(0);
        };


    this.setDataEnabled = setDataEnabled;

    this.stop = function () {
        stopLongPolling();
        stopHeartbeat();
        stopWatchdog();

        window.removeEventListener("storage", onStorageEvent);
        document.removeEventListener("visibilitychange", onVisibilityChange);

        if (broadcastChannel) {
            try { broadcastChannel.close(); } catch (e) { }
            broadcastChannel = null;
        }

        if (postTakeoverCheckTimeoutId) window.clearTimeout(postTakeoverCheckTimeoutId);
        postTakeoverCheckTimeoutId = null;
        if (electionTimeoutId) window.clearTimeout(electionTimeoutId);
        electionTimeoutId = null;
    };

    this.isLeader = function () {
        return isCurrentTabLeader();
    };

    this.getTabIdentifier = function () {
        return tabIdentifier;
    };

    this.getLeaderEpoch = function () {
        return leaderEpoch;
    };

    initialize();
};