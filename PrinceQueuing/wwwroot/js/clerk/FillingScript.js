$(document).ready(function () {
    //Load all Filling Queue
    GetAllFillingQueue();

    //LOAD all Reserve Queue
    GetAllReserveQueue();

    //LOAD all Reserve Queue
    GetAllReserveQueue();


    // Bind click event to the Next 
    $('#WaitingCountNumber').on("click", ".nextBtn", getNextQueueNumber);

    // Bind click event to Transfer the Queue from filling up table to releasing table
    $('#fillingQueues').on("click", ".toReleasingBtn", ToReleasingQueue);
    // Bind click event to the Cancel Queue in  Filling Table
    $('#fillingQueues').on("click", ".cancelFillingBtn", CancelQueueFromTable);
    // Bind click event to display the Cheque Modal 

});

// Function to get the next queue number
function getNextQueueNumber() {
    var button = $(this);
    var callBtn = $('#callBtn');
    var categoryId = button.attr('id');

    $.ajax({
        type: 'GET',
        url: "/Clerk/NextQueue",
        dataType: "json",
        data: { id: categoryId },
        success: function (response) {
            var queue = response.obj;
            if (response.isSuccess === true && response.obj !== null) {
                button.prop('disabled', true);
                callBtn.prop('disabled', true);
                setTimeout(function () {
                    button.prop('disabled', false);
                    callBtn.prop('disabled', false);
                }, 2000);
            } else if (response.isSuccess === false && response.obj !== null) {
                DisplayModalCheque(queue);
            }
            else if (response.isSuccess === true && response.obj === null) {
                alert(response.message)
            }
            else{
                button.prop('disabled', true);
                setTimeout(function () {
                    button.prop('disabled', false);
                }, 1000)
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}

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

            // Filter out queues where stageId is not 1
            reserveQ = reserveQ.filter(function (queue) {
                return queue.stageId === 1;
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

// Function Create Table
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
        stageText = "Filling Up";
        stageClass = 'text-primary';
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

//Load Filling Up Table
function GetAllFillingQueue() {
    $.ajax({
        type: 'GET',
        url: "/Clerk/GetAllFillingUpQueues",
        dataType: "json",
        success: function (response) {
            var fillingQ = response.obj;
            fillingQ.sort(function (a, b) {
                return new Date(a.filling_At) - new Date(b.filling_At);
            });
            var tbody = $("#fillingQueues");
            tbody.empty();

            if (fillingQ.length > 0) {
                var tableRows = fillingQ.map(function (queue) {
                    return createTableFilling(queue);
                });
                tbody.append(tableRows);
            } else {
                var noFillingRow = $("<tr>").append($("<td colspan='2'>").text("No filling up queues yet."));
                tbody.append(noFillingRow);
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}
//Function Create Table for Filling Up
function createTableFilling(queue) {
    var queueNumber = $("<th>").addClass("h4 align-middle w-50").text(queue.category + "-" + queue.qNumber);
    var actionCell = $("<td>");

    var toReleaseButton = $("<button>")
        .addClass("btn btn-primary toReleasingBtn")
        .attr("data-genDate", queue.generateDate)
        .attr("data-category", queue.categId)
        .attr("data-qnumber", queue.qNumber)
        .text("Releasing");
    var cancelButton = $("<button>")
        .addClass("btn btn-sm btn-secondary cancelFillingBtn")
        .attr("data-genDate", queue.generateDate)
        .attr("data-category", queue.categId)
        .attr("data-qnumber", queue.qNumber)
        .text("Cancel");


    actionCell.addClass("tableAction d-flex justify-content-center gap-3");
    //actionCell.append(toReleaseButton, cancelButton);
    actionCell.append(toReleaseButton);

    var tableRow = $("<tr>").append(queueNumber, actionCell);
    return tableRow;
}

//Transfer the Queue From Filling Up Table Table to Releasing Table
function ToReleasingQueue() {
    var generateDate = $(this).data("gendate");
    var categoryId = $(this).data("category");
    var qNumber = $(this).data("qnumber");
    $.ajax({
        type: 'GET',
        url: "/Clerk/FillingToReleaseQueue",
        data: {
            generateDate: generateDate,
            categoryId: categoryId,
            qNumber: qNumber,
        },
        dataType: "json",
        success: function (response) {
            if (response && response.isSuccess == true) {
                toastr.success(response.message)
            } else {
                toastr.error(response.message)
            }

        }, error: function (error) {

        }
    });

}

