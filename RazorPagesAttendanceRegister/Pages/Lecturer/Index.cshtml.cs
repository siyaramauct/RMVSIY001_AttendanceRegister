using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class IndexModel : PageModel
    {
    }
}
