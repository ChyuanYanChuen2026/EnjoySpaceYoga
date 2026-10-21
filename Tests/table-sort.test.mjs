import { test } from 'node:test';
import assert from 'node:assert/strict';
import { compareValues } from '../wwwroot/js/table-sort.mjs';

test('detail numbers sort numerically in both directions', () => {
    assert.deepEqual(['100', '2', '10'].sort((a, b) => compareValues(a, b, 'number', false)), ['2', '10', '100']);
    assert.deepEqual(['100', '2', '10'].sort((a, b) => compareValues(a, b, 'number', true)), ['100', '10', '2']);
});
test('missing dates remain last in both directions', () => {
    for (const descending of [false, true]) {
        const values = ['', '1900000000000', '1800000000000'];
        values.sort((a, b) => compareValues(a, b, 'number', descending));
        assert.equal(values.at(-1), '');
        assert.equal(values[0], descending ? '1900000000000' : '1800000000000');
    }
});
test('text identifiers use natural numeric order and equal values tie', () => {
    assert.ok(compareValues('課程2', '課程10', 'text', false) < 0);
    assert.equal(compareValues('老師甲', '老師甲', 'text', false), 0);
});
