using EnjoySpace.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Areas.Admin.ViewModels
{
    public class StudentListItemViewModel
    {
        public long StudentId { get; set; }
        public string? StudentNo { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateOnly? JoinedDate { get; set; }
        public bool IsActive { get; set; } = true; 
        public string? Note { get; set; }
        public bool IsUserActive { get; set; } = true; 

    }
}
