// Treat these values as Taiwan wall-clock fields, not UTC timestamps.
export function parseDateInput(value, withTime) {
    const pattern = withTime
        ? /^(\d{4})[-/](\d{2})[-/](\d{2})[ T](\d{2}):(\d{2})(?::(\d{2})(?:\.\d+)?)?$/
        : /^(\d{4})[-/](\d{2})[-/](\d{2})$/;
    const match = pattern.exec(value.trim());
    if (!match) return null;
    const [year, month, day, hour, minute, second] = match.slice(1).map(value => Number(value ?? 0));
    const hours = withTime ? hour : 0, minutes = withTime ? minute : 0, seconds = withTime ? second : 0;
    if (year < 1 || month < 1 || month > 12 || day < 1 || hours > 23 || minutes > 59 || seconds > 59) return null;
    const date = new Date(2000, month - 1, day, hours, minutes, seconds, 0);
    date.setFullYear(year);
    // Never roll February 30 or 24:00 into a different valid date.
    if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day || date.getHours() !== hours) return null;
    return date;
}

export function formatDateInput(date, withTime) {
    const pad = value => String(value).padStart(2, '0');
    const day = `${String(date.getFullYear()).padStart(4, '0')}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
    return withTime ? `${day} ${pad(date.getHours())}:${pad(date.getMinutes())}` : day;
}
