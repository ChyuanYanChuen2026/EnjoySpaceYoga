document.querySelectorAll('[data-resolve-error]').forEach(form => form.addEventListener('submit', async event => {
  event.preventDefault(); const button=form.querySelector('button'); button.disabled=true;
  const feedback=form.querySelector('[role=status]');
  try { const response=await fetch(form.action,{method:'POST',body:new URLSearchParams(new FormData(form))});if(response.redirected)throw new Error('請重新登入。'); const data=await response.json(); if(!response.ok)throw new Error(data.message||'無法更新，請重新整理。');if(window.siteNotification)window.siteNotification(data.message);else feedback.textContent=data.message;form.querySelector('input[name=reason]').disabled=true; }
  catch(e){if(window.siteNotification)window.siteNotification(e.message,'error');else feedback.textContent=e.message;button.disabled=false;}
}));
