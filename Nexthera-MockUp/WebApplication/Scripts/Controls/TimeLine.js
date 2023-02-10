matematici.TimeLine = function (selector, data) {
    var
        timeLine = this,
        element = $(selector),
        elements = [],
        paths = [],

        addNode = function (pipe, nodeData) {
            var
                node = $("<div />").addClass("Node Container" + (nodeData.shape ? " " + nodeData.shape : "") + (nodeData.future ? " Future" : ""));
            node.append($("<span />").addClass("Label").html(nodeData.label));
            node.append($("<span />").html(nodeData.value));
            if (nodeData.position) 
                node.css("left", 230 * (nodeData.position - 1) + "px");
            if (nodeData.date)
                node.append($("<span />").addClass("Date").text(nodeData.date));
            else
                node.append($("<span />").addClass("Yellow StartedOn").text("not started"));

            pipe.append(node);
            elements[nodeData.id] = node;
        },

        addShape = function (pipe, shapeData) {
            var shape = $("<div />").addClass("Shape " + shapeData.class).text(shapeData.label);
            pipe.append(shape);
            elements[shapeData.id] = shape;
        },

        addPipe = function (pipeData) {
            var pipe = $("<div />").addClass("Pipe Container");
            if (pipeData.label)
                pipe.append($("<div />").addClass("Label").text(pipeData.label))
            for (var node in pipeData.nodes)
                addNode(pipe, pipeData.nodes[node]);
            for (var shape in pipeData.shapes)
                addShape(pipe, pipeData.shapes[shape]);
            element.append(pipe);
        },

        setup = function () {
            element.addClass("Container Pipes" + data.pipes.length);
            for (var pipe in data.pipes)
                addPipe(data.pipes[pipe]);
        };

    this.initialize = function () {
        setup();
    };

    this.getElement = function () {
        return element;
    }
};