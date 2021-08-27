var
    matematici = {
        version: "v. 1.0.0"
    };

window.addEventListener("DOMContentLoaded", function () {
    document.getElementsByTagName("title")[0].innerHTML += " " + matematici.version;
    var
        h1 = document.getElementsByTagName("h1")[0],
        subtitle = document.createElement("p");
    subtitle.innerText = matematici.version;
    h1.parentNode.insertBefore(subtitle, h1.nextSibling);
});
