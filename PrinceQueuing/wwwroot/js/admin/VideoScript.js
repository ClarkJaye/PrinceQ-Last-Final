$(document).ready(function () {
    getAllVideos();

    //Delete Video
    $("#deleteBtn").on("click", deleteVideo);
    //Add A Video
    $("#addVideoBtn").on("click", uploadVideo);
    //Play The Video
    $("#playVideoBtn").on("click", playVideo);


    // Add click event handler to each video element
    $(document).on("click", ".videoInList", function () {
        // Remove the 'selected-video' class from all video elements
        $(".videoInList").removeClass("selected-video");

        // Add the 'selected-video' class to the clicked element
        $(this).addClass("selected-video");

        let videoSrc = $(this).data('video-src');
        let fileName = videoSrc.split('/').pop();
        $('#selected-video').attr('src', videoSrc);
        $('#selected-video').attr('vid-selected', fileName);
    });
});
function getAllVideos() {
    $.ajax({
        type: 'GET',
        url: '/Admin/AllVideos',
        dataType: 'json',
        success: function (response) {
            if (response) {
                let videoContainer = '';
                var videos = response.result.videoFiles;
                videos.forEach(function (videoFile, index) {
                    let fileName = videoFile.split('/').pop();

                    let vidName = videoFile.split("\\");
                    let name = vidName[vidName.length - 1];
                    let nameHead = name.split('.').slice(0, -1).join('.');

                    videoContainer += `
                        <div class="video-player videoInList mb-2" data-video-src="${videoFile}" data-video-name="${nameHead}">
                            <p class="VideoHead text-white d-none text-capitalize" style="position:absolute; top: 5px; left: 5px; z-index: 100; letter-spacing: 3px; font-size: 18px;">${nameHead}</p>
                            <video id="main-video-${index}" data-v width="940" style="width:100%; height:auto; position:relative;" src="${videoFile}" data-video-id="${fileName}" muted></video>
                        </div>
                    `;
                });
                $('#listVidsContainer').html(videoContainer);

                // Add hover event handler to each video element
                $('.videoInList').hover(function () {
                    $(this).find('p.VideoHead').removeClass('d-none');
                }, function () {
                    $(this).find('p.VideoHead').addClass('d-none');
                });

                // Add click event handler to each video element
                $('.videoInList').on("click", function () {
                    let videoSrc = $(this).data('video-src');
                    let fileName = videoSrc.split('/').pop();
                    $('#selected-video').attr('src', videoSrc);
                    $('#selected-video').attr('vid-selected', fileName);
                });
            } else {
                $('#listVidsContainer').addClass("text-center py-4 border-bottom fst-italic").html('<h4 class="text-warning">No Video</h4>');
            }
        },
        error: function (error) {
            console.log("Unable to get the data - " + error);
        }
    });
}

function getSelectedVideoName() {
    var videoName = $('#selected-video').attr('vid-selected');
    return videoName ? videoName.replace(/^[\\/]/, '').replace(/\\/g, '/') : null;
}


function deleteVideo() {
    var videoName = getSelectedVideoName();
    //console.log(videoName)
    if (videoName) {
        Swal.fire({
            title: "Are you sure?",
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, delete it!"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    type: 'POST',
                    url: '/Admin/DeleteVideo',
                    data: { videoName: videoName },
                    success: function (response) {
                        getAllVideos();

                        $('#selected-video').attr('src', '');
                        $('#selected-video').removeAttr('vid-selected');
                        $(`.videoInList[data-video-src='${videoName}']`).remove();

                        if (response.isSuccess) {
                            toastr.success(response.message);
                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function (error) {
                        console.log("Error deleting video - " + error);
                    }
                });
            }
        });

    } else {
        alert("No video selected")
    }
}

function uploadVideo() {
    var videoFile = $("input[name='videoFile']")[0].files[0];
    if (videoFile) {
        var formData = new FormData();
        formData.append("videoFile", videoFile);

        $.ajax({
            type: 'POST',
            url: '/Admin/UploadVideo',
            data: formData,
            processData: false, 
            contentType: false, 
            success: function (response) {
                getAllVideos();
                $("#videoInput").val('');
                $('#selected-video').attr('src', '');
                $('#selected-video').removeAttr('vid-selected');
                $(`.videoInList[data-video-src='${videoName}']`).remove();
                if (response.isSuccess) {
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (error) {
                console.log("Error uploading video - " + error);
            }
        });
    } else {
        alert("No video file selected");
    }
}

function playVideo() {
    var videoName = getSelectedVideoName();
    //console.log(videoName)
    if (videoName) {
        $.ajax({
            type: 'GET',
            url: '/Admin/PlayVideo',
            data: { videoName: videoName },
            success: function (response) {
                console.log(response)
                if (response.isSuccess) {
                    Swal.fire({
                        title: "Success!",
                        text: response.message,
                        icon: "success"
                    });
                }
            },
            error: function (error) {
                console.log("Error deleting video - " + error);
            }
        });
    } else {
        alert("No video selected")
    }
}