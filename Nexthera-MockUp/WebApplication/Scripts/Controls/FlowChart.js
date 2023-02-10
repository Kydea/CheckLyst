matematici.FlowChart = function (selector, data) {
    var
        flowChart = this,
        element = $(selector),
        elements = [],
        paths = [],
        legend,

        addNode = function (pipe, nodeData) {
            var
                node = $("<div />").addClass("Node Container" + (nodeData.shape ? " " + nodeData.shape : "") + (nodeData.future ? " Future" : ""));
            node.append($("<span />").addClass("Label").html(nodeData.label));
            node.append($("<span />").addClass("Center").html(nodeData.center));

            var deadline;
            if (nodeData.deadline)
                node.append(deadline = $("<span />").addClass("Deadline").html("deadline:").append($("<span />").addClass("Value").text(nodeData.deadline)));

            if (nodeData.starting)
                node.append($("<span />").addClass("Starting Days Editable").html("starting offset:").append($("<span />").addClass("Value").text(nodeData.starting)));
            if (nodeData.duration)
                node.append($("<span />").addClass("Duration Days Editable").html("phase duration:").append($("<span />").addClass("Value").text(nodeData.duration)));
            if (nodeData.expectedDuration)
                node.append($("<span />").addClass("Duration Days Editable").html("expected duration:").append($("<span />").addClass("Value").text(nodeData.expectedDuration)));
            if (nodeData.totalDuration)
                node.append($("<span />").addClass("TotalDuration Days").html("process timeline:").append($("<span />").addClass("Value").text(nodeData.totalDuration)));
            if (nodeData.deadlineDays) {
                var span = $("<span />").addClass("DeadLine Days").html("deadline:").append($("<span />").addClass("Value").text(nodeData.deadlineDays));
                node.append(span);
                if (nodeData.totalDuration && parseInt(nodeData.deadlineDays) < parseInt(nodeData.totalDuration))
                    span.addClass("Yellow");
                else
                    span.addClass("Green");
            }

            if (nodeData.madeToStock)
                node.append($("<span />").addClass("Date StartedOn").text("made to stock"));
            else if (nodeData.startedOn === null) {
                node.append($("<span />").addClass("Date Yellow StartedOn").text("not started"));
                if (nodeData.deadline && data.currentDate > nodeData.deadline + " 23:59:59")
                    deadline.addClass("Red")
            } else if (nodeData.startedOn !== undefined) {
                node.append($("<span />").addClass("Date StartedOn").text(nodeData.startedOn));
                node.append($("<span />").addClass("Date Arrow").append($("<i />").addClass("fal fa-arrow-down")));
                if (!nodeData.completedOn) {
                    node.append($("<span />").addClass("Date Yellow CompletedOn").text("not completed"));
                    //if (nodeData.deadline && (nodeData.startedOn > nodeData.deadline + " 23:59:59" || data.currentDate > nodeData.deadline + " 23:59:59"))
                    deadline.addClass("Red")
                } else
                    node.append($("<span />").addClass("Date CompletedOn").text(nodeData.completedOn));
            }

            if (nodeData.deadline) console.log(nodeData.deadline, nodeData.startedOn, nodeData.completedOn, data.currentDate);

            pipe.append(node);
            elements[nodeData.id] = node;
        },

        addPipe = function (pipeData) {
            var pipe = $("<div />").addClass("Pipe Container");
            if (pipeData.label)
                pipe.append($("<div />").addClass("Label").text(pipeData.label))
            for (var node in pipeData.nodes)
                addNode(pipe, pipeData.nodes[node]);
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
                        pathData.sourcePosition == "l" ? sourceOffset.left - 5 :
                            pathData.sourcePosition == "r" ? sourceOffset.left + sourceSize.width + 5 :
                                sourceOffset.left + sourceSize.width * (pathData.sourceBias || .5),
                    y:
                        pathData.sourcePosition == "t" ? sourceOffset.top - 5 :
                            pathData.sourcePosition == "b" ? sourceOffset.top + sourceSize.height + 5 :
                                sourceOffset.top + sourceSize.height * (pathData.sourceBias || .5)
                },
                destinationPoint = {
                    x:
                        pathData.destinationPosition == "l" ? destinationOffset.left - 5 :
                            pathData.destinationPosition == "r" ? destinationOffset.left + destinationSize.width + 5 :
                                destinationOffset.left + destinationSize.width * (pathData.destinationBias || .5),
                    y:
                        pathData.destinationPosition == "t" ? destinationOffset.top - 5 :
                            pathData.destinationPosition == "b" ? destinationOffset.top + destinationSize.height + 5 :
                                destinationOffset.top + destinationSize.height * (pathData.destinationBias || .5)
                };

            if (pathData.sourcePosition == "r" && pathData.destinationPosition == "l" || pathData.sourcePosition == "l" && pathData.destinationPosition == "r") {
                var
                    a = pathData.sourcePosition == "r" ? sourcePoint : destinationPoint,
                    b = pathData.sourcePosition == "r" ? destinationPoint : sourcePoint;

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal " + (pathData.cssClass || "") + (pathData.strong ? " Strong" : "")).css({
                    left: a.x,
                    top: a.y,
                    width: (b.x - a.x) / 2,
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Vertical " + (pathData.cssClass || "") + (pathData.strong ? " Strong" : "")).css({
                    left: a.x + (b.x - a.x) / 2,
                    top: Math.min(a.y, b.y),
                    height: Math.abs(b.y - a.y) + (pathData.strong ? 4 : 2)
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal " + (pathData.cssClass || "") + (pathData.strong ? " Strong" : "")).css({
                    left: a.x + (b.x - a.x) / 2,
                    top: b.y,
                    width: (b.x - a.x) / 2,
                }));

                if (pathData.label)
                    paths.push($("<div />").appendTo(element).addClass("PathLabel Editable").text(pathData.label).css({
                        left: a.x + (b.x - a.x) / 2,
                        top: Math.min(a.y, b.y),
                        height: Math.abs(b.y - a.y) + 1
                    }));
            } else if (pathData.sourcePoint == "r" || pathData.sourcePoint == "l")
                alert("TODO");
            else if (pathData.sourcePosition == "b" && pathData.destinationPosition == "t" || pathData.sourcePosition == "t" && pathData.destinationPosition == "b") {
                var
                    a = pathData.sourcePosition == "b" ? sourcePoint : destinationPoint,
                    b = pathData.sourcePosition == "b" ? destinationPoint : sourcePoint;

                paths.push($("<div />").appendTo(element).addClass("Path Vertical " + (pathData.cssClass || "") + (pathData.strong ? " Strong" : "")).css({
                    left: a.x,
                    top: a.y,
                    height: (b.y - a.y) / 2,
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Horizontal " + (pathData.cssClass || "") + (pathData.strong ? " Strong" : "")).css({
                    left: Math.min(a.x, b.x),
                    top: a.y + (b.y - a.y) / 2,
                    width: Math.abs(b.x - a.x) + (pathData.strong ? 4 : 2)
                }));

                paths.push($("<div />").appendTo(element).addClass("Path Vertical " + (pathData.cssClass || "") + (pathData.strong ? " Strong" : "")).css({
                    left: b.x,
                    top: a.y + (b.y - a.y) / 2,
                    height: (b.y - a.y) / 2,
                }));

                if (pathData.label)
                    paths.push($("<div />").appendTo(element).addClass("PathLabel Editable").text(pathData.label).css({
                        left: a.x + (b.x - a.x) / 2,
                        top: Math.min(a.y, b.y),
                        height: Math.abs(b.y - a.y) + 1
                    }));
            } else if (pathData.sourcePoint == "r" || pathData.sourcePoint == "l")
                alert("TODO")

            paths.push($("<div />").appendTo(element).addClass("Arrow To" + pathData.destinationPosition.toUpperCase() + (pathData.cssClass ? " " + pathData.cssClass : "") + (pathData.strong ? " Strong" : "")).css({
                left: destinationPoint.x,
                top: destinationPoint.y,
            }));
        },

        addPaths = function () {
            for (var path in data.paths)
                addPath(data.paths[path]);

        },

        clearPaths = function () {
            for (var path in paths)
                paths[path].remove();
        },

        addLegend = function () {
            legend = $("<div />").addClass("Legend");
            element.append(legend)
            if (data.legend) 
                for (var itemIndex in data.legend.items) {
                    var item = data.legend.items[itemIndex];
                    legend.append($("<div />").append($("<span />").addClass("Key").text(item.key + ":")).append($("<span />").addClass("Value").text(item.value)));
                }
            legend.append($("<div />").append($("<span />").addClass("Key").append($("<i />").addClass("fal fa-arrow-right"))).append($("<span />").addClass("Value").text("simple phase transition")));
            legend.append($("<div />").append($("<span />").addClass("Key").append($("<i />").addClass("fa fa-arrow-right"))).append($("<span />").addClass("Value").text("matherial shipping")));
            legend.append($("<div />").append($("<span />").addClass("Key").append($("<i />").addClass("fa fa-minus")).append(" ").append($("<i />").addClass("fa fa-arrow-right"))).append($("<span />").addClass("Value").text("same center")));
            if (data.currentDate)
                legend.append($("<a />").addClass("Button RedButton").text("reschedule").append($("<i />").addClass("Icon fa fa-arrow-right")));
            
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

            addLegend();

            matematici.elements.window.on("resize", resize);
            setTimeout(resize, 100);
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