$(document).ready(function () {

    /* ==========================================
       Sidebar Toggle
    =========================================== */
    var isMobile = function () { return $(window).width() <= 768; };

    // 動態插入 overlay
    if ($('.sidebar-overlay').length === 0) {
        $('body').append('<div class="sidebar-overlay" id="sidebarOverlay"></div>');
    }

    /* ==========================================
       Action Bar 同步（適用所有帶 .kd-action-bar 的頁面）
       讓底部 sticky bar 的 left 值隨 sidebar 展開/收合同步位移
    =========================================== */
    var sidebarWidth = getComputedStyle(document.documentElement)
                        .getPropertyValue('--sidebar-width').trim();

    function syncActionBar() {
        if (!$('.kd-action-bar').length) return; // 頁面沒有 action bar 時跳過
        if (isMobile()) {
            // mobile sidebar 已隱藏，action bar 靠左到底
            $('.kd-action-bar').css('left', '0');
            return;
        }
        var collapsed = $('.sidebar').hasClass('sidebar--collapsed') ||
                        $('.content-wrapper').hasClass('content-wrapper--expanded');
        $('.kd-action-bar').css('left', collapsed ? '0' : sidebarWidth);
    }

    $(document).on('click', '.hamburger-btn', function () {
        if (isMobile()) {
            // 手機：sidebar 用 .sidebar--open 控制，並顯示 overlay
            $('.sidebar').toggleClass('sidebar--open');
            $('#sidebarOverlay').toggleClass('active');
        } else {
            // 桌機/平板：sidebar 用 .sidebar--collapsed 控制
            $('.sidebar').toggleClass('sidebar--collapsed');
            $('.content-wrapper').toggleClass('content-wrapper--expanded');
        }
        syncActionBar(); // sidebar 切換後立即同步 action bar
    });

    // 點擊 overlay 關閉 sidebar
    $(document).on('click', '#sidebarOverlay', function () {
        $('.sidebar').removeClass('sidebar--open');
        $('#sidebarOverlay').removeClass('active');
    });

    // 視窗大小改變時重置狀態並同步 action bar
    $(window).on('resize', function () {
        if (!isMobile()) {
            $('.sidebar').removeClass('sidebar--open');
            $('#sidebarOverlay').removeClass('active');
        }
        syncActionBar();
    });

    // 初始化：頁面載入時同步一次
    syncActionBar();

    /* ==========================================
       Toast Close
    =========================================== */
    $(document).on('click', '#toastClose', function () {
        $('#toastNotif').fadeOut(300);
    });

    /* ==========================================
       Filter Tags Toggle (cases page)
    =========================================== */
    $(document).on('click', '.filter-tag', function () {
        $(this).toggleClass('active');
    });

    /* ==========================================
       Referral Detail Page（導購明細頁）
    =========================================== */
    if ($('#orderBody').length) {

        function applyFilter() {
            var caseVal   = $('#filterCase').val();
            var kolVal    = $('#filterKol').val().trim().toLowerCase();
            var statusVal = $('#filterStatus').val();

            $('#orderBody tr').each(function () {
                var $tr        = $(this);
                var rowCase    = $tr.data('case');
                var rowStatus  = $tr.data('status');
                var rowKol     = ($tr.data('kol') || '').toLowerCase();

                var matchCase   = !caseVal   || rowCase   === caseVal;
                var matchStatus = !statusVal || rowStatus === statusVal;
                var matchKol    = !kolVal    || rowKol.indexOf(kolVal) !== -1;

                $tr.toggle(matchCase && matchStatus && matchKol);
            });

            /* 更新異常交易數 */
            var abnormal = 0;
            $('#orderBody tr:visible').each(function () {
                if ($(this).data('status') === 'abnormal') abnormal++;
            });
            $('#abnormalCount').text(abnormal);
        }

        $('#applyBtn').on('click', applyFilter);

        $('#clearBtn').on('click', function () {
            $('#filterCase').val('');
            $('#filterKol').val('');
            $('#filterSource').val('');
            $('#filterStatus').val('');
            $('#filterDateFrom').val('');
            $('#filterDateTo').val('');
            $('#orderBody tr').show();
            $('#abnormalCount').text(
                $('#orderBody tr[data-status="abnormal"]').length
            );
        });

        /* 分頁按鈕切換（示意） */
        $(document).on('click', '.page-btn[data-page]', function () {
            $('#pagination .page-btn').removeClass('active');
            $(this).addClass('active');
        });

        /* 匯出報表（示意） */
        $('#exportBtn').on('click', function () {
            alert('報表匯出功能即將上線');
        });
    }

    /* ==========================================
       Accepting Page（成果驗收頁）
    =========================================== */
    if ($('#acPassBtn').length) {

        $('#acBackBtn').on('click', function () {
            window.location.href = 'cases-detail.html';
        });

        $('#acReviseBtn').on('click', function () {
            var comment = $('#reviewComment').val().trim();
            if (!comment) {
                alert('請先填寫審核意見再退回修改。');
                $('#reviewComment').focus();
                return;
            }
            alert('已退回 KOL 修改，審核意見已送出。');
        });

        $('#acRejectBtn').on('click', function () {
            var comment = $('#reviewComment').val().trim();
            if (!comment) {
                alert('請先填寫驗收不通過的原因。');
                $('#reviewComment').focus();
                return;
            }
            if (confirm('確定驗收不通過？此操作將通知 KOL。')) {
                alert('已標記驗收不通過。');
            }
        });

        $('#acPassBtn').on('click', function () {
            if (confirm('確定驗收通過？系統將進入結案流程。')) {
                alert('驗收通過！系統正在進行結案。');
            }
        });
    }

    /* ==========================================
       Add Cases Page（新增案件頁）
    =========================================== */
    if ($('#recruitDeadline').length) {

        // 收益分成 toggle
        $('#revShareToggle').on('change', function () {
            $('#revSharePanel').toggle(this.checked);
        });

        // 贈品 tag 新增
        $('#giftAddBtn').on('click', function () {
            var val = $('#giftInput').val().trim();
            if (!val) return;
            var tag = $('<span class="ac-tag">' + val + ' <button class="ac-tag__del">×</button></span>');
            $('#giftTags').append(tag);
            $('#giftInput').val('');
        });
        $(document).on('click', '.ac-tag__del', function () {
            $(this).closest('.ac-tag, .ac-file-tag').remove();
        });

        // 上傳區點擊
        $('#uploadZone').on('click', function () { $('#fileInput').trigger('click'); });
        $('#fileInput').on('change', function () {
            $.each(this.files, function (i, f) {
                var tag = $('<span class="ac-file-tag">' + f.name + ' <button class="ac-tag__del">×</button></span>');
                $('#fileList').append(tag);
            });
        });

        // Step tab 切換 + 捲動到對應 section
        $('.ac-tab').on('click', function () {
            $('.ac-tab').removeClass('active');
            $(this).addClass('active');

            var targetId  = $(this).data('target');
            var $target   = $('#' + targetId);
            if ($target.length) {
                var topNavH   = parseInt(getComputedStyle(document.documentElement).getPropertyValue('--topnav-h')) || 56;
                var tabsWrapH = $('.ac-tabs-wrap').outerHeight(true) || 0;
                var offset    = $target.offset().top - topNavH - tabsWrapH - 8;
                $('html, body').animate({ scrollTop: offset }, 350);
            }
        });

        // Tab 左右箭頭捲動（手機版）
        var $tabsEl    = $('.ac-tabs');
        var SCROLL_STEP = 160;

        function updateArrows() {
            var sl    = $tabsEl.scrollLeft();
            var maxSl = $tabsEl[0].scrollWidth - $tabsEl[0].clientWidth;
            $('#tabsArrowLeft').prop('disabled', sl <= 0);
            $('#tabsArrowRight').prop('disabled', sl >= maxSl - 1);
        }

        $('#tabsArrowLeft').on('click', function () {
            $tabsEl.scrollLeft($tabsEl.scrollLeft() - SCROLL_STEP);
            setTimeout(updateArrows, 320);
        });
        $('#tabsArrowRight').on('click', function () {
            $tabsEl.scrollLeft($tabsEl.scrollLeft() + SCROLL_STEP);
            setTimeout(updateArrows, 320);
        });
        $tabsEl.on('scroll', updateArrows);
        updateArrows();
    }

    /* ==========================================
       Publish Page（發佈案件確認頁）
    =========================================== */
    if ($('#pubSubmitBtn').length) {

        function checkConfirm() {
            var allChecked = $('#confirmContent').is(':checked') && $('#confirmBudget').is(':checked');
            $('#pubSubmitBtn').prop('disabled', !allChecked);
        }
        $('#confirmContent, #confirmBudget').on('change', checkConfirm);

        $('#pubBackBtn').on('click', function () {
            window.location.href = 'add-cases.html';
        });

        $('#pubSubmitBtn').on('click', function () {
            if ($(this).prop('disabled')) return;
            alert('案件已送出審核！');
        });
    }

    /* ==========================================
       Referral Page（導購成效總覽頁）
    =========================================== */
    if ($('.rf-time-tab').length) {

        /* 時間範圍切換 */
        $('.rf-time-tab').on('click', function () {
            $('.rf-time-tab').removeClass('rf-time-tab--active');
            $(this).addClass('rf-time-tab--active');
        });

        /* 匯出 CSV（示意） */
        $('#exportCsvBtn').on('click', function () {
            alert('CSV 匯出功能即將上線');
        });

        /* 重新同步（示意） */
        $('#syncBtn').on('click', function () {
            var $btn = $(this);
            $btn.prop('disabled', true).html('<i class="fa-solid fa-arrows-rotate fa-spin"></i> 同步中…');
            setTimeout(function () {
                $btn.prop('disabled', false).html('<i class="fa-solid fa-arrows-rotate"></i> 重新同步資料');
            }, 1800);
        });
    }

    /* ==========================================
       Transaction History Page（交易紀錄明細頁）
    =========================================== */
    if ($('#txnBody').length) {

        function applyTxnFilter() {
            var type     = $('#filterType').val();
            var status   = $('#filterStatus').val();
            var keyword  = $('#filterKeyword').val().trim().toLowerCase();
            var dateFrom = $('#dateFrom').val();
            var dateTo   = $('#dateTo').val();
            var visible  = 0;

            $('#txnBody tr').each(function () {
                var $tr       = $(this);
                var rowType   = $tr.data('type');
                var rowStatus = $tr.data('status');
                var rowKw     = ($tr.data('keyword') || '').toString().toLowerCase();
                var rowDate   = $tr.find('.th-txn-date').text().trim().slice(0, 10);

                var matchType   = !type     || rowType   === type;
                var matchStatus = !status   || rowStatus === status;
                var matchKw     = !keyword  || rowKw.indexOf(keyword) !== -1 ||
                                  $tr.text().toLowerCase().indexOf(keyword) !== -1;
                var matchFrom   = !dateFrom || rowDate >= dateFrom;
                var matchTo     = !dateTo   || rowDate <= dateTo;

                var show = matchType && matchStatus && matchKw && matchFrom && matchTo;
                $tr.toggle(show);
                if (show) visible++;
            });

            $('#txnFooter').toggle(visible > 0);
            $('#txnEmpty').toggle(visible === 0);
        }

        $('#searchBtn').on('click', applyTxnFilter);

        $('#filterKeyword').on('keydown', function (e) {
            if (e.key === 'Enter') applyTxnFilter();
        });
    }

    /* ==========================================
       Wallet Page（錢包管理頁）
    =========================================== */
    if ($('#loadMoreBtn').length) {

        // 交易記錄分批顯示
        var PAGE_SIZE = 10;
        var $rows = $('.wl-table tbody tr');
        var total  = $rows.length;
        var shown  = 0;

        function showNextPage() {
            var end = Math.min(shown + PAGE_SIZE, total);
            $rows.slice(shown, end).show();
            shown = end;
            if (shown >= total) {
                $('#loadMoreBtn').hide();
            }
        }

        $rows.hide();
        showNextPage();

        $('#loadMoreBtn').on('click', function () {
            showNextPage();
        });

        // 送出儲值
        $('#topupSubmitBtn').on('click', function () {
            var amount = $('#topupAmount').val();
            var email  = $('#invoiceEmail').val().trim();
            if (!amount || parseInt(amount) < 1000) {
                alert('請輸入至少 NT$1,000 的儲值金額。');
                $('#topupAmount').focus();
                return;
            }
            if (!email) {
                alert('請填寫電子信箱以接收發票。');
                $('#invoiceEmail').focus();
                return;
            }
            if (confirm('確定送出儲值 NT$' + parseInt(amount).toLocaleString() + '？')) {
                alert('儲值申請已送出，請依指示完成付款。');
            }
        });

        // 聯繫客服
        $('.wl-help__btn').on('click', function () {
            alert('正在轉接客服，請稍候。');
        });

        // 篩選
        $('.wl-filter-btn').on('click', function () {
            alert('篩選功能開發中。');
        });
    }

    /* ==========================================
       Company Info Page（企業資料維護頁）
    =========================================== */
    if ($('#ciContactList').length) {

        // 新增聯絡人卡片範本
        var contactCardTpl = function () {
            return '<div class="ci-contact-card">' +
                '<div class="ci-contact-card__header">' +
                    '<span class="ci-contact-card__name">新增聯絡人</span>' +
                    '<button class="ci-contact-card__remove">移除</button>' +
                '</div>' +
                '<div class="ci-form-grid">' +
                    '<div class="ci-form-group"><label class="ci-form-label">電話</label><input type="tel" class="ci-form-input" placeholder="02-xxxxxxxx"></div>' +
                    '<div class="ci-form-group"><label class="ci-form-label">分機</label><input type="text" class="ci-form-input" placeholder="xxx"></div>' +
                    '<div class="ci-form-group"><label class="ci-form-label">手機</label><input type="tel" class="ci-form-input" placeholder="09xxxxxxxx"></div>' +
                    '<div class="ci-form-group"><label class="ci-form-label">電子信箱</label><input type="email" class="ci-form-input" placeholder="example@email.com"></div>' +
                    '<div class="ci-form-group"><label class="ci-form-label">傳真</label><input type="tel" class="ci-form-input" placeholder="02-xxxxxxxx"></div>' +
                    '<div class="ci-form-group"><label class="ci-form-label">備註</label><input type="text" class="ci-form-input" placeholder="備註說明"></div>' +
                '</div>' +
            '</div>';
        };

        // 新增聯絡人
        $('#ciAddContactBtn').on('click', function () {
            $('#ciContactList').append(contactCardTpl());
        });

        // 移除聯絡人（事件委派）
        $(document).on('click', '.ci-contact-card__remove', function () {
            if (confirm('確定移除此聯絡人？')) {
                $(this).closest('.ci-contact-card').remove();
            }
        });

        // 儲存變更（頂部 & 底部）
        function handleSave() {
            alert('資料已儲存。');
        }
        $('#ciSaveTopBtn, #ciSaveBottomBtn').on('click', handleSave);

        // 取消
        function handleCancel() {
            if (confirm('確定取消？未儲存的變更將會遺失。')) {
                history.back();
            }
        }
        $('#ciCancelBtn, #ciCancelBottomBtn').on('click', handleCancel);

        // 查看異動紀錄
        $('#ciAuditLogBtn').on('click', function () {
            alert('異動紀錄功能開發中。');
        });

        // Sidebar 收合時同步 action bar left 值
        function syncCiActionBar() {
            if ($(window).width() <= 768) {
                $('#ciActionBar').css('left', '0');
                return;
            }
            var collapsed = $('.sidebar').hasClass('sidebar--collapsed') ||
                            $('.content-wrapper').hasClass('content-wrapper--expanded');
            var sw = getComputedStyle(document.documentElement)
                        .getPropertyValue('--sidebar-width').trim();
            $('#ciActionBar').css('left', collapsed ? '0' : sw);
        }
        syncCiActionBar();
        $(document).on('click', '.hamburger-btn', function () {
            setTimeout(syncCiActionBar, 260);
        });
        $(window).on('resize', syncCiActionBar);
    }

    /* ==========================================
       Permission Page（使用者與權限管理頁）
    =========================================== */
    if ($('#pmTableBody').length) {

        // 搜尋 + 角色篩選
        function applyPmFilter() {
            var keyword = $('#pmSearchInput').val().trim().toLowerCase();
            var role    = $('#pmRoleFilter').val();
            var count   = 0;

            $('#pmTableBody tr').each(function () {
                var $tr      = $(this);
                var text     = $tr.text().toLowerCase();
                var rowRole  = $tr.find('.pm-role-badge').text().trim().toUpperCase();
                var matchKw  = !keyword || text.indexOf(keyword) !== -1;
                var matchRole = !role   || rowRole === role;
                var show = matchKw && matchRole;
                $tr.toggle(show);
                if (show) count++;
            });

            $('.pm-pagination-info').text(
                '顯示第 1 至 ' + count + ' 名成員，共 ' + count + ' 名成員'
            );
        }

        $('#pmConfirmFilterBtn').on('click', applyPmFilter);
        $('#pmSearchInput').on('keydown', function (e) {
            if (e.key === 'Enter') applyPmFilter();
        });
        $('#pmRoleFilter').on('change', applyPmFilter);

        // 新增成員（示意）
        $('#pmAddMemberBtn').on('click', function () {
            alert('新增成員功能開發中。');
        });

        // 操作按鈕（事件委派）
        $(document).on('click', '#pmTableBody .pm-btn--outline', function () {
            var label = $(this).text().trim();
            if (label === '編輯') {
                var name = $(this).closest('tr').find('.pm-user-name').text().trim();
                alert('編輯成員：' + name);
            } else if (label === '停用') {
                var name = $(this).closest('tr').find('.pm-user-name').text().trim();
                if (confirm('確定停用 ' + name + '？')) {
                    $(this).closest('tr').find('.pm-status')
                        .removeClass('pm-status--active pm-status--pending')
                        .addClass('pm-status--disabled')
                        .html('<span class="pm-status__dot"></span>已停用');
                    $(this).text('啟用');
                }
            } else if (label === '啟用') {
                var name = $(this).closest('tr').find('.pm-user-name').text().trim();
                if (confirm('確定啟用 ' + name + '？')) {
                    $(this).closest('tr').find('.pm-status')
                        .removeClass('pm-status--disabled pm-status--pending')
                        .addClass('pm-status--active')
                        .html('<span class="pm-status__dot"></span>已啟用');
                    $(this).text('停用');
                }
            } else if (label === '權限轉移') {
                alert('權限轉移功能開發中。');
            } else if (label === '邀請重發') {
                alert('邀請信已重新發送。');
            }
        });

        // 刪除成員
        $(document).on('click', '#pmTableBody .pm-btn--danger', function () {
            var name = $(this).closest('tr').find('.pm-user-name').text().trim();
            if (confirm('確定刪除成員 ' + name + '？此操作無法復原。')) {
                $(this).closest('tr').fadeOut(300, function () { $(this).remove(); });
            }
        });
    }

    /* ==========================================
       Add User Page（新增成員頁）
    =========================================== */
    if ($('#auSubmitBtn').length) {

        var auRoleDescMap = {
            member: '一般成員僅能瀏覽案件，無法修改系統設定或管理其他成員。',
            admin:  '管理員擁有修改所有案件資訊與成員管理的權限，但無法轉移擁有者身份。',
            owner:  '擁有者具有最高權限，包含帳號管理、角色轉移及所有系統設定。'
        };

        // 角色說明同步更新
        $('#auRole').on('change', function () {
            $('#auRoleDesc').text(auRoleDescMap[$(this).val()] || '');
        });

        // 發送邀請
        $('#auSubmitBtn').on('click', function () {
            var name  = $.trim($('#auName').val());
            var email = $.trim($('#auEmail').val());

            if (!name) {
                alert('請輸入成員姓名');
                $('#auName').focus();
                return;
            }
            if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                alert('請輸入正確的電子郵件地址');
                $('#auEmail').focus();
                return;
            }
            if (confirm('確定發送邀請給 ' + name + '（' + email + '）？')) {
                alert('邀請已發送至 ' + email);
                window.location.href = 'permission.html';
            }
        });

        // Enter 鍵觸發送出
        $('#auName, #auEmail').on('keydown', function (e) {
            if (e.key === 'Enter') $('#auSubmitBtn').trigger('click');
        });
    }

    /* ==========================================
       Permission Management Page（角色權限管理頁）
    =========================================== */
    if ($('#prmPermPanels').length) {

        // 角色資料
        var prmRoles = {
            owner: {
                name: 'Owner',
                desc: '系統最高權限擁有者。擁有所有案件管理、財務設置與系統設定之完整控制權，通常為企業負責人或主要管理窗口。',
                core: ['全站功能完全存取', '管理財務及錢包儲值', '編輯及刪除其他管理員']
            },
            admin: {
                name: 'Admin',
                desc: '管理員擁有案件管理與財務查看的完整權限，可管理成員但無法刪除 Owner 或轉移擁有者身份。',
                core: ['新增及編輯所有案件', '查看財務報表', '新增與停用成員']
            },
            member: {
                name: 'Member',
                desc: '一般成員僅能瀏覽案件資訊與自身相關資料，無法修改系統設定或存取財務功能。',
                core: ['瀏覽公開案件', '查看個人帳號資訊']
            }
        };

        // 切換角色 Tab
        $(document).on('click', '.prm-role-tab', function () {
            var role = $(this).data('role');
            $('.prm-role-tab').removeClass('active');
            $(this).addClass('active');

            var data = prmRoles[role];
            if (!data) return;

            $('#prmRoleName').text(data.name);
            $('#prmRoleDesc').text(data.desc);

            var coreHtml = '';
            $.each(data.core, function (i, item) {
                coreHtml += '<li class="prm-core-item"><i class="fa-regular fa-circle-check"></i>' + item + '</li>';
            });
            $('.prm-core-list').html(coreHtml);
        });

        // 全選 checkbox 控制同群組
        $(document).on('change', '.prm-select-all', function () {
            var group   = $(this).data('group');
            var checked = $(this).prop('checked');
            $('input[type="checkbox"][data-group="' + group + '"]').not(this).prop('checked', checked);
        });

        // 子項目勾選影響全選狀態
        $(document).on('change', '.prm-perm-item__check input[type="checkbox"]', function () {
            var group  = $(this).data('group');
            var $items = $('.prm-perm-item__check input[data-group="' + group + '"]');
            var allChecked = $items.length === $items.filter(':checked').length;
            $('.prm-select-all[data-group="' + group + '"]').prop('checked', allChecked);
        });

        // 新增角色
        $('#prmAddRoleBtn').on('click', function () {
            alert('新增角色功能開發中。');
        });

        // 儲存變更
        $('#prmSaveBtn').on('click', function () {
            alert('權限設定已儲存。');
        });

        // 取消
        $('#prmCancelBtn').on('click', function () {
            if (confirm('確定取消？未儲存的變更將會遺失。')) {
                history.back();
            }
        });

        // Sidebar 收合時同步 action bar
        function syncPrmActionBar() {
            if ($(window).width() <= 1024) {
                $('#prmActionBar').css('left', '0');
                return;
            }
            var collapsed = $('.sidebar').hasClass('sidebar--collapsed') ||
                            $('.content-wrapper').hasClass('content-wrapper--expanded');
            var sw = getComputedStyle(document.documentElement)
                        .getPropertyValue('--sidebar-width').trim();
            $('#prmActionBar').css('left', collapsed ? '0' : sw);
        }
        syncPrmActionBar();
        $(document).on('click', '.hamburger-btn', function () {
            setTimeout(syncPrmActionBar, 260);
        });
        $(window).on('resize', syncPrmActionBar);
    }

});