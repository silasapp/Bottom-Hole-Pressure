$(function () {

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
            .html("<div class=\"alert alert-success\"> <i class=\"fa fa-check-circle\"> </i> <span class=\"text-dark\">" + success_message + " </span> </div>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }




    /*
     * Duration start date
     */
    $("#txtDurationStartDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     * Duration End date
     */
    $("#txtDurationEndDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     * Duration Report End date
     */
    $("#txtReportEndDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
    * Edit Duration start date
    */
    $("#txtEditDurationStartDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     *Edit  Duration End date
     */
    $("#txtEditDurationEndDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
    *Edit  Duration report End date
    */
    $("#txtEditReportEndDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });




    $("#btnCreateProposalRequest").on('click', function (event) {
        event.preventDefault();

        if ($("#txtDurationID").val() === "") {
            ErrorMessage("#ProposalRequestInfo", "Opps... Problem loading duration");
        }
        else if ($("#txtProposalComment").val() === "") {
            ErrorMessage("#ProposalRequestInfo", "Please enter comment.");
        }
        else {

            Notify.confirm({
                title: 'Send Proposal Request',
                text: 'Are you sure you want to sent this request to all companies?',
                ok: function () {
                    $("#ModelCreateProposalRequestLoadder").addClass("Submitloader");

                    $.post("/RequestProposals/CreateRequestAsync",
                        {
                            "txtComment": $("#txtProposalComment").val(),
                            "DurationID": $("#txtDurationID").val()
                        },
                        function (response) {

                            if ($.trim(response) === "Sent") {
                                SuccessMessage("#ProposalRequestInfo", "Request duration created successfully.");

                                $("#ModelCreateProposalRequestLoadder").removeClass("Submitloader");

                                location.reload(true);
                            }
                            else {
                                ErrorMessage("#ProposalRequestInfo", response);
                                $("#ModelCreateProposalRequestLoadder").removeClass("Submitloader");
                            }
                        });

                }
            });
        }
    });



    /*
     * Creating request duration
     */
    $("#FormCreateDuration").on('submit', function (even) {
        event.preventDefault();

        if ($("#txtDurationStartDate").val() === "" || $("#txtDurationEndDate").val() === "" || $("#txtDurationProposalYear").val() === "" || $("#txtReportEndDate").val() === "") {
            ErrorMessage("#RequestDurationInfo", "Please enter all fields.");
        }
        else {

            Notify.confirm({
                title: 'Create Duration',
                text: 'Are you sure you want to create this duration?',
                ok: function () {
                    $("#ModelCreateRequestDurationLoadder").addClass("Submitloader");

                    $.post("/RequestProposals/CreateDuration",
                        {
                            "DurationEndDate": $("#txtDurationEndDate").val(),
                            "DurationStartDate": $("#txtDurationStartDate").val(),
                            "ProposalYear": $("#txtDurationProposalYear").val(),
                            "ReportEndDate": $("#txtReportEndDate").val(),
                            "ContactPerson": $("#txtContactPerson").val(),
                            "Options": "Create"
                        },
                        function (response) {

                            if ($.trim(response) === "Created") {
                                SuccessMessage("#RequestDurationInfo", "Request duration created successfully.");
                                $("#FormCreateDuration")[0].reset();
                                $("#ModelCreateRequestDurationLoadder").removeClass("Submitloader");
                                $("#TableRequestDuration").load(location.href + " #TableRequestDuration");
                            }
                            else {
                                ErrorMessage("#RequestDurationInfo", response);
                                $("#ModelCreateRequestDurationLoadder").removeClass("Submitloader");
                            }
                        });
                }
            });

        }
    });



    /*
    * Editing request duration
    */
    $("#FormEditDuration").on('submit', function (even) {
        event.preventDefault();

        if ($("#txtEditDurationStartDate").val() === "" || $("#txtEditDurationEndDate").val() === "" || $("#txtEditDurationProposalYear").val() === "" || $("#txtEditReportEndDate").val() === "") {
            ErrorMessage("#EditRequestDurationInfo", "Please enter all fields.");
        }
        else {

            Notify.confirm({
                title: 'Edit Duration',
                text: 'Are you sure you want to edit this duration?',
                ok: function () {
                    $("#EditModelCreateRequestDurationLoadder").addClass("Submitloader");

                    $.post("/RequestProposals/CreateDuration",
                        {
                            "DurationId": $("#txtEditDuration").val(),
                            "DurationEndDate": $("#txtEditDurationEndDate").val(),
                            "DurationStartDate": $("#txtEditDurationStartDate").val(),
                            "ProposalYear": $("#txtEditDurationProposalYear").val(),
                            "ReportEndDate": $("#txtEditReportEndDate").val(),
                            "ContactPerson": $("#txtEditContactPerson").val(),
                            "Options": "Edit"
                        },
                        function (response) {

                            if ($.trim(response) === "Edited") {
                                SuccessMessage("#EditRequestDurationInfo", "Request duration edited successfully.");

                                $("#FormEditDuration")[0].reset();
                                $("#EditModelCreateRequestDurationLoadder").removeClass("Submitloader");
                                $("#TableRequestDuration").load(location.href + " #TableRequestDuration");
                            }
                            else {
                                ErrorMessage("#EditRequestDurationInfo", response);
                                $("#EditModelCreateRequestDurationLoadder").removeClass("Submitloader");
                            }
                        });
                }

            });
        }
    });

});


function getEditDuration(id, year, startDate, endDate, reportEnd, contact) {

    $('html, body').animate({
        scrollTop: $("#FormEditDuration").offset().top
    }, 1000);

    $("#txtEditDurationStartDate").val(startDate);
    $("#txtEditDurationEndDate").val(endDate);
    $("#txtEditDurationProposalYear").val(year);
    $("#txtEditDurationYear").text(year);
    $("#txtEditReportEndDate").val(reportEnd);
    $("#txtEditDuration").val(id);
    $("#txtEditContactPerson").val(contact);

}


function DeleteDuration(id) {

    Notify.confirm({
        title: 'Delete Duration',
        text: 'Are you sure you want to delete this duration?',
        ok: function () {
            $.post("/RequestProposals/DeleteDuration",
                {
                    "DurationId": id
                },
                function (response) {

                    if ($.trim(response) === "Deleted") {
                       
                        Notify.suc({
                            title: 'Success',
                            text: 'Proposal duration successfully deleted'
                        });

                        $("#TableRequestDuration").load(location.href + " #TableRequestDuration");
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



function ResendEmail(id) {

    Notify.confirm({
        title: 'Resend Proposal Request',
        text: 'Are you sure you want to resend this proposal request?',
        ok: function () {

            $("#RequestDivTable").addClass("Submitloader");

            $.post("/RequestProposals/ResendEmailAsync",
                {
                    "RequestId": id
                },
                function (response) {
                    if ($.trim(response) === "Sent") {

                        Notify.suc({
                            title: 'Success',
                            text: 'Proposal request sent successfully'
                        });

                        location.reload(true);

                    }
                    else {

                        Notify.alert({
                            title: 'Failure',
                            text: response
                        });
                        $("#RequestDivTable").removeClass("Submitloader");
                    }
                });
        }
    });

}