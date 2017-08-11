$(function () {
    /*+++++++++++++++++++++全局通用事件++++++++++++++++++++*/
    //控制view展示
    $("#my-app").on("click", ".tm-show-view", function () {
        if ($(this).hasClass("active")) {
            HiddenTmView();
            $(this).removeClass("active");
        } else {
            $(".tm-show-view").removeClass("active");
            $(this).addClass("active");
            ShowTmView($(this).attr("data-href"));
        }
    });

    //控制view隐藏
    $("#my-app").on("click", ".tm-hidden-view", function () {
        HiddenTmView();
        $(".tm-show-view").removeClass("active");
    });

    //查看任务详情(包含子任务)
    $("#my-app").on("click", ".tm-show-task-modal", function () {
        if ($(this).attr("data-aid")) {
            var data = {
                AssignmentId: $(this).attr("data-aid")
            };
            GetTemplate("/TaskManagement/Assignment_Detail", data, function (data) {
                $("#Edit-Assignment .modal-content").html(data);
                $('#Edit-Assignment').modal('show');
            });
        } else if ($(this).attr("data-atid")) {
            var data = {
                SubTaskId: $(this).attr("data-atid")
            };
            GetTemplate("/TaskManagement/Subtask_Detail", data, function (data) {
                $("#Edit-Assignment .modal-content").html(data);
                $('#Edit-Assignment').modal('show');
            });
        } else {
            ErrorAlert("获取原始数据失败。");
        }

    });

    //任务完成状态回传(包含子任务)


    //请求项目表单
    $("#my-app").on("click", ".create_subject_target", function () {
        $.ajax({
            url: "/TaskManagement/GetSubjectForm",
            success: function (data) {
                if (data == "FAIL") {
                    ErrorAlert("操作失败。")
                } else {
                    $("#Add-Subject #Create-Subject-Content").html(data);
                    $("#Add-Subject").modal('show');
                }
            },
            error: function () {
                ErrorAlert("请求失败。")
            }
        });
    });
});

//请求活动中的项目
function GetActiveSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_ActiveSubjectListPartial",
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("读取执行中的项目失败。")
            } else {
                $(container).html(data);
            }
        }
    })
}
//请求完成的项目
function GetFinishSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_FinishSubjectListPartial",
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("读取归档的项目失败。")
            } else {
                $(container).html(data);
            }
        }
    })
}

//获取项目的过程
function GetProcedure(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/SubjectProcedure",
        data: {
            ProcedureId: ProcedureId
        },
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("获取项目阶段失败。")
            } else {
                container.html(data)
                GetAssignment(ProcedureId, SubjectId, container.find(".tm_panel-body"));
            }  
        },
        error: function () {
            ErrorAlert("操作失败。")
        }
    });
}

//获取任务表单
function GetAssignmentForm(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/GetAssignmentForm",
        data: {
            ProcedureId: ProcedureId,
            SubjectId: SubjectId
        },
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("创建任务失败。")
            } else {
                container.html(data)
            }
        },
        error: function () {
            ErrorAlert("请求失败。")
        }
    });
}



//获取任务列表
function GetAssignment(ProcedureId, SubJectId, container) {
    $.ajax({
        url: "/TaskManagement/SubjectAssignment",
        data: {
            ProcedureId: ProcedureId,
            SubJectId: SubJectId
        },
        success: function (data) {           
            container.html(data)
            var byId = function (id) { return document.getElementById(id); };
            [].forEach.call(byId('tm_panel-container').getElementsByClassName('tm_list-show'), function (el) {
                Sortable.create(el, {
                    filter: ".tm_list-add",
                    group: 'item',
                    dragClass: "sortable-drag",
                    ghostClass: "sortable-ghost",
                    animation: 150,
                    onEnd: function (evt) {
                        if (!evt.item.dataset.aid) {
                            ErrorAlert("操作失败")
                        } else {
                            if (!evt.item.offsetParent.offsetParent.dataset.procedureid) {
                                ErrorAlert("操作失败")
                            } else {
                                $.ajax({
                                    url:"/TaskManagement/DragAssignment",
                                    type:"post",
                                    data: {
                                        AssignmentId: evt.item.dataset.aid,
                                        nowpid: evt.item.offsetParent.offsetParent.dataset.procedureid
                                    },
                                    success: function (data) {
                                        if (data.result != "SUCCESS") {
                                            ErrorAlert(data.errmsg);
                                        } else {
                                            $.ajax({                                               
                                                url: "/TaskManagement/GetProcedureJsonInfo",
                                                type:"post",
                                                data: {
                                                    SubjectId: $("#panel-wrap").attr("data-sid")
                                                },
                                                success: function (data) {
                                                    var len = data.data.length;
                                                    $(".ajax-procedure").each(function () {
                                                        for (i = 0; i < len; i++) {
                                                            if (data.data[i].ProcedureId == $(this).attr("data-procedureid")) {
                                                                
                                                                $(this).find(".total-num").html(data.data[i].TotalNum)
                                                                $(this).find(".num").html(data.data[i].FinishNum)
                                                            }
                                                        }
                                                    })
                                                }
                                            })
                                        }
                                    },
                                    error: function (data) {
                                        ErrorAlert("操作失败");
                                    }
                                })
                            }
                        }
                    },
                });
            });
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}

//确定完成任务
function ComfirmFinishAssignment(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/ComfirmFinishAssignment",
        type: "post",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败。")
        }
    });
}
function Delete_Procedure(ProcedureId,SubjectId, Callback) {
    $.ajax({
        url: "/TaskManagement/Delete_Procedure",
        type: "post",
        data: {
            ProcedureId: ProcedureId,
            SubjectId:SubjectId
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function (data) {
            ErrorAlert("操作失败")
        }
    });
}
function Delete_Assignment(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Delete_Assignment",
        type: "post",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function (data) {
            ErrorAlert("操作失败。")
        }
    });
}
function GetAssignmnet_CollaboratorAddPartial(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignmnet_CollaboratorAddPartial",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败。")
        }
    });
}
function GetAssignment_CollaboratorPartial(AssignmentId,Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_CollaboratorPartial",
        data: {
            AssignmentId: AssignmentId
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败。")
        }
    });
}
function GetSubTaskListPartial(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_SubtaskPartial",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败。")
        }
    });
}



//插件调用
/*dialog调用*/
function UnimportantAlert(text) {
    $.alert({
        autoClose: 'cancelAction|2000',
        backgroundDismiss: true,
        escapeKey: 'cancelAction',
        typeAnimated: true,
        content: text,
        buttons: {
            cancelAction: {
                text: '关闭',
            }
        }
    });
}
function ErrorAlert(text) {
    $.alert({
        type: "red",
        autoClose: 'cancelAction|10000',
        escapeKey: 'cancelAction',
        typeAnimated: true,
        content: text,
        buttons: {
            cancelAction: {
                text: '关闭',
            }
        }
    });
}
function CustomConfirm(text,callback) {
    $.confirm({
        type: "red",
        alignMiddle: true,
        typeAnimated: true,
        content: text,
        buttons: {
            cancel: {
                text: "取消"
            },
            confirm: {
                text:"确定",
                btnClass: 'btn-red',
                action: function () {
                    if (callback && typeof callback == "function") {
                        callback();
                    }
                }
            },
        }
    });
}

function CustomConfirm2(text, callback, cancelcallback) {
    $.confirm({
        type: "red",
        alignMiddle: true,
        typeAnimated: true,
        content: text,
        buttons: {
            cancel: {
                text: "否",
                action: function () {
                    if (cancelcallback && typeof cancelcallback == "function") {
                        cancelcallback();
                    }
                }
            },
            confirm: {
                text: "是",
                btnClass: 'btn-red',
                action: function () {
                    if (callback && typeof callback == "function") {
                        callback();
                    }
                }
            },
        }
    });
}

/*时间控件调用*/
function CompleteTimeWidget(cotainer) {
    $(cotainer).datetimepicker({
        format: 'yyyy-mm-dd hh:00',
        minView:"day",
        autoclose: true,
        todayBtn: true,
        clearBtn: true,
        pickerPosition: "bottom-right",
        todayHighlighttodayHighlight: true,
    });
}
function GetElementsByClass(className) {
            var elements;

            if (document.getElementsByClassName) {
                elements = document.getElementsByClassName(className);
            }
            else {
                var elArray = [];
                var tmp = document.getElementsByTagName(elements);
                var regex = new RegExp("(^|\\s)" + className + "(\\s|$)");
                for (var i = 0; i < tmp.length; i++) {

                    if (regex.test(tmp[i].className)) {
                        elArray.push(tmp[i]);
                    }
                }

                elements = elArray;
            }
            return elements
}

//get请求模板
function GetTemplate(url, data, callback) {
    if (url == null || url == "") {
        ErrorAlert("请求地址不合法。");
    } else {
        $.ajax({
            url: url,
            data: data,
            success: function (data) {
                if (data == "FAIL") {
                    ErrorAlert("数据获取失败。");
                } else {
                    if (callback && typeof (callback) == "function") {
                        callback(data);
                    }
                }
            },
            error: function () {
                ErrorAlert("请求失败。");
            }
        });
    }
}


//tm-view控制
function HiddenTmView(callback) {
    $(".tm-view").slideUp(300, function () {
        $(".tm-view").remove();
        $("body").removeClass("tm-open-view");
        if (callback && typeof callback == "function") {
            callback();
        }
    });
}

function ShowTmView(url) {
        if ($(".tm-view").length > 0) {
            HiddenTmView(function () {
                //获取对应模板
                GetTemplate(url, {}, function (data) {
                    $("body>div:last").after(data);
                    $("body").addClass("tm-open-view");
                    $(".tm-view").slideDown(300);
                });
            });
        } else {
            //获取对应模板
            GetTemplate(url, {}, function (data) {
                $("body>div:last").after(data);
                $("body").addClass("tm-open-view");
                $(".tm-view").slideDown(300);
            });
        }
}

//查看任务


//dropdown位移控制


//post请求json





