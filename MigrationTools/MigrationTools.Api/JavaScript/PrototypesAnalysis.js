var matematici = matematici || {};
matematici.prototypeAnalysis = function () {
    var
        xhttp = new XMLHttpRequest(),

        done = function () {
            console.log(xhttp.responseText);
            new JsonViewer({
                container: document.getElementById("output"),
                data: xhttp.responseText,
                theme: 'light',
                expand: false
            });

            document.getElementById("link").classList.toggle("hidden");
            document.getElementById("linkLoading").classList.toggle("hidden");
        };

    return {
        go: function () {
            document.getElementById("output").innerHTML = "";
            document.getElementById("link").classList.toggle("hidden");
            document.getElementById("linkLoading").classList.toggle("hidden");
            xhttp.open("GET", "../../Analysis/Prototypes", true);
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) done();
            };
            xhttp.send();
        }, 

        stop: function () {
            xhttp.abort();
            document.getElementById("link").classList.toggle("hidden");
            document.getElementById("linkLoading").classList.toggle("hidden");
        }
    }
}();