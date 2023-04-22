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



    // selecting all field office 
    $("#txtFieldOffice").ready(function () {
        var html = "";

        $("#txtFieldOffice").html("");
        $("#txtEditFieldOffice").html("");

        $.getJSON("/Helpers/GetAllFieldOffice",
            { "deletedStatus": false },
            function (datas) {
                $("#txtFieldOffice").append("<option disabled selected>--Select Field Office--</option>");
                $("#txtEditFieldOffice").append("<option disabled selected>--Select Field Office--</option>");

                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.fieldOfficeId + ">" + val.officeName + "</option>";
                    });
               
                $("#txtFieldOffice").append(html);
                $("#txtEditFieldOffice").append(html);
            });
    });


    // Create Zone => field office relationship

    $("#FormCreateZoneFieldOffice").on('submit', function (event) {

        event.preventDefault();

        Notify.confirm({
            title: 'Create Zone and Field Office',
            text: 'Are you sure you want to create this Zone => field office relationship?',
            ok: function () {
                $.getJSON("/ZoneFieldOffices/CreateZoneFieldOffice", { "ZoneID": $("#txtZones").val(), "FieldOfficeID": $("#txtFieldOffice").val() }, function (response) {
                    if ($.trim(response) === "ZoneFieldOffice Created") {
                        SuccessMessage("#ZoneFieldOfficeInfo", "Zone => Field Office relationship created successfully");
                        $("#FormCreateZoneFieldOffice")[0].reset();
                        ZoneFieldOfficeTable.ajax.reload();
                    }
                    else {
                        ErrorMessage("#ZoneFieldOfficeInfo", response);
                    }
                });
            }
        });

    });


    // geting list of Zone => field office with group by relationship

    var ZoneFieldOfficeTable = $("#TableZoneFieldOffice").DataTable({
       // dom: 'Bfrtip',

        ajax: {
            url: "/ZoneFieldOffices/GetZoneFieldOffice",
            type: "POST",
            lengthMenu: [[10, 25, 50, 100, 250], [10, 25, 50, 100, 250]]
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
            { data: "zoneFieldOfficeID", "visible": false, "searchable": false },
            { data: "countryID", "visible": false, "searchable": false },
            { data: "zoneId", "visible": false, "searchable": false},
            { data: "stateId", "visible": false, "searchable": false },
            { data: "fieldOfficeID", "visible": false, "searchable": false },
            { data: "countryName" },
            { data: "zoneName" },
            { data: "officeName" },
            { data: "stateName" },
            { data: "createdAt" },
            { data: "updatedAt" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary zonefieldoffice" + row["zoneFieldOfficeID"] + "\" id=\"" + row['officeName'] + "| " + row["zoneName"] + "\" onclick=\"getZoneFieldOffice(" + row["zoneFieldOfficeID"] + ")\"><i class=\"fa fa-edit\"></i> Edit </button><button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteZoneFieldOffice(" + row["zoneFieldOfficeID"] + ")\"><i class=\"fa fa-trash\"></i> Delete</button>";
                }
            }

        ]

    });

    // reseting edit form
    $("#FormEditZoneFieldOfficeReset").on('click', function (event) {
        event.preventDefault();
        $("#FormEditZoneFieldOffice")[0].reset();
        $("#relationshipStatus").html("");
    });


    // Editing Zone => Field Office relationship

    $("#FormEditZoneFieldOffice").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditZoneFieldOfficeID").val() === "") {
            ErrorMessage("#EditZonalFieldOfficeeInfo", "Please select a relationship to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Zone and Field Office',
                text: 'Are you sure you want to create this Zone => field office relationship?',
                ok: function () {
                    $.getJSON("/ZoneFieldOffices/EditZoneFieldOffice", { "ZoneID": $("#txtEditZones").val(), "FieldOfficeID": $("#txtEditFieldOffice").val(), "ZoneFieldOfficeID": $("#txtEditZoneFieldOfficeID").val() }, function (response) {
                        if ($.trim(response) === "ZoneFieldOffice Updated") {
                            SuccessMessage("#EditZonalFieldOfficeeInfo", "Updated successfully.");
                            $("#FormEditZoneFieldOffice")[0].reset();
                            $("#relationshipStatus").html("");
                            ZoneFieldOfficeTable.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditZonalFieldOfficeeInfo", response);
                        }
                    });
                }
            });
        }
    });
});


/*
 * Getting Zone => field office info for relationship editing
 */
function getZoneFieldOffice(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateZoneFieldOffice").offset().top
    }, 1000);
    var zoneFieldOfficeId = $("#txtEditZoneFieldOfficeID").val(id);
    var zoneFieldOffice = $(".zonefieldoffice" + id).attr("id").split('|');

    var officeName = zoneFieldOffice[0];
    var zone = zoneFieldOffice[1];

    var html = "";

    $("#relationshipStatus").html("");

    html = "<small><span class=\"ui tiny purple label\">" + zone + "</span> => <span class=\"ui tiny orange label\">" + officeName + "</span></small>";

    $("#relationshipStatus").append(html);

}


/*
 * Removing a zone => field office relationship.
 */
function DeleteZoneFieldOffice(id) {

    Notify.confirm({
        title: 'Remove Zone and Field Office',
        text: 'Are you sure you want to remove this Zone => field office relationship?',
        ok: function () {
            $.getJSON("/ZoneFieldOffices/DeleteZoneFieldOffice", { "ZoneFieldOfficeID": id }, function (response) {
                if ($.trim(response) === "ZoneFieldOffice Deleted") {
                    Notify.suc({
                        title: 'Success',
                        text: 'Zone & field office removed successfully'
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