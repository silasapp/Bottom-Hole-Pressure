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


    //// selecting all App Stage 
    //$("#txtPStages").ready(function () {
    //    var html = "";

    //    $("#txtPStages").html("");

    //    $.getJSON("/Helpers/GetAppStages",
    //        { "deletedStatus": false },
    //        function (datas) {

    //            $("#txtPStages").append("<option disabled selected>--Select App Stage--</option>");
    //            $("#txtEditPStages").append("<option disabled selected>--Select App Stage--</option>");

    //            $.each(datas,
    //                function (key, val) {
    //                    html += "<option value=" + val.appStageId + ">" + val.stageName + "</option>";
    //                });
    //            $("#txtPStages").append(html);
    //            $("#txtEditPStages").append(html);
    //        });
    //});


    // selecting all Roles 
    $("#txtARole").ready(function () {
        var html = "";

        $("#txtARole").html("");
        $("#txtRRole").html("");
        $("#txtEditARole").html("");

        $.getJSON("/Helpers/GetStaffRoles",
            { "deletedStatus": false },
            function (datas) {

                $("#txtARole").append("<option disabled selected>--Select Role--</option>");
                $("#txtRRole").append("<option disabled selected>--Select Role--</option>");
                $("#txtEditARole").append("<option disabled selected>--Select Role--</option>");
                $("#txtEditRRole").append("<option disabled selected>--Select Role--</option>");
                

                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.roleId + ">" + val.roleName + "</option>";
                    });
                $("#txtARole").append(html);
                $("#txtRRole").append(html);
                $("#txtEditARole").append(html);
                $("#txtEditRRole").append(html);
            });
    });



    //$("#txtPTypes").on('change', function (event) {
    //    event.preventDefault();
        
    //    $("#txtPStages").html("");
    //    var html = "";

    //    $.getJSON("/Helpers/GetAppStages",
    //        { "deletedStatus": false },
    //        function (datas) {

    //            $("#txtPStages").append("<option disabled selected>--Select App Stage--</option>");
               
    //            $.each(datas,
    //                function (key, val) {
                        
    //                    html += "<option value=" + val.stageID + ">" + val.stageName + "</option>";
    //                });
    //            $("#txtPStages").append(html);
               
    //        });
    //});


    //$("#txtEditPTypes").on('change', function (event) {
    //    event.preventDefault();

    //    $("#txtEditPStages").html("");
    //    var html = "";

    //    $.getJSON("/Helpers/GetAppStageFromType",
    //        { "TypeID": $("#txtEditPTypes").val() },
    //        function (datas) {

    //            $("#txtEditPStages").append("<option disabled selected>--Select App Stage--</option>");

    //            $.each(datas,
    //                function (key, val) {

    //                    html += "<option value=" + val.stageID + ">" + val.stageName + "</option>";
    //                });
    //            $("#txtEditPStages").append(html);

    //        });
    //});



    // Create Application Process 
    $("#FormCreateProcess").on('submit', function (event) {
        event.preventDefault();

        Notify.confirm({
            title: 'Create Application Process',
            text: 'Are you sure you want to create this process?',
            ok: function () {

                var ApplicationProccess = {

                    RoleId: $("#txtRole option:selected").val(),
                    Sort: $("#txtSort").val(),
                    LocationId: $("#txtLocation option:selected").val(),
                    CanPush: $("#txtCanPush option:selected").val(),
                    CanWork: $("#txtCanWork option:selected").val(),
                    CanAccept: $("#txtCanAccept option:selected").val(),
                    CanReject: $("#txtCanReject option:selected").val(),
                    CanInspect: $("#txtCanInspect option:selected").val(),
                    CanSchdule: $("#txtCanSchdule option:selected").val(),
                    CanRequest: $("#txtCanRequest option:selected").val(),
                    CanReport: $("#txtCanReport option:selected").val(),
                    OnAcceptRoleId: $("#txtARole option:selected").val(),
                    OnRejectRoleId: $("#txtRRole option:selected").val(),
                    Process: $("#txtProcessValue option:selected").val()
                };

                $.post("/ApplicationProccesses/CreateProcess", { "Proccess": ApplicationProccess }, function (response) {
                    if ($.trim(response) === "Process Created") {
                        SuccessMessage("#CreateProcessInfo", "Application process created successfuly.");
                        $("#FormCreateProcess")[0].reset();
                        AppProcessTables.ajax.reload();
                    }
                    else {
                        ErrorMessage("#CreateProcessInfo", response);
                    }
                });
            }

        });

    });


    // Edit Application Process
    $("#FormEditProcess").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditProcessID").val() === "") {

            ErrorMessage("#EditProccessInfo", "Please select the process to edit.");
        }
        else {

            Notify.confirm({
                title: 'Edit Application Process',
                text: 'Are you sure you want to edit this process?',
                ok: function () {

                    var ApplicationProcess = {

                        RoleId: $("#txtEditRole").val(),
                        Sort: $("#txtEditSort").val(),
                        LocationId: $("#txtEditLocation").val(),
                        CanPush: $("#txtEditCanPush").val(),
                        CanWork: $("#txtEditCanWork").val(),
                        CanAccept: $("#txtEditCanAccept").val(),
                        CanReject: $("#txtEditCanReject").val(),
                        CanInspect: $("#txtEditCanInspect").val(),
                        CanSchdule: $("#txtEditCanSchdule").val(),
                        CanRequest: $("#txtEditCanRequest").val(),
                        CanReport: $("#txtEditCanReport").val(),
                        OnAcceptRoleId: $("#txtEditARole").val(),
                        OnRejectRoleId: $("#txtEditRRole").val(),
                        Process: $("#txtEditProcess").val()
                    };

                    $.post("/ApplicationProccesses/EditProcess",
                        {
                            "processID": $("#txtEditProcessID").val(),
                            "Proccess": ApplicationProcess
                        },
                        function (response) {
                            if ($.trim(response) === "Process Updated") {
                                SuccessMessage("#EditProccessInfo", "Application process edited successfuly.");
                                $("#FormEditProcess")[0].reset();
                                AppProcessTables.ajax.reload();
                            }
                            else {
                                ErrorMessage("#EditProccessInfo", response);
                            }
                        });
                }
            });
        }

    });

    


    // geting list of application documents
    var AppProcessTables = $("#AppProcess").DataTable({

        ajax: {
            url: "/ApplicationProccesses/GetApplicationProcess",
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
            { data: "processID", "visible": false, "searchable": false },
           
            { data: "roleName" },
            { data: "locationName" },
            { data: "sort" },
            { data: "process" },
            { data: "canAccept", "orderable": false, "searchable": false },
            { data: "canReject", "orderable": false, "searchable": false },
            { data: "canPush", "orderable": false, "searchable": false },
            { data: "canWork", "orderable": false, "searchable": false },
            { data: "canInspect", "orderable": false, "searchable": false },
            { data: "canSchdule", "orderable": false, "searchable": false },
            { data: "canReport", "orderable": false, "searchable": false },
            { data: "acceptRole", "orderable": false, "searchable": false },
            { data: "rejectRole", "orderable": false, "searchable": false },
            { data: "createdAt" },           
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary process" + row["processID"] + "\" onclick=\"getProcess(" + row["processID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteProcess(" + row["processID"] + ")\"> <i class=\"fa fa-trash\"> </i> Bin </button>";
                }
            }
        ]

    });

    



    //// Editing Application document name
    //$("#FormEditAppDoc").on('submit', function (event) {
    //    event.preventDefault();
    //    if ($("#txtEditAppDocID").val() === "") {
    //        ErrorMessage("#EditAppDocInfo", "Please select an Application document to edit.");
    //    }
    //    else {
          
    //        var msg = confirm("Are you sure you want to edit this application document?");

    //        if (msg === true) {
    //            $.getJSON("/ApplicationDocuments/EditAppDoc",
    //                {
    //                    "AppDocName": $("#txtEditAppDocName").val(),
    //                    "AppDocID": $("#txtEditAppDocID").val(),
    //                    "AppDocType": $("#txtEditAppDocType").val()
    //                },
    //                function (response) {
    //                    if ($.trim(response) === "AppDoc Updated") {
    //                        SuccessMessage("#EditAppDocInfo", "Application stage updated successfully.");
    //                        $("#FormEditAppDoc")[0].reset();
    //                        AppDocTables.ajax.reload();
    //                    }
    //                    else {
    //                        ErrorMessage("#EditAppDocInfo", response);
    //                    }
    //                });
    //        }
    //    }
    //});


});


function getProcess(id) {

    $.getJSON("/ApplicationProccesses/GetProcess", { "processID": id }, function (datas) {

        if (datas.processID === 0) {
            alert(datas);
        }
        else {

            $('html, body').animate({
                scrollTop: $("#FormCreateProcess").offset().top
            }, 1000);
            $("#txtEditProcessID").val(id);

            $.each(datas,
                function (key, val) {
                   
                    $("#txtEditRole").val(val.roleId).change();
                    $("#txtEditSort").val(val.sort);
                    $("#txtEditLocation").val(val.locationId).change();
                    $("#txtEditCanPush").val(val.canPush.toString()).change();
                    $("#txtEditCanWork").val(val.canWork.toString()).change();
                    $("#txtEditCanAccept").val(val.canAccept.toString()).change();
                    $("#txtEditCanReject").val(val.canReject.toString()).change();
                    $("#txtEditProcess").val(val.process.toString()).change();
                    $("#txtEditCanInspect").val(val.canInspect.toString()).change();
                    $("#txtEditCanSchdule").val(val.canSchdule.toString()).change();
                    $("#txtEditCanReport").val(val.canReport.toString()).change();
                    $("#txtEditCanRequest").val(val.canRequest.toString()).change();
                    $("#txtEditARole").val(val.onAcceptRoleId.toString()).change();
                    $("#txtEditRRole").val(val.onRejectRoleId.toString()).change();

                });
        }
    });
}

/*
 * Removing Application documents.
 */
function DeleteProcess(id) {
   
    Notify.confirm({
        title: 'Remove Application Process',
        text: 'Are you sure you want to remove this Application Process ?',
        ok: function () {
            $.getJSON("/ApplicationProccesses/DeleteProcess", { "processID": id }, function (response) {
                if ($.trim(response) === "Process Removed") {
                    Notify.suc({
                        title: 'Success',
                        text: 'Application process removed successfully'
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

