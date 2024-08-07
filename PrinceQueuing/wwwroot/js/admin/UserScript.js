var dataTable;
$(document).ready(function () {
    loadAllUsers();
    LoadAllCard();

    $("#addUserBtn").on("click", function () {
        fetchRoles();
    });

    $("#AddUserForm").on("submit", function (e) {
        addUsers(e);
    });

    $("#UpdateUserForm").on("submit", function (e) {
        updateUser(e);
    });

    $("#addAssignBtn").on("click", addAssign);

    $("#categoryAssignContainer").on("click", "#removeAssignBtn", removeAssign);


    //Reset The form Add
    const userModal = document.getElementById('userAddModal');
    userModal.addEventListener('hidden.bs.modal', function (event) {
        //const form = userModal.querySelector('form');
        //const span = userModal.querySelectorAll('span');
        //span.forEach(function (element) {
        //    element.textContent = "";
        //});
        //form.reset();
        window.location.reload();
    });
});


function fetchRoles() {
    $.ajax({
        type: 'GET',
        url: '/Admin/GetAllRoles',
        dataType: 'json',
        success: function (response) {
            var roles = response.obj;

            var roleSelect = $('#AddRoleMultipleSelect');
            roleSelect.empty();

            const selectOptions = roles.map(role => ({
                label: role.name,
                value: role.id,
            }));

            selectOptions.forEach(option => {
                const optionElement = new Option(option.label, option.value);
                roleSelect.append(optionElement);
            });

            VirtualSelect.init({
                ele: '#AddRoleMultipleSelect',
                options: selectOptions,
                hideClearButton: false,
            });


            $('#userAddModal').modal('show');
        },
        error: function () {
            console.log('Unable to fetch roles.');
        }
    });
}


//Format DateTime
function formatDate(date) {
    var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleString('en-US', options).toUpperCase();
}
//Load Tables
function loadAllUsers() {
    $.ajax({
        url: '/admin/GetAllUsers',
        type: 'GET',
        async: false,
        success: function (response) {
            console.log(response)
            var categoryData = response.obj.map(function (value) {
                var formattedDateTime = formatDate(new Date(value.created_At));

                var assignButton = $('<button>')
                    .addClass('btn btn-sm btn-warning d-flex align-items-center gap-1')    
                    .attr('onclick', `assignUser('${value.id}')`)
                    .html('<i class="lni lni-checkmark-circle mr-2"></i><span>Assign</span>');

                var editButton = $('<button>')
                    .addClass('btn btn-sm btn-primary d-flex align-items-center gap-1')
                    .attr('onclick', `editUser('${value.id}')`)
                    .html('<i class="lni lni-pencil mr-2"></i><span>Edit</span>');

                var removeButton = $('<button>')
                    .addClass('btn btn-sm btn-danger d-flex align-items-center gap-1')
                    .attr('onclick', `removeUser('${value.id}')`)
                    .html('<i class="lni lni-trash-can mr-2"></i><span> Delete </span>');

                var buttonsContainer = $('<div>').addClass('w-100 d-flex align-items-center justify-content-center float-end gap-2');

                buttonsContainer.append(assignButton, editButton, removeButton);

                // Determine the roles display
                var rolesDisplay;
                if (Array.isArray(value.roles)) {
                    //if (value.roles.length > 3) {
                    //    rolesDisplay = value.roles.slice(0, 3).join(', ') + '...';
                    //} else {
                    //    rolesDisplay = value.roles.join(', ');
                    //}
                    rolesDisplay = value.roles.join(', ');
                } else {
                    rolesDisplay = '';
                }

                return [
                    value.userName,
                    value.email,
                    rolesDisplay,
                    `<a class=" btn-sm ${value.isActiveId === 1 ? 'btn-success' : 'btn-danger'} custom-btn-font">${value.isActiveId === 1 ? 'Active' : 'Inactive'}</a>`,
                    formattedDateTime,
                    buttonsContainer.prop('outerHTML')
                ];
              
            });
            //DATATABLE
            if (dataTable) {
                dataTable.clear().rows.add(categoryData).draw();
            } else {
                dataTable = new DataTable("#categoryTable", {
                    data: categoryData
                });
            }

        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }
    });
}
//ADD
function addUsers(e) {
    e.preventDefault();

    var roles = $('#AddRoleMultipleSelect').val();

    var rolesString = roles ? roles.join(',') : '';

    var formData = $('#AddUserForm').serializeArray();
    formData.push({ name: 'roles', value: rolesString }); 

    $.ajax({
        url: '/admin/AddUser',
        type: 'POST',
        data: $.param(formData), 
        success: function (response) {
            if (response && response.isSuccess === true) {
                loadAllUsers();
                LoadAllCard();
                toastr.success(response.message);
                $('#userAddModal').modal('hide');
            } else if (response && response.isSuccess === false) {

            }
            else {
                toastr.error(response.message);
            }
        },
        error: function (err) {
            console.log('Error:', err);
        }
    });
}
// EDIT
function editUser(id) {
    $.ajax({
        url: "/admin/GetUser?id=" + id,
        type: "GET",
        dataType: 'json',
        success: function (response) {
            if (response.isSuccess) {
                var user = response.user;
                var roles = response.roles;
                var userRoles = response.user.role;

                var roleSelect = $('#EditRoleMultipleSelect');
                var activeSelect = $('#EditActive');
                roleSelect.empty();
                activeSelect.empty();

                var activeText = user.isActive == 1 ? "Active" : "Inactive";
                activeSelect.append('<option selected value=' + user.isActive + '>' + activeText + '</option>');
                $.each(user.active, function (i, data) {
                    if (data.name.toLowerCase() !== activeText.toLowerCase()) {
                        activeSelect.append('<option value=' + data.isActiveId + '>' + data.name + '</option>');
                    }
                });

                $("#EditUserId").val(user.id);
                $("#EditUserName").val(user.userName);
                $("#EditEmail").val(user.email);

                const selectOptions = roles.map(role => ({
                    label: role.name,
                    value: role.id,
                    selected: userRoles.includes(role.name) 
                }));
                selectOptions.forEach(option => {
                    const optionElement = new Option(option.label, option.value);
                    optionElement.selected = option.selected;
                    roleSelect.append(optionElement);
                });

                VirtualSelect.init({
                    ele: '#EditRoleMultipleSelect',
                    options: selectOptions,
                    hideClearButton: false,
                });

                LoadAllCard();

                var modal = $('#userEditModal');
                modal.modal('show');
                modal.on('hidden.bs.modal', function () {
                    window.location.reload();
                });

            } else {
                alert(response.message);
            }
        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }
    });
}

// UPDATE
function updateUser(e) {
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
            var roles = $('#EditRoleMultipleSelect').val();

            var rolesString = roles ? roles.join(',') : '';

            var formData = $('#UpdateUserForm').serializeArray(); 
            formData.push({ name: 'roles', value: rolesString }); 

            $.ajax({
                url: "/admin/updateUser",
                type: "POST",
                data: $.param(formData), 
                dataType: 'json',
                success: function (response) {
                    if (response && response.isSuccess === true) {
                        loadAllUsers();
                        LoadAllCard();
                        toastr.success(response.message);
                        $('#userEditModal').modal('hide');
                    } else {
                        alert(response.message);
                    }
                },
                error: function (err) {
                    console.log('Unable to update the data.', err);
                }
            });
        }
    });
}

//REMOVE
function removeUser(id) {
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
                type: 'DELETE',
                url: "/Admin/RemoveUser?id=" + id,
                dataType: 'json',
                success: function (response) {
                    if (response) {
                        loadAllUsers();
                        LoadAllCard();
                        toastr.success(response.message);
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function () {
                    console.log('Unable to get the data.');
                }
            })
        }
    });
}

//GET AssignCategories
function getAssignCategories(id) {
    $.ajax({
        type: 'GET',
        url: '/Admin/GetAssignCategories',
        data: { userId: id },
        dataType: 'json',
        contentType: 'application/json',
        success: function (response) {
            //Assign 
            var userCategories = response.userCategories;
            var assignCont = $("#categoryAssignContainer");

            // Clear the container before adding new content
            assignCont.empty();

            if (userCategories.length > 0) {
                var contentRows = userCategories.map(function (item) {
                    return createList(item)
                });
                assignCont.append(contentRows);
            } else {
                var noDataRow = $("<li>").append($("<p class='p-3 m-0 text-center text-muted'>").text("No category assign yet."));
                assignCont.append(noDataRow);
            }
        },
        error: function () {
            console.log('Unable to get the data.');
        }
    });
}

//HELPER for Assign
function createList(data) {
    var li = $("<li>").addClass("list-group-item d-flex justify-content-between align-items-center");
    var p = $("<p>").addClass("m-0").text(data.category.categoryName);
    var removeBtn = $("<button id='removeAssignBtn'>").addClass("btn btn-sm btn-danger")
        .attr("data-categoryId", data.categoryId)
        .attr("data-userId", data.userId)
        .text("Remove");

    li.append(p, removeBtn);
    return li;
}
// ASSIGN
function assignUser(id) {
    $.ajax({
        type: 'GET',
        url: '/Admin/UserCategories',
        data: { userId: id },
        dataType: 'json',
        contentType: 'application/json',
        success: function (response) {
            var categories = response.categories;
            var user = response.user;
            var userCategories = response.userCategories;
            $('#assignUserId').val(user.id);
            $('#assignUserName').val(user.userName);
            $('#assignEmail').val(user.email);

            getAssignCategories(user.id);

            const selectOptions = categories.map(categoryItem => ({
                label: categoryItem.categoryName,
                value: categoryItem.categoryId,
                checked: userCategories.some(userCat => userCat.categoryId === categoryItem.categoryId)
            }));

            var selectElement = document.getElementById('multipleSelect');
            selectElement.innerHTML = '';

            selectOptions.forEach(option => {
                const optionElement = new Option(option.label, option.value);
                optionElement.selected = option.checked;
                selectElement.appendChild(optionElement);
            });

            VirtualSelect.init({
                ele: '#multipleSelect',
                options: selectOptions,
                hideClearButton: true,
                maxWidth: '400px',
            });

            var modal = $('#assignModal');
            modal.modal('show');
            modal.on('hidden.bs.modal', function () {
                window.location.reload();
            });

        },
        error: function () {
            console.log('Unable to get the data.');
        }
    });
}
//ADD ASSIGN
function addAssign() {
    var categoryId = $('#multipleSelect').val();
    var userId = $('#assignUserId').val();

    if (categoryId.length === 0) {
        alert('Please select at least one category.');
        return; 
    }

    $.ajax({
        type: 'POST',
        url: '/Admin/AddAssignUserCategories',
        data: { categoryId, userId },
        dataType: 'json',
        success: function (response) {
            if (response) {
                getAssignCategories(userId);
                toastr.success(response.message);
            } else {
                toastr.error(response.message);
            }
        },
        error: function () {
            console.log('Unable to get the data.');
        }
    });
}
//REMOVE ASSIGN
function removeAssign() {
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
            var categoryId = $(this).data("categoryid");
            var userId = $(this).data("userid");
            $.ajax({
                type: 'POST',
                url: '/Admin/RemoveAssignUserCategories',
                data: { categoryId, userId },
                dataType: 'json',
                success: function (response) {
                    if (response) {
                        getAssignCategories(userId);
                        toastr.success(response.message);
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function () {
                    console.log('Unable to get the data.');
                }
            })
        }
    });
}
function LoadAllCard() {

    $.ajax({
        url: "/admin/GetUserCounts",
        type: "GET",
        dataType: 'json',
        success: function (response) {
            $("#countAllUsers").text(response.totalUsers);
            $("#countActive").text(response.activeUsers);
            $("#countInactive").text(response.inactiveUsers);
        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }
    })

}