(() => {
  const root = document.querySelector('#classroom');
  const sessionId = root.dataset.session;
  const csrf = root.querySelector('[name=__RequestVerificationToken]').value;
  const feedback = document.querySelector('#classroom-feedback');
  const labels = { Scheduled: '未點名', Present: '出席', Late: '遲到', Absent: '缺席' };
  const local = value => new Date(value && !/Z$|[+-]\d\d:\d\d$/.test(value) ? value + 'Z' : value).toLocaleString('zh-TW', { timeZone: 'Asia/Taipei' });
  async function request(action, body) {
    const response = await fetch('/Classroom/' + action, body ? { method: 'POST', headers: { 'X-CSRF-TOKEN': csrf }, body: new URLSearchParams(body) } : { cache: 'no-store' });
    if (response.redirected) throw new Error('登入已逾時，請重新登入後開啟此頁。');
    const data = await response.json();
    if (!response.ok) throw new Error(data.message || '無法更新，請重新整理並確認權限。');
    return data;
  }
  let polling = false;
  async function refresh() {
    if (polling || document.hidden) return;
    polling = true;
    try {
      const data = await request('Snapshot?sessionId=' + sessionId);
      document.querySelector('#open-qr').disabled = !data.canDisplayQr;
      if (!data.canDisplayQr) { document.querySelector('#qr-display').hidden = true; document.querySelector('#qr-image').removeAttribute('src'); }
      for (const record of data.records) {
        const form = root.querySelector(`[data-id="${record.sessionParticipantId}"]`);
        if (!form) continue;
        form.querySelector('.live-status').textContent = labels[record.status] || record.status;
        form.querySelector('.live-time').textContent = record.checkedInAtUtc ? '首次到課：' + local(record.checkedInAtUtc) : '';
        if (!form.dataset.dirty) {
          form.elements.RowVersion.value = record.rowVersion;
          if (form.elements.Status) form.elements.Status.value = record.status;
          if (form.elements.ManualReason) form.elements.ManualReason.value = record.manualReason || '';
        }
        const button = form.querySelector('.save-attendance');
        if (button && !form.dataset.saving) button.disabled = new Date(data.serverNow) < new Date(root.dataset.start);
      }
      const issues = document.querySelector('#check-in-issues');
      // Preserve a note currently being typed while refreshing statuses.
      if (!issues.contains(document.activeElement)) {
        issues.replaceChildren();
        if (!data.issues.length) issues.textContent = '目前沒有打卡異常。';
        for (const issue of data.issues) {
          const box = document.createElement('form'); box.className = 'card p-3 mb-2';
          const text = document.createElement('p'); text.textContent = `${issue.student} · ${local(issue.createdAt)} · ${issue.message}`; box.append(text);
          if (issue.resolvedAt) { const p = document.createElement('p'); p.textContent = '已處理：' + issue.resolution; box.append(p); }
          else {
            const input = document.createElement('input'); input.required = true; input.maxLength = 500; input.className = 'form-control'; input.placeholder = '填寫處理說明'; input.setAttribute('aria-label', '異常處理說明');
            const button = document.createElement('button'); button.className = 'btn btn-outline-primary mt-2'; button.textContent = '記錄處理結果'; box.append(input, button);
            box.addEventListener('submit', async e => { e.preventDefault(); button.disabled = true; try { await request('Resolve', { id: issue.id, reason: input.value }); input.blur(); await refresh(); } catch (err) { if (window.siteNotification) window.siteNotification(err.message, 'error'); else feedback.textContent = err.message; } finally { button.disabled = false; } });
          }
          issues.append(box);
        }
      }
    } catch (e) { feedback.textContent = e.message; document.querySelector('#qr-display').hidden = true; document.querySelector('#qr-image').removeAttribute('src'); }
    finally { polling = false; }
  }
  root.querySelectorAll('.attendance-row').forEach(form => {
    form.addEventListener('input', () => { form.dataset.dirty = 'true'; });
    form.addEventListener('submit', async e => {
      e.preventDefault(); if (form.dataset.saving) return;
      form.dataset.saving = 'true'; const button = form.querySelector('button'); button.disabled = true;
      try { const result = await request('Record', Object.fromEntries(new FormData(form))); delete form.dataset.dirty; if (window.siteNotification) window.siteNotification(result.message); else feedback.textContent = result.message; }
      catch(e) { if (window.siteNotification) window.siteNotification(e.message, 'error'); else feedback.textContent = e.message; }
      finally { delete form.dataset.saving; button.disabled = false; await refresh(); }
    });
  });
  document.querySelector('#open-qr').addEventListener('click', async e => {
    e.target.disabled = true;
    try { const data = await request('Qr', { sessionId }); document.querySelector('#qr-image').src = data.image; document.querySelector('#qr-expiry').textContent = '有效至 ' + local(data.expiresAt); document.querySelector('#qr-warning').textContent = data.warning || ''; document.querySelector('#qr-display').hidden = false; }
    catch(e) { if (window.siteNotification) window.siteNotification(e.message, 'error'); else feedback.textContent = e.message; } finally { await refresh(); }
  });
  refresh(); setInterval(refresh, 5000); document.addEventListener('visibilitychange', refresh);
})();
