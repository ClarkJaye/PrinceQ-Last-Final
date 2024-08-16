$(document).ready(function () {
    //Load Serving
    DisplayServeTV();
    //Load Announcement
    loadAnnouncement();
    //Load Video
    DisplayVideo();
    //Date And Time
    displayDateTime();
    setInterval(displayDateTime, 1000);
});


//Dispaly Serve
function DisplayServeTV() {
    $.ajax({
        type: 'GET',
        url: "/Display/GetServings",
        dataType: "json",
        success: function (response) {
            response.queues.forEach(function (value) {
                if (value) {
                    var clerkNumber = value.clerkNumber ? value.clerkNumber.replace("Clerk ", "").trim() : null;
                    var display = document.getElementById('TVClerk' + clerkNumber);
                    var category = value.categoryId;

                    if (display) {
                        var categoryLetter = getCategoryLetter(category);
                        var queueNumberServe = value.queueNumberServe !== null ? value.queueNumberServe : "----";
                        display.innerText = categoryLetter + " - " + queueNumberServe;
                    }
                }
            });
        },
        error: function (req, status, error) {
            console.log(status);
        }
    });
}
function getCategoryLetter(categoryId) {
    var categories = {
        1: "A",
        2: "B",
        3: "C",
        4: "D"
    };
    return categories[categoryId] || "----";
}

function DisplayVideo() {
    $.ajax({
        type: 'GET',
        url: '/Admin/AllVideos',
        dataType: 'json',
        success: function (response) {
            var videos = response.result.videoFiles;
            var videoPlayer = document.getElementById('main-video-page2');
            var videoFiles = videos.map(file => file.replace(/\\/g, '/'));
            var videoIndex = 0;

            if (videoFiles.length > 0) {
                videoPlayer.src = videoFiles[videoIndex];
                videoPlayer.play();
            }
            videoPlayer.addEventListener('ended', function () {
                videoIndex++;
                if (videoIndex >= videoFiles.length) {
                    videoIndex = 0;
                }
                videoPlayer.src = videoFiles[videoIndex];
                videoPlayer.play();
            });
        },
        error: function (error) {
            console.log("Error loading videos: ", error);
        }
    });
}

function loadAnnouncement() {
    $.ajax({
        type: 'GET',
        url: '/Display/GetAnnouncement',
        dataType: 'json',
        success: function (response) {
            var announceContainer = document.getElementById('announcementWord');
            if (response.isSuccess) {
                announceContainer.textContent = response.announce.description;
            } else {
                announceContainer.textContent = "Visit Our Page!"
            }

        },
        error: function (error) {
            console.log("Error: ", error);
        }
    });
}

const displayDateTime = () => {
    var currentTime = new Date();
    var options = { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true };
    var formattedTime = currentTime.toLocaleString('en-US', options);
    var optionsDate = { year: 'numeric', month: 'long', day: 'numeric' };
    var formattedDate = currentTime.toLocaleString('en-US', optionsDate);

    $('#time').text(formattedTime);
    $('#date').text(formattedDate);
};