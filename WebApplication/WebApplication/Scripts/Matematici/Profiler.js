matematici.Profiler = function () {
    var
        date,
        time = 0;

    this.reset = function() {
        time = 0;
    }

    this.start = function () {
        date = new Date();
    };

    this.stop = function () {
        time += new Date() - date;
    };

    this.log = function () {
        console.log(time, "ms");
    }

}

matematici.profiler = new matematici.Profiler();