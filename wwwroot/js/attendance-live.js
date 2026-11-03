(() => {
  const panel = document.querySelector('#attendance-live'); if (!panel) return;
  const forms = [...document.querySelectorAll('form')].filter(f => f.elements.ParticipantId);
  forms.forEach(f => f.addEventListener('input', () => { f.dataset.dirty = 'true'; }));
  const labels = { Scheduled: '未點名', Present: '出席', Late: '遲到', Absent: '缺席' };
  let busy = false;
  async function update() {
    if(busy || document.hidden) return; busy=true;
    try {
      const response=await fetch('/Classroom/Snapshot?sessionId='+panel.dataset.session,{cache:'no-store'});
      if(!response.ok || response.redirected) throw new Error('同步中斷，請重新登入或更新頁面。');
      const data=await response.json();
      for(const r of data.records){ const form=forms.find(f=>Number(f.elements.ParticipantId.value)===r.sessionParticipantId); if(!form) continue;
        const card=form.closest('.card'); const badge=card.querySelector('.badge.bg-secondary'); if(badge) badge.textContent=labels[r.status]||r.status;
        if(!form.dataset.dirty){ form.elements.Status.value=r.status; form.elements.RowVersion.value=r.rowVersion; form.elements.ManualReason.value=r.manualReason || ''; }
        let time=card.querySelector('.qr-arrival-time'); if(!time){time=document.createElement('p');time.className='qr-arrival-time small text-secondary';form.after(time);}
        time.textContent=r.checkedInAtUtc ? '首次到課：'+new Date(r.checkedInAtUtc+'Z').toLocaleString('zh-TW',{timeZone:'Asia/Taipei'}) : '';
      }
      const summary=Object.entries(labels).map(([s,label])=>label+' '+data.records.filter(r=>r.status===s).length+' 人').join('、');
      panel.textContent='出勤同步：'+summary+'。待處理打卡異常 '+data.issues.filter(i=>!i.resolvedAt).length+' 筆。';
    }catch(e){panel.textContent=e.message;}finally{busy=false;}
  }
  update();setInterval(update,5000);document.addEventListener('visibilitychange',update);
})();
