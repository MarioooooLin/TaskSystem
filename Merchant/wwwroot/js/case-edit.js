(function () {
    "use strict";

    /* ==========================================
       步驟 Tab：點擊滾動到對應區塊
    =========================================== */
    $(document).on("click", ".ac-tab", function () {
        var $tab = $(this);
        var targetId = $tab.data("target");
        if (!targetId) return;

        $(".ac-tab").removeClass("active");
        $tab.addClass("active");

        var $target = $("#" + targetId);
        if ($target.length) {
            var offset = $(".ac-tabs-wrap").outerHeight() + 20;
            $("html, body").animate(
                {
                    scrollTop: $target.offset().top - offset,
                },
                300,
            );
        }
    });

    /* Tab 左右箭頭（手機版） */
    var $tabs = $(".ac-tabs");
    $("#tabsArrowLeft").on("click", function () {
        $tabs.animate({ scrollLeft: $tabs.scrollLeft() - 120 }, 200);
    });
    $("#tabsArrowRight").on("click", function () {
        $tabs.animate({ scrollLeft: $tabs.scrollLeft() + 120 }, 200);
    });

    /* ==========================================
       銷售分潤 Toggle
    =========================================== */
    $(document).on("change", "#revShareToggle", function () {
        $("#revSharePanel").toggle(this.checked);
    });

    /* ==========================================
       複選標籤：切換 active class
    =========================================== */
    $(document).on("change", ".ac-check-tag input", function () {
        $(this).closest(".ac-check-tag").toggleClass("active", this.checked);
    });

    /* ==========================================
       贈品項目 Tag 動態新增/刪除
    =========================================== */
    function syncBarterHiddenFields() {
        var $container = $("#barter-hidden-fields");
        $container.empty();

        $("#giftTags .ac-tag").each(function (i) {
            var $tag = $(this);
            var id = $tag.data("id") || "";
            var name = $tag.data("name") || "";
            var quantity = $tag.data("quantity") || "1";
            var note = $tag.data("note") || "";

            $container.append('<input type="hidden" name="BarterItems[' + i + '].Id" value="' + id + '" />');
            $container.append(
                '<input type="hidden" name="BarterItems[' + i + '].Name" value="' + escapeHtml(name) + '" />',
            );
            $container.append(
                '<input type="hidden" name="BarterItems[' + i + '].Quantity" value="' + quantity + '" />',
            );
            $container.append(
                '<input type="hidden" name="BarterItems[' + i + '].Note" value="' + escapeHtml(note) + '" />',
            );
        });
    }

    function escapeHtml(text) {
        return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }

    function createTag(name) {
        return $(
            '<span class="ac-tag" data-name="' +
                escapeHtml(name) +
                '" data-quantity="1" data-note="">' +
                escapeHtml(name) +
                ' <button type="button" class="ac-tag__del">×</button></span>',
        );
    }

    $("#giftAddBtn").on("click", function () {
        var $input = $("#giftInput");
        var name = $.trim($input.val());
        if (!name) return;

        $("#giftTags").append(createTag(name));
        $input.val("").focus();
        syncBarterHiddenFields();
    });

    $("#giftInput").on("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            $("#giftAddBtn").trigger("click");
        }
    });

    $("#giftTags").on("click", ".ac-tag__del", function () {
        $(this).closest(".ac-tag").remove();
        syncBarterHiddenFields();
    });

    /* 初始化：確保現有 tag 與隱藏欄位同步 */
    syncBarterHiddenFields();

    /* ==========================================
       提交模式設定
    =========================================== */
    window.setSubmitMode = function (mode) {
        $("#submit-mode").val(mode);
    };

    /* ==========================================
       表單送出前驗證（避免後端拋 NULL 例外）
    =========================================== */
    function isBlank(value) {
        return !value || $.trim(value) === "";
    }

    function showFieldError($field, message) {
        var $error = $field.siblings(".text-danger");
        if ($error.length) {
            $error.text(message);
        }
        $field.focus();
    }

    $("#case-edit-form").on("submit", function (e) {
        var submitMode = $("#submit-mode").val();
        if (submitMode !== "Publish") return true; // 儲存草稿暫不強制全部必填

        var errors = [];
        var firstInvalid = null;

        function check($field, message) {
            if (isBlank($field.val())) {
                errors.push(message);
                if (!firstInvalid) firstInvalid = { $field: $field, message: message };
            }
        }

        check($("#Title"), "請輸入案件名稱");
        check($("#Description"), "請輸入案件簡介");
        check($("#CityId"), "請選擇執行縣市");
        check($("#Address"), "請輸入詳細地址");
        check($("#WantedKolCount"), "請輸入徵求人數");
        check($("#ApplicationDeadline"), "請選擇報名截止日期");
        check($("#SubmissionDeadline"), "請選擇交付截止日期");
        check($("#CashRewardAmount"), "請輸入現金報酬");
        check($("#DeliverableDescription"), "請輸入交付需求清單");

        // 複選至少選一項
        if ($('input[name="Categories"]:checked').length === 0) {
            errors.push("請至少選擇一個 KOL 類型");
        }
        if ($('input[name="Languages"]:checked').length === 0) {
            errors.push("請至少選擇一個語言");
        }
        if ($('input[name="Platforms"]:checked').length === 0) {
            errors.push("請至少選擇一個發佈平台");
        }

        if (errors.length > 0) {
            e.preventDefault();
            alert("請先完成以下必填欄位：\n\n" + errors.join("\n"));
            if (firstInvalid) showFieldError(firstInvalid.$field, firstInvalid.message);
            return false;
        }

        return true;
    });

    /* ==========================================
       附件上傳（拖曳 + 點擊，支援多檔）
    =========================================== */
    var $uploadZone = $("#uploadZone");
    var $fileInput = $("#fileInput");

    $uploadZone.on("click", function (e) {
        if ($(e.target).closest("#fileInput").length) return;
        $fileInput.trigger("click");
    });

    $uploadZone.on("dragover", function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass("ac-upload-zone--active");
    });

    $uploadZone.on("dragleave drop", function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass("ac-upload-zone--active");
    });

    $uploadZone.on("drop", function (e) {
        var files = e.originalEvent.dataTransfer.files;
        handleFiles(files);
    });

    $fileInput.on("change", function () {
        handleFiles(this.files);
        this.value = "";
    });

    function handleFiles(files) {
        if (!files || files.length === 0) return;
        for (var i = 0; i < files.length; i++) {
            uploadAttachment(files[i]);
        }
    }

    function uploadAttachment(file) {
        var caseId = $("#CaseId").val();
        if (!caseId || caseId === "0") {
            alert("請先儲存草稿，建立案件後再上傳附件。");
            return;
        }

        var url = "/Case/UploadAttachment?caseId=" + caseId;

        var formData = new FormData();
        formData.append("CaseId", caseId);
        formData.append("File", file);
        formData.append("AttachmentType", 1); // ReferenceMaterial

        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            headers: { RequestVerificationToken: token },
            success: function () {
                if (typeof htmx !== "undefined") {
                    htmx.trigger("#attachment-list", "refresh");
                }
            },
            error: function (xhr) {
                alert("上傳失敗：" + (xhr.responseText || "未知錯誤"));
            },
        });
    }
})();
