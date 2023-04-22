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



    // Create Country
    $("#FormCreateCountry").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtCountryName").val() === "") {
            ErrorMessage("#CreateCountryInfo", "Please enter a country");
        }
        else {

            Notify.confirm({
                title: 'Create Country',
                text: 'Are you sure you want to create this country?',
                ok: function () {

                    $.getJSON("/Countries/CreateCountry", { "Country": $("#txtCountryName").val() }, function (response) {
                        if ($.trim(response) === "Country Created") {
                            SuccessMessage("#CreateCountryInfo", "Country created successfully");
                            $("#FormCreateCountry")[0].reset();
                            $("#reLoadPurpose").click();
                            CountriesTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateCountryInfo", response);
                        }
                    });
                }
            });
        }
    });


    // geting list of countries
    var CountriesTables = $("#TableCountries").DataTable({

        ajax: {
            url: "/Countries/GetCountries",
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
            { data: "countryId" },
            { data: "countryName" },
            { data: "createdAt" },
            { data: "updatedAt" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary country" + row["countryId"] + "\" id=\"" + row['countryName'] + "\" onclick=\"getCountry(" + row["countryId"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger\" onclick=\"DeleteCountry(" + row["countryId"] + ")\"> <i class=\"fa fa-trash\"> </i> Delete </button>";
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


    // Editing Countries
    $("#FormEditCountry").on('submit', function (event) {
        event.preventDefault();

        if ($("#EditCountryName").val() === "") {
            ErrorMessage("#EditCountryInfo", "Please select a country to edit.");
        }
        else {
            Notify.confirm({
                title: 'Edit Country',
                text: 'Are you sure you want to edit this country?',
                ok: function () {
                    $.getJSON("/Countries/EditCountry", { "Country": $("#EditCountryName").val(), "CountryId": $("#EditCountryID").val() }, function (response) {
                        if ($.trim(response) === "Country Updated") {
                            SuccessMessage("#EditCountryInfo", "Country updated successfully.");
                            $("#FormEditCountry")[0].reset();
                            $("#reLoadPurpose").click();
                            CountriesTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditCountryInfo", response);
                        }
                    });
                }
            });
        }
    });

});


function getCountry(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateCountry").offset().top
    }, 1000);
    var country_id = $("#EditCountryID").val(id);
    var country = $(".country" + id).attr("id");
    $("#EditCountryName").val(country);
}


    function DeleteCountry(country_id) {

        Notify.confirm({
            title: 'Remove Country',
            text: 'Are you sure you want to remove this country?',
            ok: function () {
                $.getJSON("/Countries/DeleteCountry", { "CountryId": country_id }, function (response) {
                    if ($.trim(response) === "Country Deleted") {
                        Notify.suc({
                            title: 'Remove Country',
                            text: 'Country removed successfully'
                        });
                        location.reload(true);
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