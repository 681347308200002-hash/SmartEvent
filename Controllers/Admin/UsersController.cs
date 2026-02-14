using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Models;

namespace SmartEvent.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Users
        public async Task<IActionResult> Index(string? q)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                usersQuery = usersQuery.Where(u =>
                    u.Email!.Contains(q) || u.UserName!.Contains(q));
            }

            var users = await usersQuery
                .OrderBy(u => u.Email)
                .ToListAsync();

            // Build a simple view model list with roles
            var result = new List<UserRowVM>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserRowVM
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    UserName = u.UserName ?? "",
                    LockoutEnd = u.LockoutEnd,
                    Roles = roles.ToList()
                });
            }

            ViewBag.Query = q;
            return View(result);
        }

        // GET: /Users/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);

            var vm = new EditUserVM
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                SelectedRoles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(vm);
        }

        // POST: /Users/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                // reload roles for redisplay
                vm.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            // Update basic fields (only what you allow)
            user.UserName = vm.UserName;
            user.Email = vm.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors)
                    ModelState.AddModelError("", e.Description);

                vm.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(vm);
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var selected = vm.SelectedRoles ?? new List<string>();

            var toAdd = selected.Except(currentRoles).ToList();
            var toRemove = currentRoles.Except(selected).ToList();

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            if (toAdd.Any())
                await _userManager.AddToRolesAsync(user, toAdd);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Lock/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, int days = 3650)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // lock account
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(days));

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Unlock/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            return RedirectToAction(nameof(Index));
        }
    }

    public class UserRowVM
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public DateTimeOffset? LockoutEnd { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class EditUserVM
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";

        // checkbox selected roles
        public List<string> SelectedRoles { get; set; } = new();

        // list of all roles for the view
        public List<string> AllRoles { get; set; } = new();
    }
}
