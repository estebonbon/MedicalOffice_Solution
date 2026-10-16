
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Models;
using MedicalOffice.Data;

public class DoctorController : Controller
{
    private readonly MedicalOfficeContext _context;

    public DoctorController(MedicalOfficeContext context)
    {
        _context = context;
    }

    // GET: DOCTORS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Doctors
                        .AsNoTracking()
                        .ToListAsync());
    }

    // GET: DOCTORS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // GET: DOCTORS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DOCTORS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Patients")] Doctor doctor)
    {
        try
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
        }

        return View(doctor);
    }

    // GET: DOCTORS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // POST: DOCTORS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        //Go get the Doctor to update
        var doctorToUpdate = await _context.Doctors.FirstOrDefaultAsync(d => d.ID == id);

        //Check that you got it or exit with a not found error
        if (doctorToUpdate == null)
        {
            return NotFound();
        }

        //Try updating it with the values posted
        if (await TryUpdateModelAsync<Doctor>(doctorToUpdate, "",
                d => d.FirstName, d => d.MiddleName, d => d.LastName))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorExists(doctorToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
        }
        return View(doctorToUpdate);
    }

    // GET: DOCTORS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // POST: DOCTORS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(m => m.ID == id);

        try
        {
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException dex)
        {
            if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
            {
                ModelState.AddModelError("", "Unable to Delete Doctor. Remember, you cannot delete a " +
                    "Doctor that has patients assigned.");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem" +
                    " persists see your system administrator.");
            }
        }
        return View(doctor);

    }

    private bool DoctorExists(int? id)
    {
        return _context.Doctors.Any(e => e.ID == id);
    }
}
