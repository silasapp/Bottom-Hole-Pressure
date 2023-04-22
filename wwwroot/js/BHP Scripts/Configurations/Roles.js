$(function () {

    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
            .html("<i class=\"fa fa-warning text-danger\"> </i> <span class=\"text-danger\"> " + error_message + " </span>")
            .delay(9000)
            .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
            .html("<i class=\"fa fa-check-circle text-success\"> </i> <span class=\"text-success\">" + success_message + " </span>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }



    // Create roles
    $("#FormCreateRole").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtRoleName").val() === "") {
            ErrorMessage("#CreateRoleInfo", "Please enter a role");
        }
        else {
            Notify.confirm({
                title: 'Create Role',
                text: 'Are you sure you want to create this role?',
                ok: function () {
                    $.post("/UserRoles/CreateRoles", { "RoleName": $("#txtRoleName").val() }, function (response) {
                        if ($.trim(response) === "Role Created") {
                            SuccessMessage("#CreateRoleInfo", "Role created successfully");
                            $("#FormCreateRole")[0].reset();
                            RoleTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateRoleInfo", response);
                        }
                    });
                }
            });
        }
    });


    // geting list of roles
    var RoleTables = $("#TableRoles").DataTable({

        ajax: {
            url: "/UserRoles/GetRoles",
            type: "POST",
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
        },

        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5',
            {
                extend: 'print',
                text: 'Print all',
                exportOptions: {
                    modifier: {
                        selected: null
                    }
                }
            },
            {
                extend: 'colvis',
                collectionLayout: 'fixed two-column'
            }

        ],

        language: {
            buttons: {
                colvis: 'Change columns'
            }
        },

        "processing": true,
        "serverSide": true,
        stateSave: true,

        columns: [
            { data: "roleID" },
            { data: "roleName" },
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary role" + row["roleID"] + "\" id=\"" + row['roleName'] + "\" onclick=\"getRole(" + row["roleID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger\" target=\"\" onclick=\"DeleteRole(" + row["roleID"] + ")\"> <i class=\"fa fa-trash\"> </i> Delete </button>";
                }
            }
        ],

        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            },
            {
                "targets": [1],
                "searchable": true,
                "orderable": true
            },
            {
                "targets": [2],
                "orderable": true
            },
            {
                "targets": [3],
                "orderable": true
            }

        ]
    });


    // Editing Role
    $("#FormEditRole").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditRoleID").val() === "") {
            ErrorMessage("#EditRoleInfo", "Please select a role to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Role',
                text: 'Are you sure you want to edit this role?',
                ok: function () {
                    $.getJSON("/UserRoles/EditRole", { "RoleName": $("#txtEditRoleName").val(), "RoleID": $("#txtEditRoleID").val() }, function (response) {
                        if ($.trim(response) === "Role Updated") {
                            SuccessMessage("#EditRoleInfo", "Role updated successfully.");
                            $("#FormEditRole")[0].reset();
                            RoleTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditRoleInfo", response);
                        }
                    });
                }
            });
        }
    });
});

// getting roles for editing
function getRole(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateRole").offset().top
    }, 1000);
    var role_id = $("#txtEditRoleID").val(id);
    var role = $(".role" + id).attr("id");
    $("#txtEditRoleName").val(role);
}

/*
 * Removing role.
 */
function DeleteRole(id) {
    Notify.confirm({
        title: 'Remove Role',
        text: 'Are you sure you want to remove this role?',
        ok: function () {
            $.getJSON("/UserRoles/DeleteRole", { "RoleID": id }, function (response) {
                if ($.trim(response) === "Role Deleted") {
                    Notify.suc({
                        title: 'Success',
                        text: 'Role removed successfully'
                    });
                    window.location.reload();
                }
                else {
                    Notify.alert({
                        title: 'Failure',
                        text: response
                    });
                }
            });
        }
    });
}