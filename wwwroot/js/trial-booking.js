(async () => {
  const form = document.querySelector('#trial-booking-form'); if (!form) return;
  const host = document.querySelector('#trial-selects'), add = document.querySelector('#add-trial-session'), submit = document.querySelector('#submit-trial-booking'), message = document.querySelector('#trial-booking-message');
  try {
    const response = await fetch('/Admin/TrialDesk/Options?id=' + form.dataset.request, { cache: 'no-store' });
    if (response.redirected) throw new Error('請重新登入。');
    const data = await response.json(); if (!response.ok) throw new Error(data.message || '無法載入場次');
    form.elements.rowVersion.value = data.rowVersion;
    function update() { add.disabled = host.children.length >= data.remaining || !data.sessions.length; submit.disabled = !host.children.length; }
    function append() {
      if (host.children.length >= data.remaining) return;
      const row = document.createElement('div'), label = document.createElement('label'), select = document.createElement('select'), remove = document.createElement('button');
      label.textContent = '預約場次'; select.name = 'sessionIds'; select.className = 'form-select'; select.required = true; select.add(new Option('請選擇場次', ''));
      data.sessions.forEach(s => select.add(new Option(`${new Date(s.startsAt).toLocaleString('zh-TW', {timeZone: 'Asia/Taipei'})} · ${s.name} · ${s.teacher} · ${s.location}`, s.id)));
      remove.type = 'button'; remove.textContent = '移除'; remove.className = 'btn btn-outline-secondary'; remove.onclick = () => { row.remove(); update(); };
      label.append(select); row.append(label, remove); host.append(row); update();
    }
    add.onclick = append; if (data.sessions.length && data.remaining > 0) append(); else message.textContent = '目前沒有可預約額度或符合效期的場次。'; update();
    form.onsubmit = e => { const ids = [...form.querySelectorAll('select')].map(x => x.value); if (new Set(ids).size !== ids.length) { e.preventDefault(); window.alert('重複選擇同一堂課，請調整後再送出。'); } else submit.disabled = true; };
  } catch(e) { message.textContent = e.message; }
})();
