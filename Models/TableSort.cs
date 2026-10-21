using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Models;

public interface ITableSort
{
    string Normalize(string? sort);
    string Label(string key);
}

// Only explicitly registered expressions can reach SQL; request values are never property names or SQL.
public sealed class TableSort<T>(string defaultSort, Expression<Func<T, object?>> identity) : ITableSort
{
    private readonly Dictionary<string, (string Label, LambdaExpression Expression, Expression<Func<T, object?>>[] Secondary)> columns = new();
    public TableSort<T> Column<TKey>(string key, string label, Expression<Func<T, TKey>> expression, params Expression<Func<T, object?>>[] secondary)
    {
        columns.Add(key, (label, expression, secondary));
        return this;
    }
    public string Normalize(string? sort)
    {
        if (sort is null) return defaultSort;
        var split = sort.LastIndexOf('_');
        return split > 0 && columns.ContainsKey(sort[..split]) && sort[(split + 1)..] is "asc" or "desc" ? sort : defaultSort;
    }
    public string Label(string key) => columns[key].Label;
    public List<SelectListItem> Options() => columns.SelectMany(c => new[] {
        new SelectListItem(c.Value.Label + "：升冪", c.Key + "_asc"),
        new SelectListItem(c.Value.Label + "：降冪", c.Key + "_desc") }).ToList();

    public IOrderedQueryable<T> Apply(IQueryable<T> query, string? sort)
    {
        var normalized = Normalize(sort);
        var split = normalized.LastIndexOf('_');
        var column = columns[normalized[..split]];
        var descending = normalized.EndsWith("_desc");
        var ordered = Order(query, column.Expression, descending ? "OrderByDescending" : "OrderBy");
        foreach (var selector in column.Secondary)
            ordered = Order(ordered, Unbox(selector), descending ? "ThenByDescending" : "ThenBy");
        // Unique tie-breaker prevents equal values from moving between pages.
        return Order(ordered, Unbox(identity), "ThenBy");
    }
    private static LambdaExpression Unbox(LambdaExpression selector) => Expression.Lambda(
        selector.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert ? convert.Operand : selector.Body, selector.Parameters);
    private static IOrderedQueryable<T> Order(IQueryable<T> query, LambdaExpression selector, string method) =>
        (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), method,
            [typeof(T), selector.ReturnType], query.Expression, Expression.Quote(selector)));
}
