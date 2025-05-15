using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabAssignment6.DataAccess;

namespace LabAssignment6.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly StudentrecordContext _context;

        public EmployeesController(StudentrecordContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            // Include Roles when retrieving Employees
            var employees = await _context.Employees
                .Include(e => e.Roles)
                .ToListAsync();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Include Roles in the Details action
            var employee = await _context.Employees
                .Include(e => e.Roles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            // Retrieve all roles from the database
            var roles = _context.Roles.ToList();

            // Pass roles to the view using ViewData
            ViewData["Roles"] = roles;

            return View(new Employee());
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, int[] SelectedRoles)
        {
            if (ModelState.IsValid)
            {
                // Assign selected roles to the employee
                employee.Roles = _context.Roles
                                        .Where(r => SelectedRoles.Contains(r.Id))
                                        .ToList();

                // Save the employee to the database
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reload roles for redisplaying the form if submission fails
            ViewData["Roles"] = _context.Roles.ToList();
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                                         .Include(e => e.Roles) // Include the employee's roles
                                         .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            // Retrieve all roles and pass them to the view
            ViewData["Roles"] = _context.Roles.ToList();
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee, int[] SelectedRoles)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (!SelectedRoles.Any())
            {
                ModelState.AddModelError("Roles", "You must select at least one role!");
            }

            if (_context.Employees.Any(e => e.UserName == employee.UserName && e.Id != id))
            {
                ModelState.AddModelError("UserName", "This username has been used by another employee!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing employee with roles from the database
                    var existingEmployee = await _context.Employees
                                                         .Include(e => e.Roles)
                                                         .FirstOrDefaultAsync(e => e.Id == id);

                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    // Update basic employee details
                    existingEmployee.Name = employee.Name;
                    existingEmployee.UserName = employee.UserName;
                    existingEmployee.Password = employee.Password;

                    // Update roles
                    existingEmployee.Roles.Clear(); // Remove all existing roles
                    existingEmployee.Roles = _context.Roles
                                                     .Where(r => SelectedRoles.Contains(r.Id))
                                                     .ToList();

                    // Save changes
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Reload roles for redisplaying the form if submission fails
            ViewData["Roles"] = _context.Roles.ToList();
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
