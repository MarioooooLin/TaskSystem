// 日期選擇器初始化

// 建立手機版遮罩
if (!document.getElementById('flatpickrOverlay')) {
    var overlay = document.createElement('div');
    overlay.id = 'flatpickrOverlay';
    overlay.className = 'flatpickr-mobile-overlay';
    document.body.appendChild(overlay);
}

function showOverlay() {
    document.getElementById('flatpickrOverlay').classList.add('active');
}

function hideOverlay() {
    document.getElementById('flatpickrOverlay').classList.remove('active');
}

// 修正日曆面板位置（手機版 fixed 定位時設定 top）
function fixCalendarPosition(cal, inputEl) {
    if (window.innerWidth > 768) return;

    // 先確保隱藏（清掉 fp-ready），避免閃爍
    cal.classList.remove('fp-ready');

    // 用 requestAnimationFrame 等瀏覽器完成第一次 layout 再計算
    requestAnimationFrame(function () {
        var rect    = inputEl.getBoundingClientRect();
        var calH    = cal.offsetHeight;
        var vh      = window.innerHeight;
        var spaceBelow = vh - rect.bottom - 8;
        var spaceAbove = rect.top - 8;

        if (spaceBelow >= calH) {
            cal.style.top = (rect.bottom + 6) + 'px';
        } else if (spaceAbove >= calH) {
            cal.style.top = (rect.top - calH - 6) + 'px';
        } else {
            cal.style.top = Math.max(8, (vh - calH) / 2) + 'px';
        }

        // 定位完成，淡入顯示
        cal.classList.add('fp-ready');
    });
}

// 自訂語系：週標題改為單字
var zhTwCustom = Object.assign({}, flatpickr.l10ns.zh_tw, {
    weekdays: {
        shorthand: ['日', '一', '二', '三', '四', '五', '六'],
        longhand:  ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六']
    }
});

var startPicker = flatpickr('#startDate', {
    locale: zhTwCustom,
    dateFormat: 'Y/m/d',
    disableMobile: true,
    onOpen: function (selectedDates, dateStr, instance) {
        showOverlay();
        fixCalendarPosition(instance.calendarContainer, document.getElementById('startDate'));
    },
    onClose: function (selectedDates, dateStr, instance) {
        hideOverlay();
        instance.calendarContainer.classList.remove('fp-ready');
    },
    onChange: function (selectedDates) {
        if (selectedDates[0]) {
            endPicker.set('minDate', selectedDates[0]);
        }
    }
});

var endPicker = flatpickr('#endDate', {
    locale: zhTwCustom,
    dateFormat: 'Y/m/d',
    disableMobile: true,
    onOpen: function (selectedDates, dateStr, instance) {
        showOverlay();
        fixCalendarPosition(instance.calendarContainer, document.getElementById('endDate'));
    },
    onClose: function (selectedDates, dateStr, instance) {
        hideOverlay();
        instance.calendarContainer.classList.remove('fp-ready');
    },
    onChange: function (selectedDates) {
        if (selectedDates[0]) {
            startPicker.set('maxDate', selectedDates[0]);
        }
    }
});

// 點擊整個 ttm_date_input 區塊都能開啟日曆
document.getElementById('startDateWrap').addEventListener('click', function () {
    startPicker.open();
});
document.getElementById('endDateWrap').addEventListener('click', function () {
    endPicker.open();
});

// 點擊遮罩關閉日曆
document.getElementById('flatpickrOverlay').addEventListener('click', function () {
    startPicker.close();
    endPicker.close();
    hideOverlay();
});
