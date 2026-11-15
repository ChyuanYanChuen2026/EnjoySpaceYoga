(() => {
    const element = document.getElementById('feedbackNotification');
    if (!element || element.dataset.initialized === 'true') return;
    element.dataset.initialized = 'true';
    const title = element.querySelector('#feedbackNotificationTitle');
    const body = element.querySelector('#feedbackNotificationMessage');
    const modal = bootstrap.Modal.getOrCreateInstance(element);
    const pending = [];
    let current = null;
    let returnFocus = null;

    function showNext() {
        if (current || pending.length === 0) return;
        current = pending.shift();
        returnFocus = document.activeElement;
        element.dataset.kind = current.kind;
        title.textContent = current.kind === 'error' ? '操作未完成' : '操作成功';
        body.textContent = current.message;
        modal.show();
    }

    window.siteNotification = (message, kind = 'success') => {
        if (typeof message !== 'string' || !message.trim()) return;
        const item = { message, kind: kind === 'error' ? 'error' : 'success' };
        const same = other => other && other.message === item.message && other.kind === item.kind;
        if (same(current) || pending.some(same)) return;
        pending.push(item);
        showNext();
    };

    element.addEventListener('hidden.bs.modal', () => {
        current = null;
        if (returnFocus instanceof HTMLElement && returnFocus.isConnected) returnFocus.focus();
        showNext();
    });

    // 讀取後移除，避免重複初始化時再次顯示同一份伺服器通知。
    const success = element.dataset.success;
    const legacySuccess = element.dataset.successMessage;
    const error = element.dataset.error;
    delete element.dataset.success;
    delete element.dataset.successMessage;
    delete element.dataset.error;
    window.siteNotification(error, 'error');
    window.siteNotification(success);
    window.siteNotification(legacySuccess);
})();
