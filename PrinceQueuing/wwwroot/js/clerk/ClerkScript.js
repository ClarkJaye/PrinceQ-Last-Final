$(document).ready(function () {
    var userDbld = $("#isDisabledUser").val()
    // Show User Disabled modal if userIsDisabled is true
    if (userDbld) {
        $('#notificationModal').modal('show');
    }
 
    var mainNav = $('#mainNav');
    var showNavBtn = $('#showNavBtn');
    mainNav.show();
    showNavBtn.on("click", function () {
        if (mainNav.is(':visible')) {
            mainNav.slideUp();
            showNavBtn.text('Show');
        } else {
            mainNav.slideDown();
            showNavBtn.text('Hide');
        }
    });

    GetClerkNumber();
    //Display Current Serve
    DisplayCurrentServe();

    //LOAD All Queuenumbers Count on each Category
    GetAllQueueCountNumbers();
    // Add Total Cheque
    $("#UpdateQueueForm").on("submit", function (e) {
        UpdateQueueTotalCheques(e);
    });

    // Bind click event to the Done 
    $('#callBtn').on("click", CallQueue);
    // Bind click event to the Reserve
    $('#reserveBtn').on("click", ReserveQueue);
    // Bind click event to the Cancel
    $('#cancelBtn').on("click", CancelQueue);
    // Bind click event to the Serve Queue in  Reserve Table
    $('#resQueues').on("click", ".serveReserveQueueBtn", ServeQueueInReserveTable);
    // Bind click event to the Cancel Queue in  Reserve Table
    $('#resQueues').on("click", ".cancelReserveQueueBtn", CancelQueueFromTable);

    $('#chequeContainer').on("click", "#chequeBtn", function () {
        getQueueForModalCheque()
    });
});

//Date and Time
const displayDateTime = () => {
    var currentTime = new Date();
    var options = { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true };
    var formattedTime = currentTime.toLocaleString('en-US', options);
    var optionsDate = { year: 'numeric', month: 'long', day: 'numeric' };
    var formattedDate = currentTime.toLocaleString('en-US', optionsDate);

    $('#time').text(formattedTime);
    $('#date').text(formattedDate);
};
// Initial display
displayDateTime();
setInterval(displayDateTime, 1000);
// Display Current Serve
function DisplayCurrentServe() {
    var servingDisplay = document.getElementById("servingDisplay");
    var servingDisplayStage = document.getElementById("servingDisplayStageName");
    var chequeContainer = document.getElementById("chequeContainer");

    $.ajax({
        type: 'GET',
        url: "/Clerk/GetServings",
        dataType: "json",
        success: function (response) {
            if (response && response.queue) {
                var queue = response.queue;
                var categoryId = queue.categoryId;
                var queueNumber = queue.queueNumber;
                var stageId = queue.stageId;
                var totalCheques = queue.total_Cheques;

                var categoryLetter = getCategoryLetter(categoryId);
                var displayText = categoryLetter ? `${categoryLetter} - ${queueNumber}` : "----";
                servingDisplay.innerText = displayText;

                var stageText, stageColor;

                if (categoryId === 4) {
                    stageText = "Inquiry";
                    stageColor = "text-muted";
                } else {
                    stageText = stageId === 1 ? "Filling Up" : "Releasing";
                    stageColor = stageId === 1 ? "text-primary" : "text-danger";
                }

                servingDisplayStage.className = stageColor;
                servingDisplayStage.innerText = stageText;

                var existingChequeBtn = document.getElementById("chequeBtn");
                if (stageId === 2 && totalCheques === null) {
                    if (!existingChequeBtn) {
                        var chequeBtn = document.createElement("button");
                        chequeBtn.classList.add("btn", "btn-sm", "btn-primary");
                        chequeBtn.id = "chequeBtn";
                        chequeBtn.textContent = "Cheque";
                        chequeContainer.appendChild(chequeBtn);
                    } else {
                        existingChequeBtn.style.display = 'block';
                    }
                } else if (existingChequeBtn) {
                    existingChequeBtn.style.display = 'none';
                }
            } else {
                servingDisplay.innerText = "----";
                servingDisplayStage.innerText = "----";
                servingDisplayStage.className = "";
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}


// Function to Call the queue number
function CallQueue() {
    var servingDisplay = document.getElementById("servingDisplay");
    var button = $(this);
    $.ajax({
        type: 'GET',
        url: "/Clerk/CallQueueNumber",
        dataType: "json",
        success: function (response) {
            if (response && response.isSuccess == true) {
                var queue = response.obj[0];
                var categoryLetter = getCategoryLetter(queue.categoryId);
                var displayText = categoryLetter ? `${categoryLetter} - ${queue.queueNumberServe}` : "----";
                servingDisplay.innerText = displayText;

                servingDisplay.classList.add("blink-red");
                button.prop('disabled', true);
                setTimeout(function () {
                    servingDisplay.classList.remove("blink-red");
                }, 3000);
                setTimeout(function () {
                    button.prop('disabled', false);
                }, 1000);
            } else {
                button.prop('disabled', true);
                setTimeout(function () {
                    button.prop('disabled', false);
                }, 1000);
            }     
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
};

//CANCEL QUEUE
function CancelQueue() {
    var button = $(this);
    var servingDisplay = document.getElementById("servingDisplay");
    var servingDisplayStage = document.getElementById("servingDisplayStageName");
    var existingChequeBtn = document.getElementById("chequeBtn");
    $.ajax({
        type: 'GET',
        url: "/Clerk/CancelQueue",
        dataType: "json",
        success: function (response) {
            if (response && response.isSuccess == true) {
                servingDisplay.innerText = "----";
                servingDisplayStage.innerText = "----";
                servingDisplayStage.className = "";
                servingDisplayStage.style.color = "black";
                if (existingChequeBtn) {
                    existingChequeBtn.style.display = 'none';
                }
                button.prop('disabled', true);
                setTimeout(function () {
                    button.prop('disabled', false);
                }, 1000)
            }
            else {
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
};

//RESERVE QUEUE
function ReserveQueue() {
    var button = $(this);
    var servingDisplay = document.getElementById("servingDisplay");
    var existingChequeBtn = document.getElementById("chequeBtn");

    $.ajax({
        type: 'GET',
        url: "/Clerk/ReserveQueue",
        dataType: "json",
        success: function (response) {
            if (response && response.isSuccess === true) {
                servingDisplay.innerText = "----";
                if (existingChequeBtn) {
                    existingChequeBtn.style.display = 'none';
                }
                button.prop('disabled', true);
                setTimeout(function () {
                    button.prop('disabled', false);
                }, 1000)
            }
            else {
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
};
function formatDate(date) {
    var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleString('en-US', options).toUpperCase();
}

//COUNT All on Each Category and Load All Buttons
function GetAllQueueCountNumbers(id) {
    $.ajax({
        type: 'GET',
        url: "/Clerk/GetCategories",
        dataType: "json",
        success: function (response) {
            var container = $('#WaitingCountNumber');
            container.empty();
            if (response && response.isSuccess == true && response.obj.length > 0) {
                response.obj.forEach(function (item) {
                    var categoryId = item.categoryId;
                    var categoryCount = item.queueCount;
                    var category = getCategoryLetter(categoryId);
                    var description = item.category;

                    var button = $('<button>')
                        .addClass('btn nextBtn')
                        .addClass('category-' + category)
                        .attr('id', categoryId)
                        .attr('disabled', id === categoryId)
                        .append(
                            $('<h1>')
                                .addClass('categoryBtn m-0')
                                .attr('id', 'countQ' + category)
                                .text(category + ' - ' + categoryCount)
                        );

                    if (id === categoryId) {
                        button.prop('disabled', true);
                        setTimeout(() => {
                            button.prop('disabled', false);
                        }, 200);
                    }

                    var div = $('<div>')
                        .append(
                            $('<h5>')
                                .addClass('categoryName text-center')
                                //.text('Category ' + category)
                                .text(description)
                        )
                        .append(button);

                    container.append(div);
                });

            } else {
                container.append('<p>There are no category assign yet.</p>');
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}


///////////////////////////////

//SERVE =>  Releasing Table
function ServeQueueInTable() {
    var generateDate = $(this).data("gendate");
    var categoryId = $(this).data("category");
    var qNumber = $(this).data("qnumber");
    var callBtn = $('#callBtn');

    $.ajax({
        type: 'GET',
        url: "/Clerk/ServeQueueFromTable",
        data: {
            generateDate: generateDate,
            categoryId: categoryId,
            qNumber: qNumber,
        },
        dataType: "json",
        success: function (response) {
            var queueItem = response.obj;
            if (response.isSuccess == true && response.obj !== null) {
                DisplayCurrentServe();
                callBtn.prop('disabled', true);
                setTimeout(function () {
                    callBtn.prop('disabled', false);
                }, 1000);
            } else if (response.isSuccess == false && response.obj !== null) {
                DisplayModalCheque(queueItem);
            }
            else {
                alert(response.message)
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });

};

//SERVE =>  RESERVE Table
function ServeQueueInReserveTable() {
    var generateDate = $(this).data("gendate");
    var categoryId = $(this).data("category");
    var qNumber = $(this).data("qnumber");
    $.ajax({
        type: 'GET',
        url: "/Clerk/ServeQueueFromReserveTable",
        data: {
            generateDate: generateDate,
            categoryId: categoryId,
            qNumber: qNumber,
        },
        dataType: "json",
        success: function (response) {
            var queueItem = response.obj;
            if (response.isSuccess == true && response.obj !== null) {
                DisplayCurrentServe();
            } else if (response.isSuccess == false && response.obj !== null) {
                DisplayModalCheque(queueItem);
            }
            else {
                alert(response.message)
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });

};


//CANCEL QUEUE
function CancelQueueFromTable() {
    var id = $(this).attr('id');
    var generateDate = $(this).data("gendate");
    var categoryId = $(this).data("category");
    var qNumber = $(this).data("qnumber");

    $.ajax({
        type: 'GET',
        url: "/Clerk/CancelQueueNumber",
        data: {
            generateDate: generateDate,
            categoryId: categoryId,
            qNumber: qNumber,
        },
        dataType: "json",
        success: function (response) {
            if (response.isSuccess === true) {
                // 
            } else {
               alert(response.message)
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
};

//Helper
function getCategoryLetter(categoryId) {
    switch (categoryId) {
        case 1:
            return 'A';
        case 2:
            return 'B';
        case 3:
            return 'C';
        case 4:
            return 'D';
        default:
            return '';
    }
}

// Display the Modal of Cheque
function DisplayModalCheque(queue) {
    // Reset and clear the form
    $('#UpdateQueueForm')[0].reset(); 
    $('#Cheques').val(''); 
    $("#QueueNumber").val(''); 

    $("#generateDateCheq").val(queue.queueId)
    $("#categoryIdCheq").val(queue.categoryId)
    $("#qNumberCheq").val(queue.queueNumber)
    var qNumber = getCategoryLetter(queue.categoryId) + " - " + queue.queueNumber;
    $("#QueueNumber").val(qNumber);

    // Show the modal
    $('#chequeCountModal').modal('show');
}

function UpdateQueueTotalCheques(e) {
    e.preventDefault();
    var generateDate = $("#generateDateCheq").val();
    var categoryId = $("#categoryIdCheq").val();
    var qNumber = $("#qNumberCheq").val();
    var chequeValue = $("#Cheques").val();
    if (chequeValue) {
        $.ajax({
            url: "/Clerk/UpdateQueueNumber",
            type: "POST",
            data: {
                generateDate: generateDate,
                categoryId: categoryId,
                qNumber: qNumber,
                cheque: chequeValue
            },
            dataType: 'json',
            success: function (response) {
                console.log(response)
                if (response && response.isSuccess) {

                    toastr.success(response.message)
                    $('#chequeCountModal').modal('hide');
                }
                else {
                    alert(response.message)
                }

            },
            error: function (err) {
                console.log('Unable to fetch the data.', err);
            }

        })
    } else {
        alert("Please enter the total cheque.")
    }
   
}

//Show for Cheque Modal
function getQueueForModalCheque() {
    $.ajax({
        type: 'GET',
        url: "/Clerk/GetServings",
        dataType: "json",
        success: function (response) {
            if (response) {
                DisplayModalCheque(response.queue)
            }
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}

//Display Designated Clerk
function GetClerkNumber() {
    $.ajax({
        url: "/Clerk/DesignatedDeviceId",
        type: 'GET',
        success: function (response) {
            var clerk = response.obj;
            $('#ClerkNum').text(clerk.clerkNumber);
        },
        error: function (error) {
            console.log(error);
        }
    });
}
