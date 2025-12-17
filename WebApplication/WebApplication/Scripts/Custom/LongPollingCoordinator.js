matematici.LongPollingCoordinator = function(options) {
    var
        configuration,
        executeLongPollingOnce,
        onMessageFromLeader,

        tabIdentifier,
        leaderIdentifier,
        leaderEpoch,

        broadcastChannel,
        heartbeatIntervalId,
        watchdogIntervalId,

        abortController,
        retryAttempt,

        leaderStorageKey,
        epochStorageKey,
        channelName,

        heartbeatMilliseconds,
        leaderTimeToLiveMilliseconds,
        watchdogMilliseconds,
        electionJitterMilliseconds,

        backoffBaseMilliseconds,
        backoffMaximumMilliseconds,

        postTakeoverCheckTimeoutId,
        postTakeoverCheckMilliseconds,

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
            var raw = localStorage.getItem(leaderStorageKey);
            if (!raw) return null;
            return readJson(raw);
        },

        writeLeaderRecord = function (record) {
            localStorage.setItem(leaderStorageKey, writeJson(record));
        },

        readEpochNumber = function () {
            return Number(localStorage.getItem(epochStorageKey) || "0");
        },

        writeEpochNumber = function (epoch) {
            localStorage.setItem(epochStorageKey, String(epoch));
        },

        increaseEpochNumber = function () {
            var next = readEpochNumber() + 1;
            writeEpochNumber(next);
            return next;
        },

        isLeaderRecordExpired = function (record) {
            return (nowMilliseconds() - record.ts) > leaderTimeToLiveMilliseconds;
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

            if (!isCurrentTabLeader()) {
                stopLongPolling();
                stopHeartbeat();
            }
        },

        scheduleElectionAttempt = function (delayMilliseconds) {
            var delay = (typeof delayMilliseconds === "number")
                ? delayMilliseconds
                : Math.floor(Math.random() * electionJitterMilliseconds);

            window.setTimeout(attemptElection, delay);
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

            if (broadcastChannel) {
                broadcastChannel.postMessage({ type: "LeaderTakenOver", leaderId: tabIdentifier, epoch: nextEpoch, ts: record.ts });
            }

            leaderIdentifier = tabIdentifier;
            leaderEpoch = nextEpoch;

            startHeartbeat();
            startLongPollingLoop();
            schedulePostTakeoverCheck();
        },

        startHeartbeat = function () {
            if (heartbeatIntervalId) return;

            heartbeatIntervalId = window.setInterval(function () {
                if (!isCurrentTabLeader()) return;

                var record = { leaderId: tabIdentifier, epoch: leaderEpoch, ts: nowMilliseconds() };
                writeLeaderRecord(record);

                if (broadcastChannel) {
                    broadcastChannel.postMessage({ type: "LeaderHeartbeat", leaderId: record.leaderId, epoch: record.epoch, ts: record.ts });
                }
            }, heartbeatMilliseconds);
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
            }, watchdogMilliseconds);
        },

        stopWatchdog = function () {
            if (!watchdogIntervalId) return;
            window.clearInterval(watchdogIntervalId);
            watchdogIntervalId = null;
        },

        stopLongPolling = function () {
            if (abortController) {
                try { abortController.abort(); } catch (e) { }
            }
            abortController = null;
            retryAttempt = 0;
        },

        startLongPollingLoop = function () {
            var execute;

            execute = function () {
                if (!isCurrentTabLeader()) return;

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

                        var base = Math.min(backoffMaximumMilliseconds, backoffBaseMilliseconds * Math.pow(2, retryAttempt++));
                        var jitter = base * (0.2 + Math.random() * 0.2);
                        var wait = Math.floor(base + jitter);

                        window.setTimeout(execute, wait);
                    });
            };

            execute();
        },

        onStorageEvent = function (event) {
            if (!event || event.key !== leaderStorageKey) return;

            var record = readLeaderRecord();
            if (!record) return;

            adoptLeaderIfBetter(record.leaderId, record.epoch);
        },

        onBroadcastMessage = function (event) {
            var message = event && event.data;
            if (!message || !message.type) return;

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
            }, postTakeoverCheckMilliseconds);
        },

        initialize = function () {
            configuration = options || {};

            executeLongPollingOnce = configuration.executeLongPollingOnce;
            onMessageFromLeader = configuration.onMessageFromLeader || function () { };

            if (typeof executeLongPollingOnce !== "function") throw new Error("LongPollingCoordinator richiede options.executeLongPollingOnce");

            channelName = configuration.channelName || "LongPollingChannel";
            leaderStorageKey = configuration.leaderStorageKey || "LongPolling.Leader";
            epochStorageKey = configuration.epochStorageKey || "LongPolling.Epoch";

            heartbeatMilliseconds = configuration.heartbeatMilliseconds || 1000;
            leaderTimeToLiveMilliseconds = configuration.leaderTimeToLiveMilliseconds || 3500;
            watchdogMilliseconds = configuration.watchdogMilliseconds || 800;
            electionJitterMilliseconds = configuration.electionJitterMilliseconds || 250;

            backoffBaseMilliseconds = configuration.backoffBaseMilliseconds || 500;
            backoffMaximumMilliseconds = configuration.backoffMaximumMilliseconds || 30000;

            postTakeoverCheckMilliseconds = configuration.postTakeoverCheckMilliseconds || 1200

            tabIdentifier = (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : String(Math.random()).slice(2);
            leaderIdentifier = null;
            leaderEpoch = 0;

            retryAttempt = 0;

            if (typeof BroadcastChannel !== "undefined") {
                broadcastChannel = new BroadcastChannel(channelName);
                broadcastChannel.onmessage = onBroadcastMessage;
            } else {
                broadcastChannel = null;
            }

            window.addEventListener("storage", onStorageEvent);
            document.addEventListener("visibilitychange", onVisibilityChange);

            startWatchdog();
            scheduleElectionAttempt(0);
        };

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