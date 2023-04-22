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



    // Create Location
    $("#FormCreateLocation").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtLocationName").val() === "") {
            ErrorMessage("#CreateLocationInfo", "Please enter a Location");
        }
        else {
            Notify.confirm({
                title: 'Create Location',
                text: 'Are you sure you want to create this location?',
                ok: function () {
                    $.getJSON("/Locations/CreateLocation", { "Location": $("#txtLocationName").val() }, function (response) {
                        if ($.trim(response) === "Location Created") {
                            SuccessMessage("#CreateLocationInfo", "Location created successfully");
                            $("#FormCreateLocation")[0].reset();

                            LocationTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateLocationInfo", response);
                        }
                    });
                }
            });
        }
    });


    // geting list of Location
    var LocationTables = $("#TableLocation").DataTable({

        ajax: {
            url: "/Locations/GetLocations",
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
            
            { data: "locationName" },
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary location" + row["locationId"] + "\" id=\"" + row['locationName'] + "\" onclick=\"getValue(" + row["locationId"] + ")\"> <i class=\"ui edit icon\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger\" target=\"\" onclick=\"DeleteLocation(" + row["locationId"] + ")\"> <i class=\"ui trash icon\"> </i> Bin </button>";
                }
            }
        ]
    });


    // Editing Location
    $("#FormEditLocation").on('submit', function (event) {
        event.preventDefault();

        if ($("#EditLocationName").val() === "") {
            ErrorMessage("#EditLocationInfo", "Please select a Location to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Location',
                text: 'Are you sure you want to create this location?',
                ok: function () {
                    $.getJSON("/Locations/EditLocation", { "Location": $("#EditLocationName").val(), "LocationId": $("#EditLocationID").val() }, function (response) {
                        if ($.trim(response) === "Location Updated") {
                            SuccessMessage("#EditLocationInfo", "Location updated successfully.");
                            $("#FormEditLocation")[0].reset();
                            $("#reLoadPurpose").click();
                            LocationTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditLocationInfo", response);
                        }
                    });
                }
            });
        }
    });
});


function getValue(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateLocation").offset().top
    }, 1000);
    var location_id = $("#EditLocationID").val(id);
    var location = $(".location" + id).attr("id");
    $("#EditLocationName").val(location);
}

/*
 * Removing Location.
 */
function DeleteLocation(id) {

    Notify.confirm({
        title: 'Remove Location',
        text: 'Are you sure you want to remove this location?',
        ok: function () {
            $.getJSON("/Locations/RemoveLocation", { "LocationID": id }, function (response) {
                if ($.trim(response) === "Location Removed") {
                    Notify.suc({
                        title: 'Success',
                        text: 'Location removed successfully'
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