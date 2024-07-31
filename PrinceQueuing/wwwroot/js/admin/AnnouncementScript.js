var dataTable;
$(document).ready(function () {
    TotalAnnouncement();
    loadAllAnnouncement();

    $("#addAnnounceForm").on("submit", function (e) {
        addAnnouncement(e);
    });
    $("#updateAnnounceForm").on("submit", function (e) {
        updateAnnouncement(e);
    });

    //Reset The form
    const userModal = document.getElementById('announcementModal');
    userModal.addEventListener('hidden.bs.modal', function (event) {
        const form = userModal.querySelector('form');
        const span = userModal.querySelectorAll('span');
        span.forEach(function (element) {
            element.textContent = "";
        });
        form.reset();
    });
});


function loadAllAnnouncement() {
    $.ajax({
        url: '/Admin/GetAllAnnouncement',
        type: 'GET',
        async: false,
        success: function (response) {
            console.log(response)
            var announceData = response.obj.map(function (value) {
                var createdAt = formatDate(new Date(value.created_At));

                // Wrap the description and make it ellipsis if it's too long
                var descriptionText = value.description;
                if (descriptionText.length > 50) {
                    descriptionText = descriptionText.substring(0, 50) + '...';
                }

                var editButton = $('<button>')
                    .addClass('btn btn-sm btn-primary d-flex align-items-center gap-1')
                    .attr('onclick', `editAnnouncement('${value.id}')`)
                    .html('<i class="lni lni-pencil mr-2"></i><span>Edit</span>');

                var deleteButton = $('<button>')
                    .addClass('btn btn-sm btn-danger d-flex align-items-center gap-1')
                    .attr('onclick', `deleteAnnouncement('${value.id}')`)
                    .html('<i class="lni lni-trash-can mr-2"></i><span>Delete</span>');

                var buttonsContainer = $('<div>').addClass('d-flex align-items-center float-end gap-2');

                buttonsContainer.append(editButton, deleteButton);
 
                return [
                    value.name,
                    descriptionText,
                    createdAt,
                    `<a class=" btn-sm ${value.isActiveId === 1 ? 'btn-success' : 'btn-danger'} custom-btn-font">${value.isActiveId === 1 ? 'Active' : 'Inactive'}</a>`,
                    buttonsContainer.prop('outerHTML')
                ];

            });

            //DATATABLE
            if (dataTable) {
                dataTable.clear().rows.add(announceData).draw();
            } else {
                dataTable = new DataTable("#announcementTable", {
                    data: announceData
                });
            }

        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }
    });

}

//Format DateTime
function formatDate(date) {
    var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleString('en-US', options).toUpperCase();
}

//ADD
function addAnnouncement(e) {
    e.preventDefault();
    $.ajax({
        url: '/Admin/AddAnnounce',
        type: 'POST',
        data: $('#addAnnounceForm').serialize(),
        success: function (response) {
            if (response.isSuccess) {
                $('#announcementModal').modal('hide');
                loadAllAnnouncement();
                TotalAnnouncement();
                toastr.success(response.message);
            } else {
                toastr.error(response.message)
            }
        },
        error: function (err) {
            console.log('Error:', err);
        }
    });
}

//EDIT
function editAnnouncement(id) {
    $.ajax({
        url: "/Admin/GetAnnouncement?id=" + id,
        type: "GET",
        dataType: 'json',
        success: function (response) {
            //console.log(response)
            var announce = response.obj1;
            $("#editAnnounceId").val(announce.id);
            $("#editName").val(announce.name);
            $("#editMessage").val(announce.description);
            $("#editCreated_At").val(formatDate(new Date(announce.created_At)));
            var isActive = announce.isActiveId === 1 ? "Active" : "InActive";
            var activeSelect = $("#EditActive");
            activeSelect.empty();

            activeSelect.append("<option selected value =" + announce.isActiveId + ">" + isActive + "</option>")

            $.each(response.obj2, function (i, data) {
                if (data.isActiveId !== announce.isActiveId) {
                    activeSelect.append('<option value=' + data.isActiveId + '>' + data.name + '</option>');
                }
            });


            $('#announcementEditModal').modal('show');
        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }

    })
}
//Update 
function updateAnnouncement(e) {
    e.preventDefault();

    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, update it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: "/Admin/UpdateAnnouncement",
                type: "POST",
                data: $('#updateAnnounceForm').serialize(),
                dataType: 'json',
                success: function (response) {
                    console.log(response)
                    if (response && response.isSuccess) {
                        loadAllAnnouncement();
                        toastr.success(response.message)
                    }
                    else {
                        toastr.error(response.message)
                    }

                    $('#announcementEditModal').modal('hide');
                },
                error: function (err) {
                    console.log('Unable to fetch the data.', err);
                }

            })
        }
    });

}

//DELETE
function deleteAnnouncement(id) {
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
                url: "/Admin/DeleteAnnouncement?id=" + id,
                type: 'DELETE',
                success: function (data) {
                    if (data && data.isSuccess) {
                        TotalAnnouncement();
                        loadAllAnnouncement();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

//Total Announcement
function TotalAnnouncement() {
    $.ajax({
        type: 'GET',
        url: '/Admin/GetTotalAnnounce',
        success: function (response) {
            var totalAnnounce = document.getElementById("announcementCount");
            if (response) {
                totalAnnounce.textContent = response.obj;
            } else {
                totalAnnounce.textContent = 0;
            }
        }
    })
}