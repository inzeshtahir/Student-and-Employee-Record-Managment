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
    public class AcademicRecordsController : Controller
    {
        private readonly StudentrecordContext _context;

        public AcademicRecordsController(StudentrecordContext context)
        {
            _context = context;
        }

        // GET: AcademicRecords
        public async Task<IActionResult> Index(string sortColumn, string sortDirection)
        {
            var academicRecords = await _context.Academicrecords
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .ToListAsync();

            // Default sort direction is ascending
            bool ascending = sortDirection != "desc";

            // Apply sorting with custom comparer
            academicRecords.Sort(new AcademicRecordComparer(sortColumn, ascending));

            // Pass current sorting state to the view
            ViewBag.CurrentSortColumn = sortColumn;
            ViewBag.CurrentSortDirection = sortDirection;

            return View(academicRecords);
        }

        public class AcademicRecordComparer : IComparer<LabAssignment6.DataAccess.Academicrecord>
        {
            private readonly string _sortColumn;
            private readonly bool _ascending;

            public AcademicRecordComparer(string sortColumn, bool ascending)
            {
                _sortColumn = sortColumn;
                _ascending = ascending;
            }

            public int Compare(LabAssignment6.DataAccess.Academicrecord r1, LabAssignment6.DataAccess.Academicrecord r2)
            {
                // Null grades come first
                if (r1.Grade == null && r2.Grade != null) return -1;
                if (r1.Grade != null && r2.Grade == null) return 1;

                int comparison = _sortColumn switch
                {
                    "Course" => string.Compare(r1.CourseCodeNavigation.Code, r2.CourseCodeNavigation.Code, StringComparison.OrdinalIgnoreCase),
                    "Student" => string.Compare(r1.Student.Id, r2.Student.Id, StringComparison.OrdinalIgnoreCase),
                    "Grade" => Nullable.Compare(r1.Grade, r2.Grade),
                    _ => 0
                };

                return _ascending ? comparison : -comparison;
            }
        }

        // GET: AcademicRecords/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicrecord = await _context.Academicrecords
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (academicrecord == null)
            {
                return NotFound();
            }

            return View(academicrecord);
        }

        // GET: AcademicRecords/Create
        public IActionResult Create()
        {
            ViewData["CourseCode"] = new SelectList(_context.Courses, "Code", "Code");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id");
            return View();
        }

        // POST: AcademicRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,StudentId,Grade")] Academicrecord academicrecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(academicrecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "Code", "Code", academicrecord.CourseCode);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", academicrecord.StudentId);
            return View(academicrecord);
        }

        // GET: AcademicRecords/Edit/5
        public async Task<IActionResult> Edit(string studentId, string courseCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(courseCode))
            {
                return NotFound();
            }

            var academicRecord = await _context.Academicrecords
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CourseCode == courseCode);

            if (academicRecord == null)
            {
                return NotFound();
            }

            return View(academicRecord);
        }

        // POST: AcademicRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string studentId, string courseCode, [Bind("CourseCode,StudentId,Grade")] Academicrecord academicRecord)
        {
            if (studentId != academicRecord.StudentId || courseCode != academicRecord.CourseCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(academicRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Academicrecords.Any(a => a.StudentId == studentId && a.CourseCode == courseCode))
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

            return View(academicRecord);
        }

        // GET: AcademicRecords/EditAll
        public async Task<IActionResult> EditAll(string sortColumn, string sortDirection)
        {
            var academicRecords = await _context.Academicrecords
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .ToListAsync();

            // Default sort direction is ascending
            bool ascending = sortDirection != "desc";

            // Apply sorting with custom comparer
            academicRecords.Sort(new AcademicRecordComparer(sortColumn, ascending));

            // Pass current sorting state to the view
            ViewBag.CurrentSortColumn = sortColumn;
            ViewBag.CurrentSortDirection = sortDirection;

            return View(academicRecords);
        }

        // POST: AcademicRecords/EditAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAll(List<Academicrecord> academicRecords)
        {
            // Retrieve existing records without tracking to avoid duplicate tracking issues
            var oldAcademicRecords = await _context.Academicrecords
                .AsNoTracking() // Prevents tracking of these records
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .ToListAsync();

            bool hasError = false;

            // Validate and update each record
            for (int i = 0; i < academicRecords.Count; i++)
            {
                var record = academicRecords[i];

                // Validate Grade
                if (record.Grade.HasValue)
                {
                    if (record.Grade < 0 || record.Grade > 100)
                    {
                        hasError = true;
                    }
                }

                // Retrieve the old record for navigation properties
                var oldRecord = oldAcademicRecords.FirstOrDefault(r =>
                    r.StudentId == record.StudentId && r.CourseCode == record.CourseCode);

                if (oldRecord != null)
                {
                    // Map the navigation properties from the non-tracked entity
                    record.CourseCodeNavigation = oldRecord.CourseCodeNavigation;
                    record.Student = oldRecord.Student;

                    // Attach the updated record to the DbContext
                    _context.Entry(record).State = EntityState.Modified;
                }
            }

            // If there are validation errors, return the view with the errors
            if (hasError || !ModelState.IsValid)
            {
                return View(academicRecords);
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: AcademicRecords/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicrecord = await _context.Academicrecords
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (academicrecord == null)
            {
                return NotFound();
            }

            return View(academicrecord);
        }

        // POST: AcademicRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var academicrecord = await _context.Academicrecords.FindAsync(id);
            if (academicrecord != null)
            {
                _context.Academicrecords.Remove(academicrecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AcademicrecordExists(string id)
        {
            return _context.Academicrecords.Any(e => e.StudentId == id);
        }
    }
}
