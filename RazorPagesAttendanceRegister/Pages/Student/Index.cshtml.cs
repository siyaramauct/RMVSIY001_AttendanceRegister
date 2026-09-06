using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesAttendanceRegister.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
    }
}
