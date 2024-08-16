$(document).ready(function () {
    //Load all releasing Queue
    GetAllRleasingQueue();

    //LOAD all Reserve Queue
    GetAllReserveQueue();

    //LOAD all Reserve Queue
    GetAllReserveQueue();
    

    // Bind click event to the Serve Queue in  Release Table
    $('#releasingQueues').on("click", ".serveReleasingBtn", ServeQueueInTable);
    // Bind click event to the Cancel Queue in  Release Table
    $('#releasingQueues').on("click", ".cancelReleaseBtn", CancelQueueFromTable);
    // Bind click event to display the Cheque Modal 


});
function formatDate(date) {
    var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleString('en-US', options).toUpperCase();
}

//Load Reserve Table
function GetAllReserveQueue() {
    $.ajax({
        type: 'GET',
        url: "/Clerk/GetReservedQueues",
        dataType: "json",
        success: function (response) {
            var reserveQ = response.obj;
            reserveQ.sort(function (a, b) {
                return new Date(a.reserv_At) - new Date(b.reserv_At);
            });

            // Filter out queues where stageId is not 2
            reserveQ = reserveQ.filter(function (queue) {
                return queue.stageId === 2;
            });

            var tbody = $("#resQueues");
            tbody.empty();

            if (reserveQ.length > 0) {
                var tableRows = reserveQ.map(function (queue) {
                    return createTableReserve(queue);
                });
                tbody.append(tableRows);
            } else {
                var noReserveRow = $("<tr>").append($("<td colspan='4'>").text("No reservations yet"));
                tbody.append(noReserveRow);
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}

//Function Create Table
function createTableReserve(queue) {
    var queueNumber = $("<th>").addClass("h2 align-middle").text(queue.category + "-" + queue.qNumber);

    var reserve_At = new Date(queue.reserv_At);
    var formattedDateTime = formatDate(reserve_At);

    var reserveAtCell = $("<td>")
        .addClass("text-center align-middle")
        .text(formattedDateTime);

    var stageText, stageClass;

    if (queue.categId === 4) {
        stageText = "Inquiry";
        stageClass = 'text-muted';
    } else {
        stageText = "Releasing";
        stageClass = 'text-danger';
    }

    var stageCell = $("<td>").addClass("h5 align-middle " + stageClass).text(stageText);

    var actionCell = $("<td>");
    var serveButton = $("<button>")
        .addClass("btn serveReserveQueueBtn queueBtn-" + queue.category)
        .attr("data-genDate", queue.generateDate)
        .attr("data-category", queue.categId)
        .attr("data-qnumber", queue.qNumber)
        .text("Serve");
    var cancelButton = $("<button>")
        .addClass("btn btn-secondary cancelReserveQueueBtn")
        .attr("data-genDate", queue.generateDate)
        .attr("data-category", queue.categId)
        .attr("data-qnumber", queue.qNumber)
        .text("Cancel");

    actionCell.addClass("tableAction d-flex justify-content-center gap-3");
    actionCell.append(serveButton, cancelButton);

    var tableRow = $("<tr>").append(queueNumber, reserveAtCell, stageCell, actionCell);
    return tableRow;
}



//Load Releasing Table
function GetAllRleasingQueue() {
    $.ajax({
        type: 'GET',
        url: "/Clerk/GetAllReleasingQueues",
        dataType: "json",
        success: function (response) {
            var releasingQ = response.obj;
            releasingQ.sort(function (a, b) {
                return new Date(a.releasing_At) - new Date(b.releasing_At);
            });
            var tbody = $("#releasingQueues");
            tbody.empty();

            if (releasingQ.length > 0) {
                var firstRow = createTableReleasing(releasingQ[0], 0);
                tbody.append(firstRow);

                var otherRows = releasingQ.slice(1).map((q, index) => createTableReleasing(q, index + 1));
                tbody.append(otherRows);
            } else {
                var noReleasingRow = $("<tr>").append($("<td colspan='2'>").text("No releasing queues yet."));
                tbody.append(noReleasingRow);
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}

//Function Create Table for Filling Up
function createTableReleasing(queue, rowIndex) {
    var queueNumber = $("<th>").addClass("h4 align-middle w-50").text(queue.category + "-" + queue.qNumber);
    var actionCell = $("<td>");

    if (rowIndex === 0) {
        var serveReleaseButton = $("<button>")
            .addClass("btn btn-success serveReleasingBtn")
            .attr("data-genDate", queue.generateDate)
            .attr("data-category", queue.categId)
            .attr("data-qnumber", queue.qNumber)
            .text("Serve");

        serveReleaseButton.prop('disabled', true);
        setTimeout(() => {
            serveReleaseButton.prop('disabled', false);
        }, 150);

        actionCell.addClass("tableAction d-flex justify-content-center gap-3");
        actionCell.prepend(serveReleaseButton);
    } else {
        actionCell.addClass("tableAction");
    }

    var tableRow = $("<tr>").append(queueNumber, actionCell);
    return tableRow;
}

