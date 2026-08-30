using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Pages;

public class IndexModel : PageModel
{
    private readonly UserManager<AttendanceUser> _userManager;

    public IndexModel(UserManager<AttendanceUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (user.Role == UserRole.Student)
                {
                    return RedirectToPage("/Student/RecordAttendance");
                }
                else if (user.Role == UserRole.Lecturer)
                {
                    return RedirectToPage("/Lecturer/ViewAttendance");
                }
            }
        }

        return Page();
    }
}
