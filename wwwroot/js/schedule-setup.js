(() => {
    const forms = document.querySelectorAll('[data-setup-form]');
    forms.forEach(form => form.addEventListener('submit', event => {
        if (form.dataset.submitting === 'true') { event.preventDefault(); return; }
        if (!form.checkValidity() || (window.jQuery?.validator && !window.jQuery(form).valid())) return;
        // Keep the clicked button's name/value in the request (NextCourse uses it).
        form.dataset.submitting = 'true';
        form.setAttribute('aria-busy', 'true');
    }));
    window.addEventListener('pageshow', () => forms.forEach(form => {
        delete form.dataset.submitting;
        form.removeAttribute('aria-busy');
    }));
})();
