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
    


    // Create Field Office
    $("#FormCreateFieldOffice").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtFieldOfficeName").val() === "") {
            ErrorMessage("#FieldOfficeInfo", "Please enter field office.");
        }
        else {

            Notify.confirm({
                title: 'Create Field Office',
                text: 'Are you sure you want to create this field office?',
                ok: function () {

                    $.getJSON("/FieldOffices/CreateFieldOffice", { "OfficeName": $("#txtFieldOfficeName").val() }, function (response) {
                        if ($.trim(response) === "Office Created") {
                            SuccessMessage("#FieldOfficeInfo", "Field Office created successfully");
                            $("#FormCreateFieldOffice")[0].reset();
                            FieldOfficeTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#FieldOfficeInfo", response);
                        }
                    });
                }
            });
        }
    });


    // geting list of field office
    var FieldOfficeTables = $("#TableFieldOffice").DataTable({

        ajax: {
            url: "/FieldOffices/GetFieldOffice",
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
            { data: "fieldOfficeID", "visible": false, "searchable": false },
            { data: "officeName" },
            { data: "createdAt" },
            { data: "updatedAt" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary office" + row["fieldOfficeID"] + "\" id=\"" + row['officeName'] + "\" onclick=\"getValues(" + row["fieldOfficeID"] + ")\"> <i class=\"ui edit icon\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger\" target=\"\" onclick=\"DeleteOffice(" + row["fieldOfficeID"] + ")\"> <i class=\"ui trash icon\"> </i> Delete </button>";
                }
            }

        ]

    });


    // Editing Field Office
    $("#FormEditFieldOffice").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditFieldOffice").val() === "") {
            ErrorMessage("#EditFieldOfficeInfo", "Please select a field office to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Field Office',
                text: 'Are you sure you want to edit this field office?',
                ok: function () {

                    $.getJSON("/FieldOffices/EditFieldOffice", { "OfficeName": $("#txtEditFieldOffice").val(), "FieldOfficeId": $("#txtEditFieldOfficeID").val() }, function (response) {

                        if ($.trim(response) === "Office Updated") {
                            SuccessMessage("#EditFieldOfficeInfo", "Field Office updated successfully.");
                            $("#FormEditFieldOffice")[0].reset();
                            FieldOfficeTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditFieldOfficeInfo", response);
                        }
                    });
                }
            });
        }
    });
});

/*
 * Getting Field Office info for editing
 */
function getValues(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateFieldOffice").offset().top
    }, 1000);
    var country_id = $("#txtEditFieldOfficeID").val(id);
    var office = $(".office" + id).attr("id");
    $("#txtEditFieldOffice").val(office);
}


/*
 * Removing field office.
 */
function DeleteOffice(id) {

    Notify.confirm({
        title: 'Remove Field Office',
        text: 'Are you sure you want to remove this field office?',
        ok: function () {
            $.getJSON("/FieldOffices/DeleteOffice", { "FieldOfficeID": id }, function (response) {
                if ($.trim(response) === "Office Deleted") {
                    Notify.suc({
                        title: 'Success',
                        text : 'Field office removed successfully'
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