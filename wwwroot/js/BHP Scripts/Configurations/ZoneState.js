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


    // selecting all States with respect to country to 
    $("#txtStates").ready(function () {
        var html = "";

        $("#txtStates").html("");

        $.getJSON("/Helpers/GetAllStatesFromCountry",
            { "CountryName": "Nigeria", "deletedStatus": false},
            function (datas) {
                $("#txtStates").append("<option disabled selected>--Select States--</option>");
                $("#txtEditStates").append("<option disabled selected>--Select States--</option>");
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.stateID + ">" + val.stateName + "</option>";
                    });
                $("#txtStates").append(html);
                $("#txtEditStates").append(html);
            });
    });


    // selecting all Zones 
    $("#txtZones").ready(function () {
        var html = "";

        $("#txtZones").html("");

        $.getJSON("/Helpers/GetAllZones",
            { "deletedStatus": false },
            function (datas) {

                $("#txtZones").append("<option disabled selected>--Select Zones--</option>");
                $("#txtEditZones").append("<option disabled selected>--Select Zones--</option>");

                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.zoneId + ">" + val.zoneName + "</option>";
                    });
                $("#txtZones").append(html);
                $("#txtEditZones").append(html);
            });
    });


    // Create Zone => State relationship

    $("#FormCreateZoneState").on('submit', function (event) {

        event.preventDefault();

        Notify.confirm({
            title: 'Create Zone & State',
            text: 'Are you sure you want to create this Zone => State relationship?',
            ok: function () {
                $.getJSON("/ZoneStates/CreateZoneState", { "ZoneID": $("#txtZones").val(), "StateID": $("#txtStates").val() }, function (response) {
                    if ($.trim(response) === "ZoneState Created") {
                        SuccessMessage("#ZoneStatesInfo", "Zone and State relationship created successfully");
                        $("#FormCreateZoneState")[0].reset();
                        ZoneStateTable.ajax.reload();
                    }
                    else {
                        ErrorMessage("#ZoneStatesInfo", response);
                    }
                });
            }
        });

    });


    // geting list of Zone => State relationship

    var ZoneStateTable = $("#TableZoneState").DataTable({

        ajax: {
            url: "/ZoneStates/GetZoneStates",
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
            { data: "zoneStateID", "visible": false, "searchable": false },
            { data: "zoneId", "visible": false, "searchable": false },
            { data: "stateId", "visible": false, "searchable": false },
            { data: "countryName" },
            { data: "stateName" },
            { data: "zoneName" },
            { data: "createdAt" },
            { data: "updatedAt" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary zonestate" + row["zoneStateID"] + "\" id=\"" + row['stateName'] + "| " + row["zoneName"] + "\" onclick=\"getZoneState(" + row["zoneStateID"] + ")\"> <i class=\"ui edit icon\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger\" target=\"\" onclick=\"DeleteZoneState(" + row["zoneStateID"] + ")\"> <i class=\"ui trash icon\"> </i> Delete </button>";
                }
            }

        ]

    });


    $("#FormEditZoneStateReset").on('click', function (event) {
        event.preventDefault();
        $("#relationshipStatus").html("");
        $("#FormEditZoneState")[0].reset();
    });


    // Editing Zone => State relationship

    $("#FormEditZoneState").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditZoneStateID").val() === "") {
            ErrorMessage("#EditZonalStateInfo", "Please select a relationship to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Zones & State',
                text: 'Are you sure you want to edit this Zone => State relationship?',
                ok: function () {
                    $.getJSON("/ZoneStates/EditZoneState", { "ZoneID": $("#txtEditZones").val(), "StateID": $("#txtEditStates").val(), "ZoneStateID": $("#txtEditZoneStateID").val() }, function (response) {
                        if ($.trim(response) === "ZoneState Updated") {
                            SuccessMessage("#EditZonalStateInfo", "Updated successfully.");
                            $("#FormEditZoneState")[0].reset();
                            $("#relationshipStatus").html("");
                            ZoneStateTable.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditZonalStateInfo", response);
                        }
                    });
                }
            });
        }
    });
});

/*
 * Getting Zone => state info for relationship editing
 */
function getZoneState(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateZoneState").offset().top
    }, 1000);
    var zoneStateId = $("#txtEditZoneStateID").val(id);
    var zoneState = $(".zonestate" + id).attr("id").split('|');

    var state = zoneState[0];
    var zone = zoneState[1];

    var html = "";

    $("#relationshipStatus").html("");

    html = "<span class=\"ui tiny orange label\" >" + zone + "</span> => <span class=\"ui tiny blue label\">" + state + "</span>";

    $("#relationshipStatus").append(html);

}


/*
 * Removing a zone => state.
 */
function DeleteZoneState(id) {

    Notify.confirm({
        title: 'Remove Zone & State',
        text: 'Are you sure you want to remove this Zone => State relationship?',
        ok: function () {
            $.getJSON("/ZoneStates/DeleteZoneState", { "ZoneStateID": id }, function (response) {
                if ($.trim(response) === "ZoneState Deleted") {
                    Notify.suc({
                        title: 'Success',
                        text: 'Zone & State removed successfully'
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