using EnjoySpace.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace EnjoySpace.Web.TagHelpers;

[HtmlTargetElement("table", Attributes = "sort-set")]
public class SortTableTagHelper : TagHelper
{
    public string SortSet { get; set; } = "";
    public override void Init(TagHelperContext context) => context.Items[typeof(ITableSort)] = AdminTableSorts.Get(SortSet);
    public override void Process(TagHelperContext context, TagHelperOutput output) => output.Attributes.RemoveAll("sort-set");
}

[HtmlTargetElement("th", Attributes = "sort-key")]
public class SortHeaderTagHelper : TagHelper
{
    public string SortKey { get; set; } = "";
    [ViewContext, HtmlAttributeNotBound] public ViewContext ViewContext { get; set; } = null!;
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var definition = (ITableSort)context.Items[typeof(ITableSort)];
        var request = ViewContext.HttpContext.Request;
        var current = definition.Normalize(request.Query["Sort"]);
        var active = current == SortKey + "_asc" || current == SortKey + "_desc";
        var descending = current == SortKey + "_asc";
        var query = request.Query.Where(x => !x.Key.Equals("Sort", StringComparison.OrdinalIgnoreCase)
            && !x.Key.Equals("Page", StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
        query["Sort"] = SortKey + (descending ? "_desc" : "_asc");
        query["Page"] = "1";
        var link = new TagBuilder("a");
        link.AddCssClass("table-sort-link");
        link.Attributes["href"] = QueryHelpers.AddQueryString(request.PathBase + request.Path, query);
        link.Attributes["title"] = $"依{definition.Label(SortKey)}{(descending ? "降冪" : "升冪")}排序";
        link.InnerHtml.AppendHtml(await output.GetChildContentAsync());
        var icon = new TagBuilder("span");
        icon.AddCssClass("table-sort-icon"); icon.Attributes["aria-hidden"] = "true";
        icon.InnerHtml.Append(active ? (current.EndsWith("_asc") ? "↑" : "↓") : "↕");
        link.InnerHtml.AppendHtml(icon);
        output.Attributes.RemoveAll("sort-key");
        output.Attributes.SetAttribute("scope", "col");
        if (active) output.Attributes.SetAttribute("aria-sort", current.EndsWith("_asc") ? "ascending" : "descending");
        output.Content.SetHtmlContent(link);
    }
}
