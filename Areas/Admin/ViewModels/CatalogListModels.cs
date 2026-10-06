using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class CatalogFilter
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public long? TermId { get; set; }
    public long? CourseId { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
}

public class CatalogListControls
{
    public CatalogFilter Filter { get; set; } = new();
    public string SearchLabel { get; set; } = "名稱搜尋";
    public List<SelectListItem> StatusOptions { get; set; } = [];
    public List<SelectListItem>? TermOptions { get; set; }
    public List<SelectListItem>? CourseOptions { get; set; }
    public List<SelectListItem> SortOptions { get; set; } = [];
    public int TotalCount { get; set; }
    public const int PageSize = 10;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public class CatalogListViewModel<T>
{
    public List<T> Items { get; set; } = [];
    public CatalogListControls Controls { get; set; } = new();
}
