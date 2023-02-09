matematici.Graph = function (selector, data) {
    var
        graph = this,
        element = $(selector),
        elements = [],
        paths = [],

        addNode = function (pipe, nodeData) {
            var node = $("<div />").addClass("Node" + (nodeData.shape ? " " + nodeData.shape : "") + (nodeData.future ? " Future" : ""));
            node.append($("<span />").addClass("Label").html(nodeData.label));
            node.append($("<span />").addClass("Center").html(nodeData.center));
            if (nodeData.date)
                node.append($("<span />").addClass("Date").text(nodeData.date));
            else if (nodeData.madeToStock)
                node.append($("<span />").addClass("Grey StartedOn").text("made to stock"));
            else if (!nodeData.startedOn)
                node.append($("<span />").addClass("Yellow StartedOn").text("not started"));
            else {
                node.append($("<span />").addClass("StartedOn").text(nodeData.startedOn));
                node.append($("<span />").addClass("NoMargin Arrow").append($("<i />").addClass("fal fa-arrow-down")));
                if (!nodeData.completedOn)
                    node.append($("<span />").addClass("NoMargin Yellow CompletedOn").text("not completed"));
                else
                    node.append($("<span />").addClass("NoMargin CompletedOn").text(nodeData.completedOn));
            }

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

        addPath = function (pathData) {
            var
                containerOffset = element.offset(),
                source = elements[pathData.sourceId],
                destination = elements[pathData.destinationId],
                sourceOffset = source.offset();
            sourceOffset.left -= containerOffset.left;
            sourceOffset.top -= containerOffset.top;
            var
                sourceSize = {
                    width: source.outerWidth(), height: source.outerHeight()
                },
                destinationOffset = destination.offset(),
                destinationSize = {
                    width: destination.outerWidth(), height: destination.outerHeight()
                };
            destinationOffset.left -= containerOffset.left;
            destinationOffset.top -= containerOffset.top;
            var
                sourcePoint = {
                    x:
                        pathData.sourcePosition == "l" ? sourceOffset.left :
                            pathData.sourcePosition == "r" ? sourceOffset.left + sourceSize.width :
                                sourceOffset.left + sourceSize.width * (pathData.sourceBias || .5),
                    y:
                        pathData.sourcePosition == "t" ? sourceOffset.top :
                            pathData.sourcePosition == "b" ? sourceOffset.top + sourceSize.height :
                                sourceOffset.top + sourceSize.height * (pathData.sourceBias || .5)
                },
                destinationPoint = {
                    x:
                        pathData.destinationPosition == "l" ? destinationOffset.left :
                            pathData.destinationPosition == "r" ? destinationOffset.left + destinationSize.width :
                                destinationOffset.left + destinationSize.width * (pathData.destinationBias || .5),
                    y:
                        pathData.destinationPosition == "t" ? destinationOffset.top :
                            pathData.destinationPosition == "b" ? destinationOffset.top + destinationSize.height :
                                destinationOffset.top + destinationSize.height * (pathData.destinationBias || .5)
                };

            if (pathData.sourcePosition == "r" && pathData.destinationPosition == "l" || pathData.sourcePosition == "l" && pathData.destinationPosition == "r") {
                var
                    a = pathData.sourcePosition == "r" ? sourcePoint : destinationPoint,
                    b = pathData.sourcePosition == "r" ? destinationPoint : sourcePoint;

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal " + (pathData.cssClass || "")).css({
                    left: a.x,
                    top: a.y,
                    width: (b.x - a.x) / 2,
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Vertical " + (pathData.cssClass || "")).css({
                    left: a.x + (b.x - a.x) / 2,
                    top: Math.min(a.y, b.y),
                    height: Math.abs(b.y - a.y) + 1
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal " + (pathData.cssClass || "")).css({
                    left: a.x + (b.x - a.x) / 2,
                    top: b.y,
                    width: (b.x - a.x) / 2,
                }));
            } else if (pathData.sourcePoint == "r" || pathData.sourcePoint == "l")
                alert("TODO");
            else if (pathData.sourcePosition == "b" && pathData.destinationPosition == "t" || pathData.sourcePosition == "t" && pathData.destinationPosition == "b") {
                var
                    a = pathData.sourcePosition == "b" ? sourcePoint : destinationPoint,
                    b = pathData.sourcePosition == "b" ? destinationPoint : sourcePoint;

                paths.push($("<div />").appendTo(element).addClass("Path Vertical " + (pathData.cssClass || "")).css({
                    left: a.x,
                    top: a.y,
                    height: (b.y - a.y) / 2,
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal " + (pathData.cssClass || "")).css({
                    left: Math.min(a.x, b.x),
                    top: a.y + (b.y - a.y) / 2,
                    width: Math.abs(b.x - a.x) + 1
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Vertical " + (pathData.cssClass || "")).css({
                    left: b.x,
                    top: a.y + (b.y - a.y) / 2,
                    height: (b.y - a.y) / 2,
                }));
            } else if (pathData.sourcePoint == "r" || pathData.sourcePoint == "l")
                alert("TODO")


            paths.push($("<div />").appendTo(element).addClass("Arrow To" + pathData.destinationPosition.toUpperCase()).css({
                left: destinationPoint.x,
                top: destinationPoint.y,
            }));
        },

        addPaths = function () {
            for (var path in data.paths)
                addPath(data.paths[path]);

        }

        clearPaths = function () {
            for (var path in paths)
                paths[path].remove();
        },

        resize = function () {
            clearPaths();
            addPaths();
        },

        setup = function () {
            element.addClass("Container Pipes" + data.pipes.length + " " + data.orientation);
            for (var pipe in data.pipes)
                addPipe(data.pipes[pipe]);

            addPaths();

            matematici.elements.window.on("resize", resize);
        };

    this.initialize = function () {
        setup();
    };

    this.destroy = function () {
        matematici.elements.window.off("resize", resize);
    };

    this.getElement = function () {
        return element;
    }
};