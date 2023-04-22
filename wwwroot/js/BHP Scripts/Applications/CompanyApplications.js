$(document).ready(function () {


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

    

    // schedule for company
    $.post("/Schdules/GetCompanyScheduleCount", function (response) {
       
        $("#NotifyCount").text(response);
        $("#ScheduleCount").text(response);
    });


    setInterval(function (e, xhr) {

        // schedule for company
        $.post("/Schdules/GetCompanyScheduleCount", function (response) {

            notifyCount = 0;
            notifyCount += response;
            $("#NotifyCount").text(response);
            $("#ScheduleCount").text(response);
        });


        $.post("/Session/CheckSession", function (response) {

            if ($.trim(response) === "true") {

                var location = window.location.origin + "/Account/ExpiredSession";
                window.location.href = location;
            }
        });


    }, 12000); // 2 min


    // selecting all countries to 
    $("#txtOriginState").ready(function () {
        var html = "";

        //$("#txtOriginState").html("");
        //$("#txtDestinationFieldState").html("");

        $.getJSON("/Helpers/GetAllStatesFromCountry",
            { "deletedStatus": false },
            function (datas) {

                //$("#txtOriginState").append("<option disabled selected>--Select State--</option>");
                //$("#txtDestinationFieldState").append("<option disabled selected>--Select State--</option>");
                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.stateID + ">" + val.stateName + "</option>";
                    });
                $("#txtOriginState").append(html);
                $("#txtDestinationFieldState").append(html);
            });
    });








    $("#CreateNewApplication").on('submit', function (e) {
        e.preventDefault();

        if ($("#txtCompanyID").val() === "" || $("#txtAppStageId").val() === "") {
            ErrorMessage("#CreateApplicationInfo", "Company/App Stage not found.");
        }
        else {

            var msg = confirm("Are you sure you want to create this application?");

            if (msg === true) {

                var Applications = {
                    SelfFacility: $("#txtOriginName").val().toUpperCase(),
                    SelfFacilityAddress: $("#txtOriginAddress").val(),
                    DestinationCompany: $("#txtDestinationCompanyName").val().toUpperCase(),
                    DestinationCompanyAddress: $("#txtDestinationCompanyAddress").val(),
                    DestinationFacility: $("#txtDestinationFieldName").val().toUpperCase(),
                    DestinationFacilityAddress: $("#txtDestinationFieldAddress").val(),
                    TransportationMode: $("#txtTransportMode").val(),
                    Qty: $("#txtQuantity").val(),
                    SelfFacilityState: $("#txtOriginState").val(),
                    DestinationFacilityState: $("#txtDestinationFieldState").val(),
                    OriginMesurementInstrument: $("#txtOriginMesurmentInstrument").val(),
                    DestinationMesurementInstrument: $("#txtDestinationMesurmentInstrument").val(),
                };

                $("#CreateAppLoader").addClass("Submitloader");

                $.post("/Applications/CreateApplication",
                    {
                        "Apps": Applications,
                        "StageId": $("#txtAppStageId").val(),
                        "CompanyId": $("#txtCompanyID").val(),
                        "TransportationMode": $("#txtTransportMode option:selected").text(),
                        "AppID" : $("#txtAppIDs").val()
                    }, function (response) {

                        var res = $.trim(response).split("|");
                        var location = window.location.origin + "/Applications/";

                        if (res[0] === "CreateTruck") {
                            SuccessMessage("#CreateApplicationInfo", "Application created successfully.");
                            $("#CreateAppLoader").removeClass("Submitloader");
                            window.location.href = location + "CreateTruck/" + res[1] + "/" + res[2];
                        }
                        else if (res[0] === "CreateBarge") {
                            SuccessMessage("#CreateApplicationInfo", "Application created successfully.");
                            $("#CreateAppLoader").removeClass("Submitloader");
                            window.location.href = location + "CreateBarge/" + res[1] + "/" + res[2];
                        }
                        else if (res[0] === "Created") {
                            SuccessMessage("#CreateApplicationInfo", "Application created successfully.");
                            $("#CreateAppLoader").removeClass("Submitloader");
                            window.location.href = window.location.origin + "/Companies/MyApplications";
                        }
                        else {
                            ErrorMessage("#CreateApplicationInfo", res[0]);
                            $("#CreateAppLoader").removeClass("Submitloader");
                        }
                    });
            }

        }
    });



    /*
     * Pass payment
     */
    $("#btnPassPayment").on('click', function (event) {

        var appid = $("#txtPayAppID").val().trim();

        var msg = confirm("Are you sure you want to perform this operation?");

        $("#PaymentDiv").addClass("Submitloader");

        if (msg === true) {

            $.post("/Applications/PassPayment",
                {
                    "AppID": appid
                },

                function (responses) {

                    var response = responses.trim().split("|");

                    if (response[0] === "Payment Passed") {
                        var location = window.location.origin + "/Applications/UploadDocuments/" + response[1]; // Encrypted application ID
                        window.location.href = location;
                        $("#PaymentDiv").removeClass("Submitloader");
                    }
                    else {
                        $("#PaymentDiv").removeClass("Submitloader");
                        ErrorMessage("#DivPaymentInfo", responses);
                    }
                });
        }
        else {
            $("#PaymentDiv").removeClass("Submitloader");
        }
    });





    $("#txtIssuedDate").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0)
    });


    $("#txtExpiryDate").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0)
    });



    /*
     * Accepting application schedule
     */
    $("#btnAcceptSchedule").on('click', function (event) {
        event.preventDefault();

        if ($("#txtAcceptComment").val() === "") {
            ErrorMessage("#ScheduleModalInfo", "Please enter comment");
        }
        else {
            var msg = confirm("Are you sure you want to accept this schedule?");

            if (msg === true) {

                $("#DivAcceptSchedule").addClass("Submitloader");

                $.post("/Schdules/CustomerAcceptSchedule",
                    {
                        "ScheduleID": $("#txtScheduleID").val(),
                        "txtComment": $("#txtAcceptComment").val()
                    },
                    function (response) {
                        if ($.trim(response) === "Schedule Accepted") {
                            SuccessMessage("#ScheduleModalInfo", "Schedule approved successfully.");
                            location.reload(true);
                        }
                        else {

                            $("#DivAcceptSchedule").removeClass("Submitloader");
                            ErrorMessage("#ScheduleModalInfo", response);
                        }
                    });
            }
        }
    });


    /*
    * Rejecting application schedule
    */
    $("#btnRejectSchedule").on('click', function (event) {
        event.preventDefault();

        if ($("#txtRejectComment").val() === "") {
            ErrorMessage("#ScheduleRejectModalInfo", "Please enter comment");
        }
        else {

            var msg = confirm("Are you sure you want to reject this schedule?");

            if (msg === true) {

                $("#DivRejectSchedule").addClass("Submitloader");

                $.post("/Schdules/CustomerRejectSchedule",
                    {
                        "ScheduleID": $("#txtScheduleID").val(),
                        "txtComment": $("#txtRejectComment").val()
                    },
                    function (response) {
                        if ($.trim(response) === "Schedule Rejected") {
                            SuccessMessage("#ScheduleRejectModalInfo", "Schedule rejected successfully.");
                            location.reload(true);
                        }
                        else {
                            $("#DivRejectSchedule").removeClass("Submitloader");
                            ErrorMessage("#ScheduleRejectModalInfo", response);
                        }
                    });
            }
        }
    });


});








