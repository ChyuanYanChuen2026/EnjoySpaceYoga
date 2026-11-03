import { parseDateInput, formatDateInput } from './date-time-input.mjs';

const td = window.tempusDominus;
if (td && window.Popper) {
    document.querySelectorAll('input[type="date"], input[type="datetime-local"]').forEach(input => {
        if (input.disabled || input.readOnly) return;
        const withTime = input.type === 'datetime-local';
        const initial = input.value;
        const min = parseDateInput(input.min || '', withTime);
        const max = parseDateInput(input.max || '', withTime);
        const format = withTime ? 'yyyy-MM-dd HH:mm' : 'yyyy-MM-dd';
        const label = input.labels?.[0]?.textContent.trim() || (withTime ? '日期時間' : '日期');
        const wrapper = document.createElement('div');
        wrapper.className = 'input-group es-date-picker';
        wrapper.dataset.tdTargetToggle = '.es-picker-toggle';
        // Preserve spacing for the teacher forms that put margins on the input.
        for (const name of [...input.classList]) {
            if (/^mb-\d$/.test(name)) { wrapper.classList.add(name); input.classList.remove(name); }
        }
        input.before(wrapper);
        wrapper.append(input);
        const toggle = document.createElement('button');
        toggle.type = 'button';
        toggle.className = 'btn btn-outline-secondary es-picker-toggle';
        toggle.setAttribute('aria-label', `選擇${label}`);
        toggle.setAttribute('aria-haspopup', 'dialog');
        toggle.innerHTML = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 2v6m10-6v6M3 11h18"/></svg>';
        wrapper.append(toggle);
        input.type = 'text';
        input.value = ''; // Configure strict parsing before loading an existing value.
        input.placeholder = format;
        input.autocomplete = 'off';
        input.dataset.dateTimePicker = withTime ? 'datetime' : 'date';
        const help = document.createElement('div');
        help.id = `${input.id || input.name.replaceAll('.', '_')}-date-help`;
        help.className = 'form-text mb-2';
        help.textContent = `輸入格式：${format}，也可點選月曆。${withTime ? '時間採 24 小時制（台灣時間）。' : ''}`;
        wrapper.after(help);
        input.setAttribute('aria-describedby', [input.getAttribute('aria-describedby'), help.id].filter(Boolean).join(' '));
        function validate() {
            const value = input.value.trim();
            const date = value ? parseDateInput(value, withTime) : null;
            let error = value && !date ? `請輸入有效的日期${withTime ? '時間' : ''}，格式為 ${format}。` : '';
            if (date && ((min && date < min) || (max && date > max))) error = '日期時間超出允許範圍。';
            input.setCustomValidity(error);
            input.setAttribute('aria-invalid', error ? 'true' : 'false');
            return !error;
        }
        const picker = new td.TempusDominus(wrapper, {
            allowInputToggle: true,
            useCurrent: false,
            keepInvalid: true,
            stepping: 1,
            restrictions: { ...(min ? { minDate: min } : {}), ...(max ? { maxDate: max } : {}) },
            display: {
                theme: 'light',
                buttons: { today: false, clear: true, close: true },
                components: { clock: withTime, hours: withTime, minutes: withTime, seconds: false },
                icons: { type: 'icons', time: 'es-td-clock', date: 'es-td-calendar', up: 'es-td-up', down: 'es-td-down',
                    previous: 'es-td-prev', next: 'es-td-next', today: 'es-td-calendar', clear: 'es-td-clear', close: 'es-td-close' }
            },
            localization: {
                locale: 'zh-TW', format, hourCycle: 'h23', startOfTheWeek: 0,
                dayViewHeaderFormat: { year: 'numeric', month: 'long' },
                clear: '清除', close: '完成', selectMonth: '選擇月份', previousMonth: '上個月', nextMonth: '下個月',
                selectYear: '選擇年份', previousYear: '上一年', nextYear: '下一年', selectDecade: '選擇年代',
                previousDecade: '前十年', nextDecade: '後十年', previousCentury: '前一世紀', nextCentury: '後一世紀',
                pickHour: '選擇小時', incrementHour: '增加一小時', decrementHour: '減少一小時',
                pickMinute: '選擇分鐘', incrementMinute: '增加一分鐘', decrementMinute: '減少一分鐘',
                selectTime: '選擇時間', selectDate: '選擇日期'
            }
        });
        picker.dates.parseInput = value => {
            const parsed = typeof value === 'string' ? parseDateInput(value, withTime) : value;
            return parsed instanceof Date && !Number.isNaN(parsed.valueOf()) ? td.DateTime.convert(parsed) : undefined;
        };
        // Leave invalid typed values visible so the user can fix them; never silently clear or normalize them.
        input.value = initial;
        if (initial && validate()) picker.dates.setFromInput(initial);
        input.addEventListener('input', validate);
        input.addEventListener('change', validate);
        let timeInput;
        picker.subscribe(td.Namespace.events.show, () => {
            toggle.setAttribute('aria-expanded', 'true');
            const widget = picker.display.widget;
            widget.setAttribute('role', 'dialog');
            widget.setAttribute('aria-label', `選擇${label}`);
            if (withTime && !widget.querySelector('.es-picker-time-entry')) {
                const row = document.createElement('label');
                row.className = 'es-picker-time-entry';
                row.textContent = '時間（24 小時制）';
                timeInput = document.createElement('input');
                timeInput.type = 'text'; timeInput.placeholder = 'HH:mm'; timeInput.maxLength = 5; timeInput.className = 'form-control';
                timeInput.setAttribute('aria-label', '輸入時間（24 小時制）');
                row.append(timeInput); widget.append(row);
                timeInput.addEventListener('change', () => {
                    const date = parseDateInput(input.value, true) || (picker.dates.lastPicked ? new Date(picker.dates.lastPicked.valueOf()) : null);
                    if (!date) return;
                    if (!/^(?:[01]\d|2[0-3]):[0-5]\d$/.test(timeInput.value)) {
                        input.value = `${formatDateInput(date, false)} ${timeInput.value}`;
                        validate();
                        timeInput.setCustomValidity('請輸入 00:00 至 23:59 的時間。'); return;
                    }
                    timeInput.setCustomValidity('');
                    const [hour, minute] = timeInput.value.split(':').map(Number);
                    date.setHours(hour, minute, 0, 0);
                    picker.dates.setValue(td.DateTime.convert(date));
                });
            }
            syncTime();
        });
        function syncTime() {
            if (timeInput) {
                const date = parseDateInput(input.value, true);
                timeInput.value = date ? formatDateInput(date, true).slice(11) : '';
                timeInput.setCustomValidity('');
                timeInput.disabled = !date;
            }
        }
        picker.subscribe(td.Namespace.events.change, () => { validate(); syncTime(); });
        picker.subscribe(td.Namespace.events.hide, () => { toggle.setAttribute('aria-expanded', 'false'); validate(); });
        input.form?.addEventListener('reset', () => setTimeout(() => {
            picker.dates.clear(); input.value = initial;
            if (initial && validate()) picker.dates.setFromInput(initial);
            validate();
        }, 0));
        input.form?.addEventListener('submit', event => {
            if (!validate()) { event.preventDefault(); event.stopImmediatePropagation(); input.reportValidity(); }
        }, true);
    });
}
