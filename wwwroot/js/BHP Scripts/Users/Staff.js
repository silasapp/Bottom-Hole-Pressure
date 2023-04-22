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

    // selecting all staff on elps  
    $("#txtElpsStaff").ready(function () {
        var html = "";
        $("#txtElpsStaff").html("");
        $("#StaffLoader1").addClass("loader");

        $.getJSON("/Users/GetAllElpsStaff",
            function (datas) {
                $("#txtElpsStaff").append("<option disabled selected>--Select Staff--</option>");
               
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.userId + ">" + val.email + "</option>";
                    });
                $("#txtElpsStaff").append(html);
               
            });
        $("#StaffLoader1").removeClass("loader");
    });


    // Getting all roles
    $("#txtRole").ready(function () {
        var html = "";
        $("#txtRole").html("");
        $("#txtEditRole").html("");
        $.getJSON("/Helpers/GetStaffRoles",
            { "deletedStatus": false },
            function (datas) {
                $("#txtRole").append("<option disabled selected>--Select Roles--</option>");
                $("#txtEditRole").append("<option disabled selected>--Select Roles--</option>");
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.roleId + ">" + val.roleName + "</option>";
                    });
                $("#txtRole").append(html);
                $("#txtEditRole").append(html);
            });
    });


    // Getting all Location
    $("#txtLocation").ready(function () {
        var html = "";
        $("#txtLocation").html("");
        $("#txtEditLocation").html("");
        $.getJSON("/Helpers/GetLocation",
            { "deletedStatus": false },
            function (datas) {
                $("#txtLocation").append("<option disabled selected>--Select Location--</option>");
                $("#txtEditLocation").append("<option disabled selected>--Select Location --</option>");
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.locationId + ">" + val.locationName + "</option>";
                    });
                $("#txtLocation").append(html);
                $("#txtEditLocation").append(html);
            });
    });




    // get a staff in elps using staffemail
    $("#txtElpsStaff").on("change", function (event) {
        var staffemail = $("#txtElpsStaff").val().trim();
        var output = "";
        $("#StaffLoader2").addClass("loader");

        $.getJSON("/Users/GetElpsStaff",
            { "staffemail": staffemail },
            function (datas) {

                if (datas === null) {
                    $("#StaffLoader2").removeClass("loader");
                    $("#txtElpsID").val("");
                    $("#txtElpsHashID").val("");
                    $("#txtElpsEmail").val("");
                    $("#txtFirstName").val("");
                    $("#txtLastName").val("");

                } else {
                    $("#txtElpsID").val(datas.id);
                    $("#txtElpsHashID").val(datas.userId);
                    $("#txtElpsEmail").val(datas.email);
                    $("#txtFirstName").val(datas.firstName);
                    $("#txtLastName").val(datas.lastName);

                    $("#StaffLoader2").removeClass("loader");
                }
            });
    });



    // saving local staff to the system
    $("#FormCreateStaff").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to create this Staff?");

        var fileUpload = $("#txtSignature").get(0);

        var files = fileUpload.files;

        var data = new FormData();

        data.append("StaffSignature", files[0]);
        data.append("StaffID", $("#txtStaffID").val());
        data.append("Email", $("#txtElpsEmail").val());
        data.append("FirstName", $("#txtFirstName").val());
        data.append("LastName", $("#txtLastName").val());
        data.append("RoleID", $("#txtRole").val());
        data.append("FieldOfficeID", $("#txtFieldOffice").val());
        data.append("LocationID", $("#txtLocation").val());
        data.append("ElpsHashID", $("#txtElpsHashID").val());

        if (msg === true) {

            $.ajax({
                type: "POST",
                url: "/Users/CreateStaff",
                contentType: false,
                processData: false,
                data: data,
                async: false,
                beforeSend: function () {
                    $("#FormCreateStaff").addClass("Submitloader");
                },
                success: function (response) {
                    if ($.trim(response) === "Staff Created") {
                        SuccessMessage("#CreateStaffInfo", "Staff created successfully.");
                        $("#FormCreateStaff")[0].reset();

                        StaffTables.ajax.reload();
                        $("#FormCreateStaff").removeClass("Submitloader");
                    }
                    else {
                        ErrorMessage("#CreateStaffInfo", response);
                        $("#FormCreateStaff").removeClass("Submitloader");
                    }
                },
                error: function (response) {
                    ErrorMessage("#CreateStaffInfo", response);
                    $("#FormCreateStaff").removeClass("Submitloader");
                },
                complete: function () {
                    $("#FormCreateStaff").removeClass("Submitloader");
                }

            });
        }
    });



    // geting list of all staff to table
    var StaffTables = $("#TableStaff").DataTable({
        ajax: {
            url: "/Users/GetStaffRecord",
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
            { data: "staffID", "visible": false, "searchable": false },
            
            {
                "render": function (data, type, row) {
                    return "<span>" + row["firstName"] + " " + row["lastName"] + " </span>";
                }
            },
          
            { data: "staffEmail" },
           
            {
                "render": function (data, type, row) {
                    return "<span>" + row["fieldOffice"] + " (" + row["locationName"] + ") </span>";
                }
            },
            { data: "role" },
            
            {
                "render": function (data, type, row) {
                    var active = row['activeStatus'] === "Activated" ? "ui tiny blue label" : "ui tiny red label";
                    return "<span class=\""+active+"\">" + row["activeStatus"] + "</span>";
                }
            },
            { data: "createdAt" },
            { data: "createdBy", "searchable": false, "orderable": false},
            { data: "updatedAt" },
            { data: "signature", "searchable": false, "orderable": false },
            {
                "render": function (data, type, row) {
                    var location = window.location.origin + "/Users/StaffLogins/" + row["staffID"];
                    var loc = window.location.origin + "/Users/Activities/" + row["staffEmail"];

                    var editContent = row['staffID'] + "|" + row['staffEmail'] + "|" + row['firstName'] + "|" + row['lastName'] + "|" + row['locationId'] + "|" + row['roleId'] + "|" + row['fieldOfficeId'] + "|" + row['signaturePath'];
                    var active = row['activeStatus'] === "Activated" ? "<button class=\"btn btn-xs btn-danger status" + row["staffID"] + "\" onclick=\"StatusStaff(" + row['staffID'] + ", 'Deactivated')\"> <i class=\"fa fa-power-off\"> </i> Deactivate </button>" : "<button class=\"btn btn-warning btn-xs status" + row['staffID'] + "\" onclick=\"StatusStaff(" + row['staffID'] + ", 'Activated')\"> <i class=\"fa fa-user-o\"> </i> Activate </button>";
                    return "<button class=\"btn btn-xs btn-primary staff" + row["staffID"] + "\" id=\"" + editContent + "\" onclick=\"getStaff(" + row["staffID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <br/> " + active + " <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteStaff(" + row["staffID"] + ")\"> <i class=\"fa fa-trash\"> </i> Delete </button> <a class=\"btn btn-xs btn-success\" href=\"" + location + "\"> <i class=\"fas fa-sign-in-alt\"> </i> Logins </a> <a class=\"btn btn-xs btn-info\" href=\"" + loc + "\"> <i class=\"fas fa-route\"> </i> Activities </a>";
                }
            },
            
        ]

    });



    // editing staff info
    $("#FormEditStaff").on('submit', function (evenet) {
        evenet.preventDefault();
        var msg = confirm("Are you sure you want to edit this staff information?");

        var fileUpload = $("#txtEditSignature").get(0);

        var files = fileUpload.files;

        var data = new FormData();

        data.append("StaffSignature", files[0]);
        data.append("StaffID", $("#txtEditStaffID").val());
        data.append("FirstName", $("#txtEditFirstName").val());
        data.append("LastName", $("#txtEditLastName").val());
        data.append("RoleID", $("#txtEditRole").val());
        data.append("OfficeID", $("#txtEditFieldOffice").val());
        data.append("LocationID", $("#txtEditLocation").val());


        if (msg === true) {

            $.ajax({
                type: "POST",
                url: "/Users/EditStaff",
                contentType: false,
                processData: false,
                data: data,
                async: false,
                beforeSend: function () {
                    $("#EditForm").addClass("Submitloader");
                },
                success: function (response) {
                    if ($.trim(response) === "Staff Updated") {
                        SuccessMessage("#EditStaffInfo", "Updated successfully.");
                        $("#FormEditStaff")[0].reset();

                        StaffTables.ajax.reload();
                        $("#EditForm").removeClass("Submitloader");
                    }
                    else {
                        ErrorMessage("#EditStaffInfo", response);
                        $("#EditForm").removeClass("Submitloader");
                    }
                },
                error: function (response) {
                    ErrorMessage("#EditStaffInfo", response);
                    $("#EditForm").removeClass("Submitloader");
                },
                complete: function () {
                    $("#EditForm").removeClass("Submitloader");
                }

            });

        }
    });



    // btn to slide to create staff
    $("#btnCreateStaff").on('click', function (event) {
        event.preventDefault();
        $('html, body').animate({
            scrollTop: $("#FormEditStaff").offset().top
        }, 1000);
    });




    $("#btnCreateProcess").on('click', function (event) {
        event.preventDefault();
        $('html, body').animate({
            scrollTop: $("#FormCreateProcess").offset().top
        }, 1000);
    });




    /*
     * Out of office From datetimepicker
     */
    $("#txtOutDateFrom").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     * Out of office To datetimepicker
     */
    $("#txtOutDateTo").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });

    /*
    * Out of office From Edit datetimepicker
    */
    $("#txtEditOutDateFrom").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     * Out of office To Edit datetimepicker
     */
    $("#txtEditOutDateTo").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     * Creating Out of office module
     */
    $("#FormOutOfOffice").on('submit', function (e) {
        e.preventDefault();

        var msg = confirm("Are you sure you want to create this out of office?");

        if (msg === true) {
            $.getJSON("/OutOfOffices/CreateOutOfOffice",
                {
                    "ReliverId": $("#txtStaffs").val(),
                    "DateFrom": $("#txtOutDateFrom").val(),
                    "DateTo": $("#txtOutDateTo").val(),
                    "Comment": $("#txtOutComment").val()
                },
                function (response) {
                    if ($.trim(response) === "Out Created") {
                        SuccessMessage("#CreateOutInfo", "Updated successfully.");
                        $("#FormOutOfOffice")[0].reset();

                        outOfOfficeTable.ajax.reload();
                    }
                    else {
                        ErrorMessage("#CreateOutInfo", response);
                    }
                });
        }
    });



    /*
     * Get Staffs for out of office
     */
    $("txtStaffs").ready(function () {
        var html = "";
        $("#txtStaffs").html("");
        $("#txtEditStaffs").html("");
        $.getJSON("/Helpers/GetStaffs",
            { "deletedStatus": false },
            function (datas) {
                $("#txtStaffs").append("<option disabled selected>--Select Staff--</option>");
                $("#txtEditStaffs").append("<option disabled selected>--Select Staff --</option>");
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.staffId + ">" + val.firstName + " " + val.lastName + " (" + val.staffEmail + ") </option>";
                    });
                $("#txtStaffs").append(html);
                $("#txtEditStaffs").append(html);
            });
    });





    /*
     * Editing Out of office module
     */
    $("#FormEditOutOfOffice").on('submit', function (e) {
        e.preventDefault();

        var msg = confirm("Are you sure you want to edit this out of office?");

        if (msg === true) {
            $.getJSON("/OutOfOffices/EditOutOfOffice",
                {
                    "ReliverId": $("#txtEditStaffs").val(),
                    "DateFrom": $("#txtEditOutDateFrom").val(),
                    "DateTo": $("#txtEditOutDateTo").val(),
                    "Comment": $("#txtEditOutComment").val(),
                    "OutId": $("#EditOutID").val()
                },
                function (response) {
                    if ($.trim(response) === "Out Edited") {
                        SuccessMessage("#EditOutInfo", "Updated successfully.");
                        $("#FormEditOutOfOffice")[0].reset();

                        outOfOfficeTable.ajax.reload();
                    }
                    else {
                        ErrorMessage("#EditOutInfo", response);
                    }
                });
        }
    });


    /*
     * refersh out of office
     */
    setInterval(function () {
        outOfOfficeTable.ajax.reload();
        allOutOfOfficeTable.ajax.reload();
        relieveOutOfOfficeTable.ajax.reload();
    }, 20000);



    // geting list of out of office for a staff
    var outOfOfficeTable = $("#TableOutOfOffice").DataTable({
        ajax: {
            url: "/OutOfOffices/GetStaffOutOfOffice",
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
            { data: "outID", "visible": false, "searchable": false },
            { data: "me" },
            { data: "reliever" },
            { data: "dateFrom" },
            { data: "dateTo" },
            { data: "status" },

            { data: "createdAt" },

            { data: "updatedAt" },

            {
                "render": function (data, type, row) {

                    var editContent = row['outID'] + "|" + row['relieverID'] + "|" + row['dateFrom'] + "|" + row['dateTo'] + "|" + row['comment'];
                    var deleted = row['status'] === "STARTED" ? "<button class=\"btn btn-sm btn-primary out" + row["outID"] + "\" onclick=\"FinishedOut(" + row['outID'] + ")\"> <i class=\"fa fa-user-check\"> </i> Finished </button>" : "";
                    var edit = row['status'] === "WAITING" ? "<button class=\"btn btn-sm btn-primary editout" + row["outID"] + "\" id=\"" + editContent + "\" onclick=\"getOuts(" + row["outID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button>" : "";

                    return edit + " " + deleted + "<button class=\"btn btn-sm btn-danger\" onclick=\"DeleteOut(" + row['outID'] + ")\"> <i class=\"fa fa-trash\"> </i> Delete </button>";
                }
            }
        ]

    });




    // geting list of out of office staff
    var allOutOfOfficeTable = $("#TableAllOutOfOffice").DataTable({
        ajax: {
            url: "/OutOfOffices/GetAllStaffOutOfOffice",
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
            { data: "outID", "visible": false, "searchable": false },
            { data: "staff" },
            { data: "reliever" },
            { data: "dateFrom" },
            { data: "dateTo" },
            { data: "status" },
            {
                "render": function (data, type, row) {
                    return "<small>" + row['comment'] + "</small>";
                }
            },
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {

                    var end = row['status'] === "STARTED" ? "<button class=\"btn btn-sm btn-danger out" + row["outID"] + "\" onclick=\"FinishedOut(" + row['outID'] + ")\"> <i class=\"fas fa-user-check\"> </i> End this </button>" : "";

                    return end;
                }
            }
        ]

    });


    // geting list of out of office staff to relieve
    var relieveOutOfOfficeTable = $("#TableRelieveStaff").DataTable({
        ajax: {
            url: "/OutOfOffices/GetRelieveStaff",
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
            { data: "outID", "visible": false, "searchable": false },
            { data: "staff" },
            { data: "dateFrom" },
            { data: "dateTo" },
            { data: "status" },
            { data: "deskCount" },

            {
                "render": function (data, type, row) {
                    return "<small>" + row['comment'] + "</small>";
                }
            },
            {
                "render": function (data, type, row) {
                    var switchAccount = "<button class=\"btn btn-sm btn-primary relieve" + row["staffEmail"] + "\" onclick=\"SwitchAccount('" + row['staffEmail'] + "')\"> <i class=\"fas fa-random\"></i> </i> Switch Account </button>";
                    return switchAccount;
                }
            }
        ]

    });




    function showMyImage(fileInput, fileID, error_id, image_name, img_id, img_path) {
        var files = fileInput.files;
        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            var imageType = /image.*/;
            if (!file.type.match(imageType)) {
                ErrorMessage($(error_id), "This image format is not supported.");
                $(image_name).val('');
                return false;
            }
            else if (file.size > (1048576 * 2)) { //2MB
                ErrorMessage($(error_id), "Large image size. Max of 2MB");
                $(image_name).val('');
                return false;
            }

            var img = document.getElementById(fileID);
            img.file = file;

            var reader = new FileReader();
            reader.onload = (function (aImg) {
                return function (e) {
                    aImg.src = e.target.result;
                };

            })(img);
            reader.readAsDataURL(file);
        }
    }



    $("#txtEditSignature").on('change', function () {
        showMyImage(this, "EditSignaturethumbnil", "#errorImage", "#txtEditSignature", "#EditSignaturethumbnil", "~/images/Signature/TestSignature.jpg");
    });


    $("#txtSignature").on('change', function () {
        showMyImage(this, "Signaturethumbnil", "#errorImage1", "#txtSignature", "#Signaturethumbnil", "~/images/Signature/TestSignature.jpg");
    });





});

function getStaff(id) {
    $('html, body').animate({
        scrollTop: $("#FormEditStaff").offset().top
    }, 1000);
    var staffDetails = $(".staff" + id).attr("id").split('|');

    var sign = staffDetails[7].split("~");

    $("#txtEditStaffID").val(staffDetails[0]);
    $("#txtEditElpsEmail").val(staffDetails[1]);
    $("#txtEditFirstName").val(staffDetails[2]);
    $("#txtEditLastName").val(staffDetails[3]);
    $("#txtEditLocation").val(staffDetails[4]).change();
    $("#txtEditRole").val(staffDetails[5]).change();
    $("#txtEditFieldOffice").val(staffDetails[6]).change();
    $("#EditSignaturethumbnil").attr("src", sign[1]);
}


function StatusStaff(id, Status) {
    var msg = confirm("Are you sure you want to " + Status + " this staff?");

    if (msg === true) {
        $.getJSON("/Users/DeactivateStaff", { "StaffID": id, "Status": Status }, function (response) {
            if ($.trim(response) === "Done") {
                alert("Staff " + Status + " successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}

function DeleteStaff(id) {
    
    var msg = confirm("Are you sure you want to remove this staff?");

    if (msg === true) {
        $.getJSON("/Users/RemoveStaff", { "StaffID": id }, function (response) {
            if ($.trim(response) === "Staff Removed") {
                alert("Staff Removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}



/*
 * getting out of office for editing
 */
function getOuts(id) {
    $('html, body').animate({
        scrollTop: $("#FormEditOutOfOffice").offset().top
    }, 1000);
    var Details = $(".editout" + id).attr("id").split('|');
    $("#EditOutID").val(Details[0]);
    $("#txtEditStaffs").val(Details[1]).change();
    $("#txtEditOutDateFrom").val(Details[2]);
    $("#txtEditOutDateTo").val(Details[3]);
    $("#txtEditOutComment").val(Details[4]);
}


/*
 * ending an out of office by support
 */
function FinishedOut(id) {
    var msg = confirm("Are you sure you want to end this out of office?");

    if (msg === true) {
        $.getJSON("/OutOfOffices/FinishedOut", { "OutID": id }, function (response) {
            if ($.trim(response) === "Done") {
                alert("Out of office finished successfully");
                window.location.reload(true);
            }
            else {
                alert(response);
            }
        });
    }
}


/*
 * deleting an out of office by staff
 */
function DeleteOut(id) {
    var msg = confirm("Are you sure you want to delete this out of office?");

    if (msg === true) {
        $.getJSON("/OutOfOffices/DeleteOut", { "OutID": id }, function (response) {
            if ($.trim(response) === "Done") {
                alert("Out of office deleted successfully");
                window.location.reload(true);
            }
            else {
                alert(response);
            }
        });
    }
}


/*
 * Switching account for relieve staff
 */
function SwitchAccount(email) {
    var msg = confirm("Are you sure you want to switch account to relieve this staff? \n\nYou will have to sign out and sign in back into your own account.");

    if (msg === true) {

        $.post("/OutOfOffices/SwitchAccount", { "email": email }, function (response) {
            if ($.trim(response) === "Done") {
                alert("Signing you out now, switching to " + email + " account's");
                var location = window.location.origin + "/Auth/UserAuth?email=" + email;
                window.location.href = location;
            }
            else {
                alert(response);
            }
        });
    }
}
