matematici.prototypeAnalysis = function () {
    var
        setTotals = function (records) {
            var html = "";
            var totals = document.getElementById("totals");
            for (var index in records)
                html += "<br />" + index + ": " + records[index].ok + " ok, " + records[index].ko + " ko";
            totals.innerHTML = html;
        },

        setErrors = function (records) {
            var html = "<table><tr><th>company</th><th>prototype</th><th>version</th><th>state</th><th>error</th></tr>";
            for (var prototype in records)
                for (var error in records[prototype]) {
                    var tokens = prototype.split(".");
                    html += "<tr><td>" + tokens[0] + "</td><td>" + tokens[1] + "</td><td>" + tokens[2] + "</td><td>" + tokens[3] + "</td><td>" + records[prototype][error] + "</td></tr>";
                }
            html += "</table>";
            detail.innerHTML = html;
            document.getElementById("copy").classList.remove("hidden")
        },

        copy = function () {
            var textArea = document.createElement("textarea");
            document.body.appendChild(textArea);
            textArea.value = document.getElementById("detail").innerHTML;
            textArea.focus();
            textArea.select();

            try {
                successful = document.execCommand("copy");
            } catch (e) {
                console.log(e);
            }
            setTimeout(function () { textArea.remove(); }, 0);
        },

        ping = function () {
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "../../PrototypesAnalysis/Monitor", true);
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4) {
                    if (this.status == 200) {
                        var monitor = JSON.parse(this.responseText);
                        document.getElementById("start").classList.toggle("hidden", !!monitor.running);
                        document.getElementById("stop").classList.toggle("hidden", !monitor.running);
                        var controls = document.getElementById("status").getElementsByTagName("input");
                        for (var index in controls) {
                            var control = controls[index];
                            control.value = monitor[control.name];
                        }
                        if (monitor.counts) setTotals(monitor.counts);
                        if (monitor.errors) setErrors(monitor.errors);
                        setTimeout(ping, 1000);
                    } else {
                        console.log(xhttp);
                        alert("Error reading server status");
                    }
                }
            };
            xhttp.send();
        },

        initialize = function () {
            ping();
        };

    initialize();

    return {
        go: function () {
            var xhttp = new XMLHttpRequest();

            errors = document.getElementById("totals").innerHTML = document.getElementById("detail").innerHTML = "n/a";
            document.getElementById("copy").classList.add("hidden");
            document.getElementById("start").classList.add("hidden");
            xhttp.open("GET", "../../PrototypesAnalysis/Start", true);
            xhttp.send();
        },

        stop: function () {
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "../../PrototypesAnalysis/Abort", true);
            xhttp.send();
        },

        ping: ping,
        copy: copy
    }
}();