//Create Connection
var connectionQueueHub = new signalR.HubConnectionBuilder().withUrl("/PrinceQ.DataAccess/hubs/queueHub").build();

//From Generate a Number By Register Personnel
connectionQueueHub.on('UpdateQueueFromPersonnel', () => {
    GetAllQueueCountNumbers();
});

//Next Queuenumber
connectionQueueHub.on('UpdateQueue', (id) => {
    GetAllQueueCountNumbers(id);
});

connectionQueueHub.on("fillingQueue", function () {
    GetAllFillingQueue();
});
connectionQueueHub.on("releasingQueue", function () {
    GetAllRleasingQueue();
});

// Put Queuenumber In Reserved
connectionQueueHub.on("reservedQueue", function () {
    GetAllReserveQueue();
});

//Serve the Queue in Reserve
connectionQueueHub.on("servedInReservedQueue", () => {
    GetAllReserveQueue();
});
//Cancel the Queue in Reserve
connectionQueueHub.on("cancelreservedqueue", () => {
    GetAllReserveQueue();
});

//Serving Display For Clerk
connectionQueueHub.on("ServeInReservedQueue", () => {
     GetAllReserveQueue();
})
//End

//CancelQueue of that User
connectionQueueHub.on("cancelQueueInMenu", () => {
    var servingDisplay = document.getElementById("servingDisplay");
    servingDisplay.innerText = "----";
});

//For that User
connectionQueueHub.on("DisplayQueue", function () {
    DisplayCurrentServe();
});
connectionQueueHub.on("removeCheqBtn", function () {
    $("#chequeBtn").remove();
});

//For Dashboard User
connectionQueueHub.on("RecentServing", function () {
    GetAllRecentQueue();
});

connectionQueueHub.on("updateGenerateCard", function () {
    TotalGeneratedNumber();
});
connectionQueueHub.on("updateServingCard", function () {
    TotalQueueServe();
});
connectionQueueHub.on("updateReservedCard", function () {
    TotalReserveNumber();
});
connectionQueueHub.on("updateCancelCard", function () {
    TotalCancelNumber();
});

//For TV Display

//Speech / Sound
function speak(text) {
    const synth = window.speechSynthesis;
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.rate = 0.6;

    synth.speak(utterance);
}
function playBackgroundMusic() {
    const audio = new Audio('/src/audio/ascend.mp3');
    audio.play();
}

// Call QueueNumber in Monitor 
connectionQueueHub.on("CallQueueNumber", function (categoryId, queueNumber, clerkNumber) {
    var clerkNum = clerkNumber ? clerkNumber.replace("Clerk ", "").trim() : null;
    var display = document.getElementById('TVClerk' + clerkNum);
    var category = categoryId;

    if (display) {
        playBackgroundMusic()

        var categoryLetter = getCategoryLetter(category);
        var queueNumberServe = queueNumber !== null ? queueNumber : "----";
        display.innerText = categoryLetter + " - " + queueNumberServe;
        display.classList.add("blink-red");

        setTimeout(function () {
            display.classList.remove("blink-red");
        }, 3000);

        setTimeout(function () {
            var getQ = getCategoryLetter(category);
            var queue = getQ + "-- " + queueNumber;

            var clerk = "Clerk-" + clerkNum;
            var speechText = queue + " Please Proceed to " + clerk;
            speak(speechText);
        }, 1000);
    } else {
        servingDisplay.innerText = "----";
    }
});


connectionQueueHub.on("CutOff", () => {
    cutOffAnnounce();
});



//For TV Display Video
connectionQueueHub.on("DisplayVideo", () => {
    DisplayVideo();
})
//For TV Display Announcement
connectionQueueHub.on("LoadAnnouncement", () => {
    loadAnnouncement();
})

//Display In TV to Remove
connectionQueueHub.on("QueueDisplayInTvRemove", (value) => {
    var clerkNumber = value.toLowerCase();

    if (clerkNumber === 'clerk 1') {
        var display = document.getElementById('TVClerk1');
        display.innerText = "----"
    } else if (clerkNumber === 'clerk 2') {
        var display = document.getElementById('TVClerk2');
        display.innerText = "----"
    } else if (clerkNumber === 'clerk 3') {
        var display = document.getElementById('TVClerk3');
        display.innerText = "----"
    }
})

//Display In TV to Remove
connectionQueueHub.on("CallQueueInTVRed", (value) => {
    var clerkNumber = value.toLowerCase();

    var display;
    if (clerkNumber === 'clerk 1') {
        display = document.getElementById('TVClerk1');
        display.classList.add("blink-red");
    } else if (clerkNumber === 'clerk 2') {
        display = document.getElementById('TVClerk2');
        display.classList.add("blink-red");
    } else if (clerkNumber === 'clerk 3') {
        display = document.getElementById('TVClerk3');
        display.classList.add("blink-red");
    }
    setTimeout(function () {
        display.classList.remove("blink-red");
    }, 3000);
})


function fulfilled() {
    console.log("Connection Successful");
}

function rejected() {
    console.log("Connection failed");
}

// Start connection
connectionQueueHub.start().then(fulfilled, rejected);