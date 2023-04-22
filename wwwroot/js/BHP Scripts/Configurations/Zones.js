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



    // Create Zonal Office
    $("#FormCreateZonalOffice").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtZonalOfficeName").val() === "") {
            ErrorMessage("#ZonalOfficeInfo", "Please enter zonal office.");
        }
        else {
            Notify.confirm({
                title: 'Create Zonal Office',
                text: 'Are you sure you want to create this Zonal office?',
                ok: function () {
                    $.getJSON("/ZonalOffices/CreateZonalOffice", { "ZoneName": $("#txtZonalOfficeName").val() }, function (response) {
                        if ($.trim(response) === "Zone Created") {
                            SuccessMessage("#ZonalOfficeInfo", "Zonal Office created successfully");
                            $("#FormCreateZonalOffice")[0].reset();
                            ZonalOfficeTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#ZonalOfficeInfo", response);
                        }
                    });
                }
            });
        }
    });


    // geting list of zonal offices
    var ZonalOfficeTables = $("#TableZonalOffice").DataTable({

        ajax: {
            url: "/ZonalOffices/GetZonalOffice",
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
            { data: "zoneId", "visible": false, "searchable": false },
            { data: "zoneName" },
            { data: "createdAt" },
            { data: "updatedAt" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary zone" + row["zoneId"] + "\" id=\"" + row['zoneName'] + "\" onclick=\"getZone(" + row["zoneId"] + ")\"> <i class=\"ui edit icon\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger\" target=\"\" onclick=\"DeleteZone(" + row["zoneId"] + ")\"> <i class=\"ui trash icon\"> </i> Delete </button>";
                }
            }

        ]

    });


    // Editing Zonal Office
    $("#FormEditZonalOffice").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditZonalOffice").val() === "") {
            ErrorMessage("#EditZonalOfficeInfo", "Please select a zonal office to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Zonal Office',
                text: 'Are you sure you want to edit this Zonal office?',
                ok: function () {

                    $.getJSON("/ZonalOffices/EditZonalOffice", { "ZoneName": $("#txtEditZonalOffice").val(), "ZonalOfficeId": $("#txtEditZonalOfficeID").val() }, function (response) {

                        if ($.trim(response) === "Zone Updated") {
                            SuccessMessage("#EditZonalOfficeInfo", "Zonal Office updated successfully.");
                            $("#FormEditZonalOffice")[0].reset();
                            ZonalOfficeTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditZonalOfficeInfo", response);
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
function getZone(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateZonalOffice").offset().top
    }, 1000);
    var country_id = $("#txtEditZonalOfficeID").val(id);
    var state = $(".zone" + id).attr("id");
    $("#txtEditZonalOffice").val(state);
}


/*
 * Removing field office.
 */
function DeleteZone(id) {

    Notify.confirm({
        title: 'Remove Zonal Office',
        text: 'Are you sure you want to remove this Zonal office?',
        ok: function () {
            $.getJSON("/ZonalOffices/DeleteZonalOffice", { "ZonalOfficeId": id }, function (response) {
                if ($.trim(response) === "Zone Deleted") {
                    Notify.suc({
                        title: 'Success',
                        text: 'Zonal office removed successfully'
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