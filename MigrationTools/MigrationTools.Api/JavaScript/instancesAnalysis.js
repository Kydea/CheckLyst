matematici.instancesAnalysis = function () {
    var
        setTotals = function (ok, ko) {
            var totals = document.getElementById("totals");
            totals.innerHTML = ok + " ok, " + ko + " ko";
        },

        setErrors = function (records) {
            var html = "<table><tr><th>company</th><th>prototype</th><th>version</th><th>year</th><th>number</th><th>state</th><th>error</th></tr>";
            for (var index in records) {
                var tokens = records[index].Key.split(".");
                html += "<tr><td>" + tokens[0] + "</td><td>" + (tokens[1] || "") + "</td><td>" + (tokens[2] || "") + "</td><td>" + (tokens[3] || "") + "</td><td>" + (tokens[4] || "") + "</td><td>" + (tokens[5] || "") + "</td><td>" + records[index].Message + "</td></tr>";
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

        update = function () {
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "../../InstancesAnalysis/Update", true);
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4) {
                    if (this.status == 200) {
                        var monitor = JSON.parse(this.responseText);
                        document.getElementById("continue").classList.toggle("hidden", !!monitor.running);
                        document.getElementById("restart").classList.toggle("hidden", !!monitor.running);
                        document.getElementById("stop").classList.toggle("hidden", !monitor.running);
                        var controls = document.getElementById("status").getElementsByTagName("input");
                        for (var index in controls) {
                            var control = controls[index];
                            control.value = monitor[control.name];
                        }
                        if (monitor.ok != undefined) setTotals(monitor.ok, monitor.ko);
                        if (monitor.errors) setErrors(monitor.errors);
                        //setTimeout(ping, 10000);
                    } else {
                        console.log(xhttp);
                        alert("Error reading status");
                    }
                }
            };
            xhttp.send();
        },

        initialize = function () {
            update();
        };

    initialize();

    return {
        continue: function () {
            var xhttp = new XMLHttpRequest();

            errors = document.getElementById("totals").innerHTML = document.getElementById("detail").innerHTML = "n/a";
            document.getElementById("copy").classList.add("hidden");
            document.getElementById("continue").classList.add("hidden");
            document.getElementById("restart").classList.add("hidden");
            xhttp.open("GET", "../../InstancesAnalysis/Continue", true);
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4) {
                    if (this.status == 200)
                        update();
                    else {
                        console.log(xhttp);
                        alert("Error starting service");
                    }
                }
            };

            xhttp.send();
        },

        restart: function () {
            var xhttp = new XMLHttpRequest();

            errors = document.getElementById("totals").innerHTML = document.getElementById("detail").innerHTML = "n/a";
            document.getElementById("copy").classList.add("hidden");
            document.getElementById("continue").classList.add("hidden");
            document.getElementById("restart").classList.add("hidden");
            xhttp.open("GET", "../../InstancesAnalysis/Restart", true);
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4) {
                    if (this.status == 200)
                        update();
                    else {
                        console.log(xhttp);
                        alert("Error restarting service");
                    }
                }
            };

            xhttp.send();
        },

        stop: function () {
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "../../InstancesAnalysis/Stop", true);
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4) {
                    if (this.status == 200)
                        update();
                    else {
                        console.log(xhttp);
                        alert("Error stopping service");
                    }
                }
            };
            xhttp.send();
        },

        update: update,
        copy: copy
    }
}();