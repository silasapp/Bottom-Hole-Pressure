
$(function () {


    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
            .html("<div class=\"alert alert-danger\"><i class=\"fa fa-warning\"> </i> <span class=\"text-dark\"> " + error_message + " </span> </div>")
            .delay(9000)
            .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
            .html("<div class=\"alert alert-info\"> <i class=\"fa fa-check-circle\"> </i> <span class=\"text-dark\">" + success_message + " </span> </div>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }
  

    $(".customDT").DataTable({
        "processing": true,
        "serverSide": true,
        ajax: {
            url: "/Company/GetApplications",
            type: "POST",
            data: function (d) {
                d.RequestId = $("#RequestId").val();
            },
            "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
            buttons: ['csv', 'excel', 'pdf', 'print']            
        },
        dom: 'Blfrtip',
        columns: [
            { data: "qrt" },
            { data: "fields" },
            { data: "reservoir" },
            { data: "initialRpressure" },
            { data: "rbubblePointPressure" },
            { data: "wellName" },
            { data: "lastSurveyDate" },
            { data: "measuredRpressure" },
            { data: "timeShut" },
            { data: "usedInstrument" },
            { data: "operaionWellCost" },
            { data: "reservoirCreatedAt" }
        ]
    });

    $(".SurveyDT").DataTable({
        "processing": true,
        "serverSide": true,
        ajax: {
            url: "/Company/GetSurveyResult",
            type: "POST",
            data: function (d) {
                d.RequestId = $("#RequestId").val();
            },
            "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
            buttons: ['csv', 'excel', 'pdf', 'print']
        },
        dom: 'Blfrtip',
        columns: [
            { data: "field" },
            { data: "reservior" },
            { data: "remainingReserve" },
            { data: "wellName" },
            { data: "dateOfSurvey" },
            { data: "oil" },
            { data: "water" },
            { data: "gas" },
            { data: "bsw" },
            { data: "gor" },
            { data: "bean" },
            { data: "datumDepth" },
             { data: "initialPressure" },
            { data: "thp" },
            { data: "chp" },
            { data: "fbhpMp" },
            { data: "sbhpMp" },
            { data: "rpDatum" },
            { data: "productivityIndex" },
            { data: "dateOfLastSurvey" },
            { data: "remark" },
        ]
    });

    $("#UploadedFile").change(function () {
        var name = $('input[type=file]').val().replace(/C:\\fakepath\\/i, '');
        $("#filename").text(name);
    });




    $("#SubmitApp").on('submit', function (event) {
        event.preventDefault();
        var msg = confirm("Are you sure you want to Submit this excel file?");

        var fileUpload = $("#UploadedFile").get(0);

        var files = fileUpload.files;

        var data = new FormData();

        data.append("Files", files[0]);
        data.append("ReqId", $("#RequestId").val());

        if (msg === true) {

            $("#CreateApplicationDivLoader").addClass("Submitloader");

            $.ajax({
                url: '/Company/SaveApplicationForm',
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                type: 'POST',
                async:false,
                success: function (res) {
                    var message = res.message;
                    var response = res.response;
                    var reqId = res.req_id;
                    if ($.trim(response) === "Saved") {

                        SuccessMessage("#successmessage", message);

                        $('.customDT').DataTable().ajax.reload();

                        window.location.href = "/Company/ApplicationPayment?id=" + reqId;
                        
                        $("#CreateApplicationDivLoader").removeClass("Submitloader");
                        
                    }
                    else if ($.trim(response) === "Resubmitted") {

                        SuccessMessage("#successmessage", message);

                        window.location.href = "/Company/MyApplications";

                        $("#CreateApplicationDivLoader").removeClass("Submitloader");

                    }
                    else if ($.trim(response) === "No file" || $.trim(response) === "Wrong extension" || $.trim(response) === "Exception error" || $.trim(response) === "Empty field")
                    {
                        ErrorMessage("#errormessage", message);
                        $("#CreateApplicationDivLoader").removeClass("Submitloader");
                        //$('#failed').append(document.createTextNode(message));
                    }
                },
                error: function (res) {
                    var message = res.message;
                    var response = res.response;
                    if ($.trim(response) === "No file" || $.trim(response) === "Wrong extension" || $.trim(response) === "Exception error" || $.trim(response) === "Empty field")
                    {
                        ErrorMessage("#errormessage", message);
                        $("#CreateApplicationDivLoader").removeClass("Submitloader");
                       // $('#failed').append(document.createTextNode(message));
                    }
                }
            });
        }
    });




    /*
    * Submitting uploaded documents forms
    */
    $("#btnSubmitDocuments").on('click', function (event) {
        event.preventDefault();

        var LocalID = document.getElementsByName('txtLocalDocID[]');
        var DocSource = document.getElementsByName('txtDocSource[]');
        var CompDocElpsID = document.getElementsByName('txtCompDocElpsID[]');
        var missingCompDocElpsID = document.getElementsByName('missingCompElpsDocID[]');


        var AppID = $("#txtAppID").val().trim();

        if (missingCompDocElpsID.length > 0) {

            // validating missing documents
            for (var i = 0; i < missingCompDocElpsID.length; i++) {
                if (missingCompDocElpsID[i].value === "0" || missingCompDocElpsID[i].value === "") {
                    ErrorMessage("#divUploadDocInfo", "Please Upload all required documents");
                    break;
                }
                else {
                    alert("Inputs not found...");
                }
            }
        }
       
        else {

            // saving uploaded documents
            var AppDocuments = [];


            for (var j = 0; j < LocalID.length; j++) {

                AppDocuments.push({
                    "LocalDocID": LocalID[j].value.trim(),
                    "CompElpsDocID": CompDocElpsID[j].value.trim(),
                    "DocSource": DocSource[j].value.trim()
                });
            }


            var msg = confirm("Documents uploaded. Are you sure you want to proceed?");

            $("#divDocumentUpload").addClass("Submitloader");

            if (msg === true) {

                $.post("/Company/SubmitDocuments",
                    {
                        "RequestID": AppID,
                        "SubmittedDocuments": AppDocuments
                    },

                    function (response) {
                        var result = response;

                        if (result === "All Done") {
                            alert("Your document has been uploaded successfully... Please submit your application");
                            var location = window.location.origin + "/Company/MyApplications";
                            window.location.href = location;
                        }
                        else {
                            ErrorMessage("#divUploadDocInfo", result);
                            $("#divDocumentUpload").removeClass("Submitloader");
                        }
                    }
                );
            }
            else {
                $("#divDocumentUpload").removeClass("Submitloader");
            }
        }
    });







    $("#SubmitSurveyResult").on('submit', function (event) {
        event.preventDefault();
        var msg = confirm("Are you sure you want to Submit this excel file?");

        var fileUpload = $("#UploadedFile").get(0);

        var files = fileUpload.files;

        var data = new FormData();

        data.append("Files", files[0]);
        data.append("ReqId", $("#RequestId").val());


        if (msg === true) {

            $("#SubmitSurveyResultLoader").addClass("Submitloader");

            $.ajax({
                url: '/Company/SaveResultForm',
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                type: 'POST',
                async: false,
                success: function (res) {
                    var message = res.message;
                    var response = res.response;
                    var reqId = res.req_id;

                    if ($.trim(response) === "Saved") {
                        SuccessMessage("#successmessage", message);
                        $('.SurveyDT').DataTable().ajax.reload();
                        window.location.href = "/Company/MyApplications";
                    }
                    else if ($.trim(response) === "Resubmitted") {
                        SuccessMessage("#successmessage", message);
                        window.location.href = "/Company/MyApplications";
                    }
                    else{

                        ErrorMessage("#errormessage", message);
                        $("#SubmitSurveyResultLoader").removeClass("Submitloader");
                    }
                },
                error: function (res) {
                    var message = res.message;
                    var response = res.response;
                   
                        ErrorMessage("#errormessage", message);
                        $("#SubmitSurveyResultLoader").removeClass("Submitloader");
                        // $('#failed').append(document.createTextNode(message));
                    
                }


            });

        }

    });

    $(".customTable").DataTable({
        "aLengthMenu": [10, 25, 50, 75, 100, 200],
        buttons: ['csv', 'excel', 'pdf', 'print'],
        'iDisplayLength': 10,
        columnDefs: [{ orderable: false, "targets": [0, -1] }],
        dom: 'Blfrtip',
        "language": {
            "lengthMenu": "Display  _MENU_ records",
            "zeroRecords": "No record found",
            "info": "Showing page _PAGE_ of _PAGES_",
            "infoEmpty": "No Record to Display Currently",
            "infoFiltered": "(filtered from _MAX_ total records)"
        },
        "order": [[1, "desc"]]
    });
    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
            .html("<div class=\"alert alert-danger\"><i class=\"fa fa-warning\"> </i> <span class=\"\"> " + error_message + " </span> </div>")
            .delay(9000)
            .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
            .html("<div class=\"alert alert-info\"> <i class=\"fa fa-check-circle\"> </i> <span class=\"\">" + success_message + " </span> </div>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }


    var appTable = $("#MyAppsTable").DataTable({

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

        select: true,

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "order": [[5, "desc"]],
        "processing": true,
        stateSave: true

        
    });


    var permitTable = $("#MyPermitsTable").DataTable({

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

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "order": [[0, "desc"]],
        "processing": true,
        stateSave: true
    });


    var FacilityTable = $("#MyFacilityTable").DataTable({

        lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],
        dom: 'Bfrtip',
        buttons: [
            'pageLength', 
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "order": [[0, "desc"]],
        "processing": true,
       
    });



    var FacilityTables = $("#MyFacilityTables").DataTable({

        lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],
        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "order": [[0, "desc"]],
        "processing": true,
        
    });



    var FacilityTables1 = $("#MyFacilityTable1").DataTable({

        lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],
        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "order": [[0, "desc"]],
        "processing": true,
        
    });



    var FacilityTables2 = $("#MyFacilityTable2").DataTable({

        lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],
        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "order": [[0, "desc"]],
        "processing": true,
        
    });


    /*
     * Get company profile
     */
    $("#CompanyProfile").ready(function () {

        $("#CompanyDiv").addClass("loader");
        $("#CompanyProfile").addClass("loader");

        $.getJSON("/Company/GetCompanyProfile", { "CompanyId": $("#CompId").val() }, function (datas) {

            if (datas === "Network Error") {
                ErrorMessage("#FormCompanyProfileInfo", "A Network error has occured. Please check your network.");
                $("#CompanyDiv").removeClass("loader");
            }
            else {
                
                $("#txtId").val(datas.id);
                $("#txtCompanyName").val(datas.name);
                $("#txtBusinessType").val(datas.business_Type);
                $("#txtCompanyEmail").val(datas.user_Id);
                $("#txtContactFirstName").val(datas.contact_FirstName);
                $("#txtContactLastName").val(datas.contact_LastName);
                $("#txtContactNo").val(datas.contact_Phone);
                $("#txtRegNo").val(datas.rC_Number);
                $("#txtTinNo").val(datas.tin_Number);
                $("#txtYear").val(datas.year_Incorporated);
                $("#txtRegAddressId").val(datas.registered_Address_Id);
                $("#txtOperationalAddressId").val(datas.operational_Address_Id);
                $("#txtTotalAssets").val(datas.total_Asset);
                $("#txtStaffNo").val(datas.no_Staff);
                $("#txtYearlyRevenue").val(datas.yearly_Revenue);
                $("#txtExpatriateNo").val(datas.no_Expatriate);

                $("#txtNationality").append('<option value="' + datas.nationality + '" selected="selected">' + datas.nationality + '</option>');

                $("#CompanyDiv").removeClass("loader");
                $("#CompanyProfile").removeClass("loader");
            }
        });

    });


    // updating company profile 
    $("#FormCompanyProfile").on('submit', function (event) {

        event.preventDefault();

        var msg = confirm("Are you sure you want to update this company information?");

        if (msg === true) {

            var Company = {

                Id: $("#txtId").val(), // company id
                User_Id: $("#txtCompanyEmail").val(),
                Name: $("#txtCompanyName").val(),
                Business_Type: $("#txtBusinessType").val(),
                Contact_FirstName: $("#txtContactFirstName").val().toUpperCase(),
                Contact_LastName: $("#txtContactLastName").val().toUpperCase(),
                Contact_Phone: $("#txtContactNo").val(),
                RC_Number: $("#txtRegNo").val(),
                Tin_Number: $("#txtTinNo").val(),
                Year_Incorporated: $("#txtYear").val(),
                Nationality: $("#txtNationality").val(),
                Registered_Address_Id: $("#txtRegAddressId").val(),
                Operational_Address_Id: $("#txtOperationalAddressId").val(),
                Total_Asset: $("#txtTotalAssets").val(),
                NO_Staff: $("#txtStaffNo").val(),
                Yearly_Revenue: $("#txtYearlyRevenue").val(),
                No_Expatriate: $("#txtExpatriateNo").val()
            };

            $.post("/Company/UpdateCompanyProfile", Company, function (response) {

                if ($.trim(response) === "Company Updated") {
                    SuccessMessage("#FormCompanyProfileInfo", "Company's profile updated successfully.");
                }
                
                else {
                    ErrorMessage("#FormCompanyProfileInfo", response);
                }
            });
        }
    });



    // checking for registered and operational address 
    $("#divProfile").on('click', function (event) {
        event.preventDefault();

        if (($("#txtRegAddressId").val() === "" && $("#txtOperationalAddressId").val() === "") || ($("#txtRegAddressId").val() === "0" && $("#txtOperationalAddressId").val() === "0")) {
            $("#OpenRegAddressModal").click();
        }
        else {
            $("#CompanyAddress").addClass("loader");

            var address_id = "";
            var addressType = "";

            if ($("#txtRegAddressId").val() === "") {

                if ($("#txtOperationalAddressId").val() === "") {
                    address_id = "";
                    addressType = "operational";
                }
                else {
                    address_id = $("#txtOperationalAddressId").val();
                    addressType = "operational";
                }
            }
            else {
                address_id = $("#txtRegAddressId").val();
                addressType = "registered";
            }
            
            $.getJSON("/Helpers/GetCompanyAddress",
                {
                    "address_id": address_id
                },

                function (datas) {

                    if (datas !== "Empty") {

                        $("#txtUpAddressID").val(datas.id);
                        $("#txtUpAddressType").val(addressType);
                        $("#txtUpStreet").val(datas.address_1);
                        $("#txtUpCity").val(datas.city);
                        $("#txtUpPostalCode").val(datas.postal_code);
                        
                        $("#txtUpCountry").append('<option value="' + datas.country_Id + '" selected="selected">' + datas.countryName + '</option>');
                        $("#txtUpState").append('<option value="' + datas.stateId + '" selected="selected">' + datas.stateName + '</option>');

                        $("#CompanyAddress").removeClass("loader");
                    }
                    else {

                        if (datas === "Empty") {
                            ErrorMessage("#FormCompanyAddressInfo", "No Address found. Address is empty, please try again.");
                        }
                        else if (datas === "Network Error") {
                            ErrorMessage("#FormCompanyAddressInfo", "A network error occured, please check your network.");
                        }
                        else {
                            ErrorMessage("#FormCompanyAddressInfo", datas);
                        }
                        $("#CompanyAddress").removeClass("loader");
                    }

                    $("#CompanyAddress").removeClass("loader");
            });
        }
    });


    // get sates function by country id
    function GetStates(country_id, state_id, loader_id) {

        var html = "";
        $(state_id).html("");
        $(loader_id).text("Getting States....");

        $.getJSON("/Helpers/GetStates", { 'CountryId': $(country_id).val() }, function (datas) {

            $(state_id).append("<option disabled selected>--Select States--</option>");

            $.each(datas,
                function (key, val) {
                    html += "<option value=" + val.id + ">" + val.name + "</option>";
                });

            $(state_id).append(html);
            
            $(loader_id).text("");
        });
    }


    // geting states on country change - creating address
    $("#txtARegCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtARegCountry", "#txtARegState", "#StateLoader");
    });

    // geting states on country change - updating address
    $("#txtUpCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtUpCountry", "#txtUpState", "#UStateLoader");
    });

    // geting states on country change - creating director
    $("#txtRegDCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtRegDCountry", "#txtRegDState", "#RegDStateLoader");
    });

    // geting states on country change - geting director
    $("#txtDCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtDCountry", "#txtDState", "#DStateLoader");
    });


    

    //Creating a new Address for a company
    $("#FormRegAddress").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to save this new address for this company?");

        if (msg === true) {

            var Address = [
                {
                    address_1: $("#txtAAddress1").val(),
                    city: $("#txtACity").val(),
                    postal_code: $("#txtAPostalCode").val(),
                    stateId: $("#txtARegState").val(),
                    country_Id: $("#txtARegCountry").val(),
                    type: "registered"
                }
            ];
            
            $.post("/Company/CreateCompanyAddress", { "CompanyId": $("#txtId").val(), Address }, function (response) {

                if ($.trim(response) === "Created Address") {
                    SuccessMessage("#FormRegAddressInfo", "Company's address created successfully.");
                    window.location.reload(true);
                }
                else {
                    ErrorMessage("#FormRegAddressInfo", response);
                }
            });
        }
    });


    /*
     * Updating Company Address
     */
    $("#FormAddress").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to update this address?");

        if (msg === true) {

            var Address = [
                {
                    id: $("#txtUpAddressID").val(),
                    address_1: $("#txtUpStreet").val(),
                    city: $("#txtUpCity").val(),
                    postal_code: $("#txtUpPostalCode").val(),
                    stateId: $("#txtUpState").val(),
                    country_Id: $("#txtUpCountry").val(),
                    type: $("#txtUpAddressType").val()
                }
            ];

            $.post("/Company/UpdateCompanyAddress", { Address }, function (response) {

                if ($.trim(response) === "Address Updated") {
                    SuccessMessage("#FormCompanyAddressInfo", "Company's address updated successfully.");
                }
                else {
                    ErrorMessage("#FormCompanyAddressInfo", response);
                }
            });
        }

        
    });


    /*
     * Get Directors names 
     */
    $("#divDirectors").on('click', function (event) {
        event.preventDefault();

        $("#CompanyDirectors").addClass("loader");

        var html = "";
        $("#DirectorNames").html("");

        $.getJSON("/Company/GetDirectorsNames", { "CompanyId": $("#txtId").val() }, function (datas) {

            if (datas === "Network Error") {
                ErrorMessage("#CompanyDirectorsInfo", "A Network error has occured. Please check your network.");
                $("#CompanyDirectors").removeClass("loader");
            }
            else if (datas === null || datas === "" || datas.length === 0) {
                ErrorMessage("#CompanyDirectorsInfo", "No directors found. Please register your director(s)");
                $("#CompanyDirectors").removeClass("loader");
            }
            else {
                $.each(datas,
                    function (key, val) {
                        html += "<span class=\"btn btn-dark btn-block\" onclick=\"getDirectors("+ val.id+")\"> <i class=\"ti-user\"> </i> " + val.lastName + " " + val.firstName + "</span>";
                    });

                $("#DirectorNames").append(html);
                $("#CompanyDirectors").removeClass("loader");
            }
        });

    });


    /*
     * Creating company directors
     */
    $("#FormRegDirectors").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to register this director to this company?");

        if (msg === true) {

            var Directors = [
                {
                    company_Id: $("#txtId").val(),
                    firstName: $("#txtRegDFirstName").val().toUpperCase(),
                    lastName: $("#txtRegDLastName").val().toUpperCase(),
                    telephone: $("#txtRegDPhone").val(),
                    nationality: $("#txtRegDNationality").val(),
                    address:
                        {
                            address_1: $("#txtRegDAddress").val(),
                            city: $("#txtRegDCity").val(),
                            postal_Code: $("#txtRegDPostalCode").val(),
                            stateId: $("#txtRegDState").val(),
                            country_Id: $("#txtRegDCountry").val()
                        }
                }
            ];

            $.post("/Company/CreateCompanyDirectors", { "CompanyId": $("#txtId").val(), Directors }, function (response) {

                if ($.trim(response) === "Director Created") {
                    SuccessMessage("#FormRegDirectorInfo", "Company's director created successfully.");
                    window.location.reload(true);
                }
                else {
                    ErrorMessage("#FormRegDirectorInfo", response);
                }
            });
        }
    });


    /*
     * Updating company's director information 
     */ 
    $("#FormUpdateDirectors").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to update this director information?");

        if (msg === true) {

            var Directors = [
                {
                    id: $("#txtUpDDirectorId").val(),
                    company_Id: $("#txtUpDCompanyId").val(),
                    firstName: $("#txtUpDFirstName").val().toUpperCase(),
                    lastName: $("#txtUpDLastName").val().toUpperCase(),
                    telephone: $("#txtUpDPhoneNo").val(),
                    nationality: $("#txtUpDNationality").val(),
                    address_id: $("#txtUpDAddressId").val(),
                    address:
                        {
                            address_1: $("#txtUpDAddress").val(),
                            city: $("#txtUpDCity").val(),
                            postal_Code: $("#txtUpDPostalCode").val(),
                            country_Id: $("#txtDCountry").val(),
                            stateId: $("#txtDState").val(),
                            id: $("#txtUpDAddressId").val()
                        }
                }
            ];

            $.post("/Company/UpdateCompanyDirectors", { Directors }, function (response) {

                if ($.trim(response) === "Director Updated") {
                    SuccessMessage("#DirectorsInfo", "Company's director updated successfully.");
                }
                else {
                    ErrorMessage("#DirectorsInfo", response);
                }
            });
        }
    });
     
});

function submitapp(requestid) {

    if (requestid === null || requestid === "" || requestid.length === 0) {
        alert("Request Id could not be found");
    }
    else {
        var confirmaction = confirm("Are you sure you want to submit this application?");
        if (confirmaction === true) {
            $.get("/Company/SubmitApplication", { 'id': requestid }, function (data) {
                var res = data.response;
                var msg = data.message;

                if (res === null || res === "" || res.length === 0) {
                    alert("Something went wrong while getting response!!!");
                } else if (res === "saved") {
                    alert(msg);
                    window.location.href = "/Company/MyApplications";
                } else {
                    alert(msg);
                }
            });
        }
    }
}




function getDirectors(id) {

    if (id === null || id === "" || id.length === 0 || id <= 0) {
        ErrorMessage("#CompanyDirectorsInfo", "No reccord found for this director.");
    }
    else {
        $("#divRegDirector").addClass("loader");

        $.getJSON("/Company/GetDirectors", { "DirectorID": id }, function (datas) {

            if (datas === "Network Error") {
                ErrorMessage("#CompanyDirectorsInfo", "A Network error has occured. Please check your network.");
                $("#divRegDirector").removeClass("loader");
            }
            else if (datas === null || datas === "" || datas.length === 0) {
                ErrorMessage("#CompanyDirectorsInfo", "No directors found. Please register your director(s)");
                $("#divRegDirector").removeClass("loader");
            }
            else {
                $("#txtUpDCompanyId").val(datas.company_Id);
                $("#txtUpDAddressId").val(datas.address_Id);
                $("#txtUpDDirectorId").val(datas.id);
                $("#txtUpDFirstName").val(datas.firstName);
                $("#txtUpDLastName").val(datas.lastName);
                $("#txtUpDPhoneNo").val(datas.telephone);
                $("#txtUpDNationality").val(datas.nationality).change();
                $("#txtUpDAddress").val(datas.address.address_1);
                $("#txtUpDCity").val(datas.address.city);
                $("#txtDCountry").val(datas.address.country_Id).change();
                $("#txtUpDPostalCode").val(datas.address.postal_Code);
                $("#txtDState").val(datas.address.stateId).change();

                $("#divRegDirector").removeClass("loader");
            }
        });
    }
}

