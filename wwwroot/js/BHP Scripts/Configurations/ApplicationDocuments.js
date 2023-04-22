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



    // Create Company Application Documents 
    $("#FormCreateAppDoc1").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtAppDocName1").val() === "") {
            ErrorMessage("#CreateAppDocInfo1", "Please select a company application document");
        }
        else {
            var msg = confirm("Are you sure you want to create this company Application Document?");

            if (msg === true) {
                $.getJSON("/ApplicationDocuments/CreateAppDoc", { "AppDocElpsID": $("#txtAppDocName1").val(), "AppDocName": $("#txtAppDocName1 option:selected").text(), "AppDocType": "Company" }, function (response) {
                    if ($.trim(response) === "AppDoc Created") {
                        SuccessMessage("#CreateAppDocInfo1", "Company Application Document name created successfully");
                        $("#FormCreateAppDoc1")[0].reset();
                        AppDocTables.ajax.reload();
                    }
                    else {
                        ErrorMessage("#CreateAppDocInfo1", response);
                    }
                });
            }
        }
    });


    // Create Facility Application Documents 
    $("#FormCreateAppDoc2").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtAppDocName2").val() === "") {
            ErrorMessage("#CreateAppDocInfo2", "Please select a facility application document");
        }
        else {
            var msg = confirm("Are you sure you want to create this facility Application Document?");

            if (msg === true) {
                $.getJSON("/ApplicationDocuments/CreateAppDoc", { "AppDocElpsID": $("#txtAppDocName2").val(), "AppDocName": $("#txtAppDocName2 option:selected").text(), "AppDocType": "Facility" }, function (response) {
                    if ($.trim(response) === "AppDoc Created") {
                        SuccessMessage("#CreateAppDocInfo2", "Facility Application Document name created successfully");
                        $("#FormCreateAppDoc2")[0].reset();
                        AppDocTables.ajax.reload();
                    }
                    else {
                        ErrorMessage("#CreateAppDocInfo2", response);
                    }
                });
            }
        }
    });


    // geting list of application documents
    var AppDocTables = $("#TableAppDoc").DataTable({

        ajax: {
            url: "/ApplicationDocuments/GetAppDoc",
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
            { data: "appDocID", "visible": false, "searchable": false },
            { data: "appDocElpsID", "visible": false, "searchable": false },
            { data: "docName" },
            { data: "docType" },
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary doc" + row["appDocID"] + "\" id=\"" + row['docName'] + "\" onclick=\"getAppDoc(" + row["appDocID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteAppDoc(" + row["appDocID"] + ")\"> <i class=\"fa fa-trash-o\"> </i> Delete </button>";
                }
            }
        ]

    });


    // selecting all company Documents from elps to save needed ones on this app 
    $("#txtAppDocName1").ready(function () {
        var html = "";
        $("#divDocuments1").addClass("loader");

        $("#txtAppDocName1").html("");

        $.getJSON("/Helpers/GetElpsDocumentsTypes",
            function (datas) {

                if (datas !== "Network Error") {
                    $("#txtAppDocName1").append("<option disabled selected>--Select Documents--</option>");
                    $.each(datas,
                        function (key, val) {
                            html += "<option value=" + val.id + ">" + val.name + "</option>";
                        });
                    $("#txtAppDocName1").append(html);
                    $("#divDocuments1").removeClass("loader");
                }
                else {
                    ErrorMessage("#CreateAppDocInfo1", "A network error has occured. Please check your network.");
                    $("#divDocuments1").removeClass("loader");
                }

            });
    });


    // selecting all facility Documents from elps to save needed ones on this app 


    $("#txtAppDocName2").ready(function () {
        var html = "";
        $("#divDocuments2").addClass("loader");

        $("#txtAppDocName2").html("");

        $.getJSON("/Helpers/GetElpsFacDocumentsTypes",
            function (datas) {

                if (datas !== "Network Error") {
                    $("#txtAppDocName2").append("<option disabled selected>--Select Documents--</option>");
                    $.each(datas,
                        function (key, val) {
                            html += "<option value=" + val.id + ">" + val.name + "</option>";
                        });
                    $("#txtAppDocName2").append(html);
                    $("#divDocuments2").removeClass("loader");
                }
                else {
                    ErrorMessage("#CreateAppDocInfo2", "A network error has occured. Please check your network.");
                    $("#divDocuments2").removeClass("loader");
                }

            });
    });




    // Editing Application document name
    $("#FormEditAppDoc").on('submit', function (event) {
        event.preventDefault();
        if ($("#txtEditAppDocID").val() === "") {
            ErrorMessage("#EditAppDocInfo", "Please select an Application document to edit.");
        }
        else {
          
            var msg = confirm("Are you sure you want to edit this application document?");

            if (msg === true) {
                $.getJSON("/ApplicationDocuments/EditAppDoc",
                    {
                        "AppDocName": $("#txtEditAppDocName").val(),
                        "AppDocID": $("#txtEditAppDocID").val(),
                        "AppDocType": $("#txtEditAppDocType").val()
                    },
                    function (response) {
                        if ($.trim(response) === "AppDoc Updated") {
                            SuccessMessage("#EditAppDocInfo", "Application stage updated successfully.");
                            $("#FormEditAppDoc")[0].reset();
                            AppDocTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditAppDocInfo", response);
                        }
                    });
            }
        }
    });
});


function getAppDoc(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateAppDoc1").offset().top
    }, 1000);
    var app_id = $("#txtEditAppDocID").val(id);
    var app = $(".doc" + id).attr("id");
    $("#txtEditAppDocName").val(app);
}

/*
 * Removing Application documents.
 */
function DeleteAppDoc(id) {
    var msg = confirm("Are you sure you want to remove this Application document ?");
    if (msg === true) {
        $.getJSON("/ApplicationDocuments/DeleteAppDoc", { "AppDocID": id }, function (response) {
            if ($.trim(response) === "AppDoc Deleted") {
                alert("Application document removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}

