const collator = new Intl.Collator('zh-Hant', { numeric: true, sensitivity: 'base' });
export function compareValues(a, b, type, descending) {
    const empty = value => value == null || value.trim() === '' || (type === 'number' && !Number.isFinite(Number(value)));
    if (empty(a) || empty(b)) return empty(a) === empty(b) ? 0 : empty(a) ? 1 : -1;
    const result = type === 'number' ? Number(a) - Number(b) : collator.compare(a, b);
    return descending ? -result : result;
}

// Only full, unpaged detail tables opt in. Moving existing rows keeps form state intact.
export function initializeTable(table) {
    const body = table.tBodies[0];
    if (!body) return;
    const headers = [...table.querySelectorAll('thead th[data-sort-type]')];
    const rows = [...body.rows];
    headers.forEach(header => {
        const label = header.textContent.trim();
        const button = document.createElement('button');
        button.type = 'button'; button.className = 'table-sort-link table-sort-button';
        const icon = document.createElement('span');
        icon.className = 'table-sort-icon'; icon.setAttribute('aria-hidden', 'true'); icon.textContent = '↕';
        button.append(document.createTextNode(label), icon);
        button.title = `依${label}升冪排序`;
        header.replaceChildren(button); header.scope = 'col';
        button.addEventListener('click', () => {
            const descending = header.getAttribute('aria-sort') === 'ascending';
            headers.forEach(h => {
                h.removeAttribute('aria-sort');
                h.querySelector('.table-sort-icon').textContent = '↕';
                h.querySelector('button').title = `依${h.querySelector('button').firstChild.textContent}升冪排序`;
            });
            header.setAttribute('aria-sort', descending ? 'descending' : 'ascending');
            icon.textContent = descending ? '↓' : '↑';
            button.title = `依${label}${descending ? '升冪' : '降冪'}排序`;
            const value = row => row.cells[header.cellIndex].getAttribute('data-sort-value') ?? row.cells[header.cellIndex].textContent.trim();
            // Copy the original ordering for a deterministic tie-breaker in both directions.
            const sorted = rows.map((row, index) => ({ row, index })).sort((a, b) =>
                compareValues(value(a.row), value(b.row), header.dataset.sortType, descending) || a.index - b.index);
            body.append(...sorted.map(item => item.row));
        });
    });
}
if (typeof document !== 'undefined') document.querySelectorAll('table[data-sort-local]').forEach(initializeTable);
