var myApp = new Framework7({
    // Default title for modals
    modalTitle: '项目管理',

    // If it is webapp, we can enable hash navigation:
    pushState: true,

    // Hide and show indicator during ajax requests
    onAjaxStart: function (xhr) {
        myApp.showIndicator();
    },
    onAjaxComplete: function (xhr) {
        myApp.hideIndicator();
    }
});
var $$ = Dom7;
var mainView = myApp.addView('.view-main', {
    dynamicNavbar: true,
});
//初始化主页数据
HomeInitAjax();
$$(".tm-dropdown-menu-item").on("click", function () {
    var link_id = $$(this).attr("data-id");
    if ($$(this).hasClass("open")) {
        $$(this).removeClass("open");
        $$("#" + link_id).removeClass("open");
        $$(".tm-dropdown-mask").hide();
    } else {
        $$(".tm-dropdown-menu-item").removeClass("open");
        $$(".tm-dropdown-menu-list.open").removeClass("open");
        $$(".tm-dropdown-mask").show();
        $$(this).addClass("open");
        var movepx = $$(".tm-dropdown-menu-item[data-id='tm-type-dropdown']").find("span").offset().left + "px";
        $$("#" + link_id).addClass("open");
        $$("#" + link_id).find("label").css("paddingLeft", movepx);
    }
})
$$(".label-radio").on("click", function () {
    alert($$(this).find("input").val())
});
$$(".tm-dropdown-mask").on("click", function () {
    $$(this).hide();
    $$(".tm-dropdown-menu-item").removeClass("open");
    $$(".tm-dropdown-menu-list.open").removeClass("open");
})
myApp.onPageInit('home', function (page) {
    HomeInitAjax()
});
myApp.onPageInit('my', function (page) {
    alert("my");
});
myApp.onPageInit('subject_procedure', function (page) {
    //初始化加载数据
    ProcedureAjax($$(".tab-link-procedure.active"))
    $$(".tab-link-procedure").on("click", function () {
        var that = $$(this);
        if (!$$(this).hasClass("active")) {
            if (!$$(this).hasClass("init-finish")) {         
                ProcedureAjax(that);
            };
        }
    });
});
$$(".tab-link").on("click", function (page) {
    loadScript('http://suggestion.baidu.com/su?wd=w', function () { console.log('loaded') });
    var tab_link = $$(this).attr("href");
    if (!$$(this).hasClass("active")) {
        if (!$$(this).hasClass("init-finish")) {
            $$(this).addClass("init-finish");
            alert("开始加载数据");
        };
    }
});
function ProcedureAjax(that) {
    $$.ajax({
        url: "/MobileTaskManagement/ProcedureAssigemnetAjax",
        type: "post",
        data: {
            ProcedureId: that.attr("data-pid")
        },
        success: function (data) {
            if (!that.hasClass("init-finish")) {
                that.addClass("init-finish")
            }
            var _data = JSON.parse(data);
            if (_data.result == "SUCCESS") {
                var _tasklen = _data.data.length;
                var _container = that.attr("href");
                for (i = 0; i < _tasklen; i++) {
                    var _status = _data.data[i].status == 1 ? "未完成" : "完成";
                    var _deadtime = ChangeDateFormat(_data.data[i].deadTime) == "" ? "" : "截止时间:" + ChangeDateFormat(_data.data[i].deadTime);
                    $$(_container).find("ul").append("<li><a class=\"item-content item-link\" href=\"/MobileTaskManagement/AssignmentDetail?AssignmentId=" + _data.data[i].id + "\">"
                                            + "<div class=\"item-inner\"><div class=\"item-title-row\">"
                                            + "<div class=\"item-title\">" + _data.data[i].title + "</div>"
                                             + "<div class=\"item-after\">" + _data.data[i].holderName + "</div></div>"
                                              + "<div class=\"item-subtitle color-gray\"><span>状态:" + _status + "</span>&nbsp;<span>" + _deadtime + "</span></div></div></a></li>")
                }
                //检查是否有数据和显示方式
                if ($$(_container).find("ul li").length == 0) {
                    $$(_container).find(".tm-empty").show();
                    $$(_container).find("ul").hide();
                }
            } else {
                myApp.alert("数据获取失败。");
            }
        },
        error: function () {
            myApp.alert("请求数据失败。");
        }

    })
}
function HomeInitAjax() {
    $$.ajax({
        url: "/MobileTaskManagement/PersonalSubject",
        type: "post",
        success: function (data) {
            var _data = JSON.parse(data);
            if (_data.result == "FAIL") {
                myApp.alert("数据获取失败。")
            } else {
                $$(".tm-subject-area ul").html("");
                var _sublen = _data.subject.length;
                for (i = 0; i < _sublen; i++) {
                    var _defaultimg = "/Content/images/cover-default.jpg";
                    if (_data.subject[i].imgUrl != null) {
                        _defaultimg = "http://cdn2.shouquanzhai.cn/" + _data.subject[i].imgUrl;
                    }
                    if (_data.subject[i].status == 1) {
                        $$(".tm-subject-area.active ul").append("<li><a class=\"item-link\" href=\"/MobileTaskManagement/SubjectProcedure?SubjectId=" + _data.subject[i].id + "\">"
                                            + "<div class=\"item-content\"><div class=\"item-media\"><img width=\"44\" src=" + _defaultimg + "></div>"
                                            + "<div class=\"item-inner\"><div class=\"item-title-row\">"
                                            + "<div class=\"item-title\">" + _data.subject[i].title + "</div></div>"
                                            + "<div class=\"item-subtitle color-gray\">" + _data.subject[i].holderName + "</div></div></div></a></li>")

                    } else {
                        $$(".tm-subject-area.archived ul").append("<li><a class=\"item-link\" href=\"/MobileTaskManagement/SubjectProcedure?SubjectId=" + _data.subject[i].id + "\">"
                                          + "<div class=\"item-content\"><div class=\"item-media\"><img width=\"44\" src=" + _defaultimg + "></div>"
                                          + "<div class=\"item-inner\"><div class=\"item-title-row\">"
                                          + "<div class=\"item-title\">" + _data.subject[i].title + "</div></div>"
                                          + "<div class=\"item-subtitle color-gray\">" + _data.subject[i].holderName + "</div></div></div></a></li>")
                    }
                }
                //检查是否有数据和显示方式
                $$(".tm-subject-area").each(function () {
                    if ($$(this).find("ul li").length == 0) {
                        $$(this).find(".tm-empty").show();
                        $$(this).find("ul").hide();
                    }
                })
            }
        },
        error: function () {
            myApp.alert("请求数据失败。")
        }
    })
}
function ChangeDateFormat(val) {
    if (val != null) {
        var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
        //月份为0-11，所以+1，月份小于10时补个0
        var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
        var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
        var hour = date.getHours();
        var minute = date.getMinutes();
        var second = date.getSeconds();
        var dd = date.getFullYear() + "-" + month + "-" + currentDate + " "+hour+":"+"00"
        return dd;
    }
    return "";
}
function loadScript(url, func) {
    var head = document.head || document.getElementByTagName('head');
    var script = document.createElement('script');
    script.src = url;
    script.onload = script.onreadystatechange = function () {
        if (!this.readyState || this.readyState == 'loaded' || this.readyState == 'complete') {
            func();
            script.onload = script.onreadystatechange = null;
        }
    };

    head.insertBefore(script, head.childNodes[0]);
}
console.log(window)


