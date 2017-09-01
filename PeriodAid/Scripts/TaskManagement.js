$(function () {
    
    //控制子任务展示
    $("#my-app").on("click", ".tm-show-subtask-panel",function () {
        $(this).parents("li").find(".tm-subtask-ul").slideToggle(300);
        return false;
    })

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
    $("#my-app").on("click", ".tm-show-task-modal", function (e) {
        var data = {};
        if ($(this).attr("data-aid")) {
            data.AssignmentId = $(this).attr("data-aid");
            //请求任务详情
            GetTemplate("/TaskManagement/Assignment_Detail", data, function (data) {
                $("#Edit-Assignment .modal-content").html(data);
                $('#Edit-Assignment').modal('show');
            });
        } else if ($(this).attr("data-atid")) {
            data.SubTaskId = $(this).attr("data-atid");
            //请求子任务详情
            GetTemplate("/TaskManagement/Subtask_Detail", data, function (data) {
                $("#Edit-Assignment .modal-content").html(data);
                $('#Edit-Assignment').modal('show');
            });
        } else {
            ErrorAlert("获取原始数据失败。");
        }
        return false;

    });

    //任务完成状态回传(包含子任务)
    $("#my-app").on("click", ".tm-change-task-status", function () {
        var _self = $(this);
        var _data = {};
        var subjectdata={
            SubjectId:$("#panel-wrap").attr("data-sid")
        };
        if ($("#my-app").hasClass("tm-open-view")) {
            if ($(this).attr("data-aid")) {
                _data.AssignmentId = _self.attr("data-aid");
                EditForAjax("/TaskManagement/ComfirmFinishAssignment", _data, function (data) {
                    if ($(".tm-select-range.selected").attr("data-type") == "finish" || $(".tm-select-range.selected").attr("data-type") == "unfinish") {
                            _self.parents("li").remove();
                        }
                }, function () {
                    if (_self.hasClass("tm-checkbox-input")) {
                        _self.attr("checked", !_self.is(":checked"));
                    }
                    _self.attr("checked", _self.is(":checked"));

                });
            } else if (($(this).attr("data-atid"))) {
                _data.SubTaskId = $(this).attr("data-atid");
                EditForAjax("/TaskManagement/ComfirmFinishSubtask", _data, function (data) {
                        if ($(".tm-select-range.selected").attr("data-type") == "finish" || $(".tm-select-range.selected").attr("data-type") == "unfinish") {
                            _self.parents("li").remove();
                        }
                }, function () {
                    if (_self.hasClass("tm-checkbox-input")) {
                        _self.attr("checked", !_self.is(":checked"));
                    }
                    _self.attr("checked", _self.is(':checked'));
                });
            } else {
                ErrorAlert("获取原始数据失败。");
            }
            if (!_self.hasClass("tm-checkbox-input")) {
                return false;
            }
        } else {
            if ($(this).attr("data-aid")) {
                _data.AssignmentId = _self.attr("data-aid");
                EditForAjax("/TaskManagement/ComfirmFinishAssignment", _data, function (data) {
                    if ($("#panel-wrap").attr("data-sid")) {
                        GetAssignment(data.Id, $("#panel-wrap").attr("data-sid"), $(".ajax-procedure[data-procedureid=" + data.Id + "]").find(".tm_panel-body"));
                        updateTaskNum($("#panel-wrap").attr("data-sid"))
                        /*获取完成的任务*/
                        getAssignment("/TaskManagement/SubjectAssignment", { SubjectId: $("#panel-wrap").attr("data-sid") }, $("#tm-finsih-task"), function (data) {
                            /*成功回调*/
                        }, function (data) {
                            /*失败回调*/
                        })
                    }
                }, function () {
                    if (_self.hasClass("tm-checkbox-input")) {
                        _self.attr("checked", !_self.is(":checked"));
                    }
                    _self.attr("checked", _self.is(":checked"));

                });
            } else if (($(this).attr("data-atid"))) {
                _data.SubTaskId = $(this).attr("data-atid");
                EditForAjax("/TaskManagement/ComfirmFinishSubtask", _data, function (data) {
                    if ($("#panel-wrap").attr("data-sid")) {
                        $("#Edit-Assignment #Status").val(data.ParentStatus)
                        GetSubTaskListPartial($("#modal-header-assignment").attr("data-aid"), function (data) {
                            $("#modal_subtask_area").html(data);
                        });
                        GetAssignment(data.Id, $("#panel-wrap").attr("data-sid"), $(".ajax-procedure[data-procedureid=" + data.Id + "]").find(".tm_panel-body"));
                        updateTaskNum($("#panel-wrap").attr("data-sid"))
                        /*获取完成的任务*/
                        getAssignment("/TaskManagement/SubjectAssignment", { SubjectId: $("#panel-wrap").attr("data-sid") }, $("#tm-finsih-task"), function (data) {
                            /*成功回调*/
                        }, function (data) {
                            /*失败回调*/
                        })
                    }
                }, function () {
                    if (_self.hasClass("tm-checkbox-input")) {
                        _self.attr("checked", !_self.is(":checked"));
                    }
                    _self.attr("checked", _self.is(':checked'));
                });
            } else {
                ErrorAlert("获取原始数据失败。");
            }
            if (!_self.hasClass("tm-checkbox-input")) {
                return false;
            }
        }
    });

    //请求项目表单
    $("#my-app").on("click", ".create_subject_target", function () {
        GetTemplate("/TaskManagement/GetSubjectForm", {}, function (data) {
            $("#Add-Subject #Create-Subject-Content").html(data);
            $("#Add-Subject").modal('show');
        });
    });
});


function MoveTask() {
    var byId = function (id) { return document.getElementById(id); };
    [].forEach.call(byId('tm_panel-container').getElementsByClassName('tm_list-show'), function (el) {
        Sortable.create(el, {
            filter: ".tm_list-add",
            group: 'item-ul',
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
                            url: "/TaskManagement/DragAssignment",
                            type: "post",
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
                                        type: "post",
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
}
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



//获取任务列表
function GetAssignment(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/SubjectAssignment",
        data: {
            ProcedureId: ProcedureId,
            SubjectId: SubjectId
        },
        success: function (data) {           
            container.html(data)
            MoveTask()
        },
        error: function () {
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

/*时间控件调用*/
function CompleteTimeWidget(inputName, container) {
    if (container) {
        $(inputName).datetimepicker({
            format: 'yyyy-mm-dd hh:00',
            container: container,
            minView: "day",
            autoclose: true,
            todayBtn: true,
            clearBtn: true,
            pickerPosition: "bottom-right",
            todayHighlighttodayHighlight: true,
        });
    } else {
 $(inputName).datetimepicker({
        format: 'yyyy-mm-dd hh:00',
        minView:"day",
        autoclose: true,
        todayBtn: true,
        clearBtn: true,
        pickerPosition: "bottom-right",
        todayHighlighttodayHighlight: true,
    });
    }
  
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
function GetTemplate(url, data, callback,errback) {
    if (url == null || url == "") {
        ErrorAlert("请求地址不合法。");
    } else {
        $.ajax({
            url: url,
            data: data,
            success: function (data) {
                if (data == "FAIL") {
                    ErrorAlert("数据获取失败。");
                    if (errback && typeof (errback) == "function") {
                        errback(data)
                    }
                } else {
                    if (callback && typeof (callback) == "function") {
                        callback(data);
                    }
                }
            },
            error: function (data) {
                ErrorAlert("请求失败。");
                if (errback && typeof (errback) == "function") {
                    errback(data)
                }
            }
        });
    }
}

//post请求json
function EditForAjax(url, data, callback,errback) {
    if (url == null || url == "") {
        ErrorAlert("请求地址不合法。");
    } else {
        $.ajax({
            url: url,
            type:"post",
            data: data,
            success: function (data) {
                if (data.result == "FAIL") {
                    ErrorAlert(data.errmsg);
                    if (errback && typeof (errback) == "function") {
                        errback(data)
                    }
                } else {
                    if (callback && typeof (callback) == "function") {
                        callback(data);
                    }
                }
            },
            error: function () {
                ErrorAlert("请求失败。");
                if (errback && typeof (errback) == "function") {
                    errback(data)
                }
            }
        });
    }
}

//tm-view控制
function HiddenTmView(callback) {
    $(".tm-view").slideUp(500, function () {
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
                    $(".tm-view").slideDown(500);
                });
            });
        } else {
            //获取对应模板
            GetTemplate(url, {}, function (data) {
                $("body>div:last").after(data);
                $("body").addClass("tm-open-view");
                $(".tm-view").slideDown(500);
            });
        }
}

//任务拖动事件注册函数 id(ul的id)
function dragtask(id) {
    var el = document.getElementById(id)
    Sortable.create(el, {
        filter: ".tm_list-add",
        group: 'item-ul',
        dragClass: "sortable-drag",
        ghostClass: "sortable-ghost",
        animation: 150,
        onEnd: function (evt) {
            //判读是否携带任务id
            if (!evt.item.dataset.aid) {
                ErrorAlert("操作失败")
            } else {
                if (!evt.item.offsetParent.offsetParent.dataset.procedureid) {
                    ErrorAlert("操作失败")
                } else {
                    EditForAjax("/TaskManagement/DragAssignment", {
                        AssignmentId: evt.item.dataset.aid,
                        nowpid: evt.item.offsetParent.offsetParent.dataset.procedureid
                    }, function (data) {
                        EditForAjax("/TaskManagement/GetProcedureJsonInfo", {
                            SubjectId: $("#panel-wrap").attr("data-sid")
                        }, function (data) {
                            var len = data.data.length;
                            $(".ajax-procedure").each(function () {
                                for (i = 0; i < len; i++) {
                                    if (data.data[i].ProcedureId == $(this).attr("data-procedureid")) {
                                        $(this).find(".total-num").html(data.data[i].TotalNum)
                                        $(this).find(".num").html(data.data[i].FinishNum)
                                    }
                                }
                            })
                        }, function (data) {
                            /*失败回调*/
                        })
                    }, function (data) {
                        /*失败回调*/
                    })
                }
            }
        },
    });
}
//获取任务函数
function getAssignment(url, data, container, success, error) {
    /*获取任务*/
    GetTemplate(url, data, function (data) {
        container.html(data);
        if (success && typeof (success) == "function") {
            success(data);
        }
    }, function (data) {
        if (error && typeof (error) == "function") {
            success(data);
        }
    });
}

//任务面板页面数据更新
function updateTaskNum(SubejctId) {
    var subjectdata={
        SubjectId: SubejctId
    }
    EditForAjax("/TaskManagement/GetProcedureJsonInfo", subjectdata, function (data) {
        var len = data.data.length;
        var totalnum = 0;
        $(".ajax-procedure").each(function () {
            for (i = 0; i < len; i++) {
                if (data.data[i].ProcedureId == $(this).attr("data-procedureid")) {
                    $(this).find(".total-num").html(data.data[i].TotalNum)
                    $(this).find(".num").html(data.data[i].FinishNum)
                    totalnum += data.data[i].FinishNum;
                }
            }
        });
        $("#tm-finsh-total").html(totalnum)
    })
}

//帮助卡片
$("#bn_help").on("click", function () {
    $.ajax({
        url: "/TaskManagement/PersonalHelp",
        data: {
            SubjectId: $(this).attr("data-link")
        },
        success: function (data) {
            if (data != "FAIL") {
                $("#Create-Subject-Content").html(data);
                $("#Add-Subject").modal();
            }
            else {
                ErrorAlert("获取项目基本信息失败。");
            }
        }
    })
});










