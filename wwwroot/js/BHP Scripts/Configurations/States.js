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


    // selecting all countries to 
    $("#txtStateCountry").ready(function () {
        var html = "";
        
        $("#txtStateCountry").html("");

        $.getJSON("/Helpers/GetAllCountries",
            { "deletedStatus": false},
            function (datas) {

                $("#txtStateCountry").append("<option disabled selected>--Select Country--</option>");
                $("#txtEditStateCountryName").append("<option disabled selected>--Select Country--</option>");
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.countryId + ">" + val.countryName + "</option>";
                    });
                $("#txtStateCountry").append(html);
                $("#txtEditStateCountryName").append(html);
            });
    });



    // Create State
    $("#FormCreateState").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtStateName").val() === "") {
            ErrorMessage("#CreateStateInfo", "Please enter a state");
        }
        else {
            Notify.confirm({
                title: 'Create State',
                text: 'Are you sure you want to create this state?',
                ok: function () {
                    $.getJSON("/States/CreateState", { "CountryID": $("#txtStateCountry").val(), "StateName": $("#txtStateName").val() }, function (response) {
                        if ($.trim(response) === "State Created") {
                            SuccessMessage("#CreateStateInfo", "State created successfully");
                            $("#FormCreateState")[0].reset();
                            StatesTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateStateInfo", response);
                        }
                    });
                }
            });
        }
    });


    // geting list of countries
    var StatesTables = $("#TableStates").DataTable({

        ajax: {
            url: "/States/GetStates",
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
            { data: "countryId", "visible": false, "searchable":false},
            { data: "stateId", "visible": false, "searchable": false },
            { data: "countryName" },
            { data: "stateName" },
            { data: "createdAt" },
            { data: "updatedAt" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary state" + row["stateId"] + "\" id=\"" + row['stateName'] + "\" onclick=\"getState(" + row["stateId"] + ")\"> <i class=\"ui edit icon\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger button\" target=\"\" onclick=\"DeleteState(" + row["stateId"] + ")\"> <i class=\"ui trash icon\"> </i> Delete </button>";
                }
            }

        ]
    
    });


    // Editing Countries
    $("#FormEditState").on('submit', function (event) {
        event.preventDefault();
        
        if ($("#EditCountryName").val() === "") {
            ErrorMessage("#EditStateInfo", "Please select a country to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit State',
                text: 'Are you sure you want to edit this state?',
                ok: function () {
                    $.getJSON("/States/EditState", { "StateName": $("#txtEditStateName").val(), "StateID": $("#EditStateID").val(), "CountryID": $("#txtEditStateCountryName").val() }, function (response) {
                        if ($.trim(response) === "State Updated") {
                            SuccessMessage("#EditStateInfo", "State updated successfully.");
                            $("#FormEditState")[0].reset();
                            StatesTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditStateInfo", response);
                        }
                    });
                }
            });
        }
    });
});

/*
 * Getting State info for editing
 */
function getState(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateState").offset().top
    }, 1000);
    var country_id = $("#EditStateID").val(id);
    var state = $(".state" + id).attr("id");
    $("#txtEditStateName").val(state);
}


/*
 * Removing a state.
 */ 
function DeleteState(id) {

    Notify.confirm({
        title: 'Remove State',
        text: 'Are you sure you want to remove this state?',
        ok: function () {
            $.getJSON("/States/DeleteState", { "StateID": id }, function (response) {
                if ($.trim(response) === "State Deleted") {
                    Notify.suc({
                        title: 'Success',
                        text: 'State removed successfully'
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