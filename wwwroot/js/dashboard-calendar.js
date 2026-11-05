(() => {
    const element = document.getElementById('calendar');
    const error = document.getElementById('calendarError');
    const legend = document.getElementById('calendarLegend');
    const colorBy = document.getElementById('calendarColorBy');
    const panel = element.closest('.calendar-panel');
    const narrow = window.matchMedia('(max-width: 767.98px)');
    const timeZone = 'Asia/Taipei';
    const timeFormat = { hour: '2-digit', minute: '2-digit', hour12: false };
    const clock = new Intl.DateTimeFormat('zh-TW', { ...timeFormat, timeZone });

    if (!window.FullCalendar?.Luxon3 || !window.luxon) {
        error.textContent = '行事曆元件載入失敗，請確認網路連線後重新整理。';
        error.classList.remove('d-none');
        return;
    }

    // Persisted IDs keep colours stable even when a different range is fetched.
    function hueFor(id) {
        return Number((BigInt(id) * 137n + 19n) % 360n);
    }

    function sessionContent({ event, view, timeText }, h) {
        const { courseName, termName, teacherName } = event.extendedProps;
        const name = h('strong', { className: 'calendar-session-name' }, courseName);
        if (view.type === 'listMonth') {
            // FullCalendar renders the time and colour marker in separate list columns.
            return h('a', { href: event.url, className: 'calendar-session-link' },
                name, h('span', { className: 'calendar-session-meta' }, [termName, teacherName].filter(Boolean).join(' · ')));
        }
        return h('span', { className: 'calendar-session' },
            h('span', { className: 'calendar-session-time' }, timeText),
            h('span', { className: 'calendar-session-start' }, clock.format(event.start)), name);
    }

    function updateLegend() {
        const { activeStart, activeEnd } = calendar.view;
        const groups = new Map();
        // FullCalendar may reuse a month's events when navigating to a week or day.
        for (const event of calendar.getEvents()) {
            if (event.start >= activeEnd || event.end <= activeStart) continue;
            const { termId, termName, courseId, courseName } = event.extendedProps;
            if (colorBy.value === 'course') groups.set(courseId, `${courseName}（${termName}）`);
            else groups.set(termId, termName);
        }
        const chips = [...groups].sort((a, b) => a[1].localeCompare(b[1], 'zh-Hant'))
            .map(([id, name]) => {
                const chip = document.createElement('li');
                chip.style.setProperty('--calendar-hue', hueFor(id));
                chip.textContent = name;
                return chip;
            });
        legend.querySelector('ul').replaceChildren(...chips);
        legend.hidden = chips.length === 0;
    }

    const calendar = new FullCalendar.Calendar(element, {
        initialView: narrow.matches ? 'listMonth' : 'dayGridMonth',
        locale: 'zh-tw',
        timeZone,
        headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,timeGridWeek,timeGridDay,listMonth' },
        buttonText: { today: '今天', month: '月', week: '週', day: '日', list: '清單' },
        noEventsText: '這個月沒有課程場次。',
        height: 'auto',
        nowIndicator: true,
        fixedWeekCount: false,
        dayMaxEvents: 3,
        moreLinkText: n => `另 ${n} 場`,
        displayEventEnd: true,
        eventTimeFormat: timeFormat,
        slotLabelFormat: timeFormat,
        eventDisplay: 'block',
        eventContent: sessionContent,
        eventDidMount({ el, event }) {
            const { termId, termName, courseId } = event.extendedProps;
            el.style.setProperty('--term-hue', hueFor(termId));
            el.style.setProperty('--course-hue', hueFor(courseId));
            const label = `${clock.format(event.start)}–${clock.format(event.end)}　${event.title}（${termName}）`;
            el.title = label;
            (el.matches('a') ? el : el.querySelector('a'))?.setAttribute('aria-label', label);
        },
        eventsSet: updateLegend,
        datesSet: updateLegend,
        events: {
            url: element.dataset.eventsUrl,
            success: () => error.classList.add('d-none'),
            failure: () => error.classList.remove('d-none')
        }
    });

    panel.dataset.colorBy = colorBy.value;
    colorBy.addEventListener('change', () => {
        panel.dataset.colorBy = colorBy.value;
        updateLegend();
    });
    calendar.render();
    narrow.addEventListener('change', ({ matches }) => {
        if (matches && calendar.view.type === 'dayGridMonth') calendar.changeView('listMonth');
        if (!matches && calendar.view.type === 'listMonth') calendar.changeView('dayGridMonth');
    });
})();
