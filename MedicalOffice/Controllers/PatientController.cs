
using MedicalOffice.Data;
using MedicalOffice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class PatientController : Controller
{
    private readonly MedicalOfficeContext _context;

    public PatientController(MedicalOfficeContext context)
    {
        _context = context;
    }

    // GET: PATIENTS
    public async Task<IActionResult> Index()    
    {
        // Alternative to .Include(): explicitly join Patients and Doctors and populate the related Doctor object.
        //var patients = from p in _context.Patients
        //               join d in _context.Doctors on p.DoctorID equals d.ID
        //               select new Patient
        //               {
        //                   ID = p.ID,
        //                   OHIP = p.OHIP,
        //                   FirstName = p.FirstName,
        //                   MiddleName = p.MiddleName,
        //                   LastName = p.LastName,
        //                   DOB = p.DOB,
        //                   ExpYrVisits = p.ExpYrVisits,
        //                   Phone = p.Phone,
        //                   EMail = p.EMail,
        //                   DoctorID = p.DoctorID,
        //                   Doctor = d
        //               };

        var patients = _context.Patients
                            .Include(p => p.Doctor)
                            .Include(p => p.MedicalTrial)
                            .AsNoTracking();

        return View(await patients.ToListAsync());
    }

    // GET: PATIENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .Include(p => p.MedicalTrial)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    // GET: PATIENTS/Create
    public IActionResult Create()
    {
        PopulateDropDownLists();
        return View();
    }

    // POST: PATIENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken] //Helps to protect site against forgery attacks
    //The bind property acts as an allow list, other properties cannot be modified by the user
    public async Task<IActionResult> Create([Bind("ID,OHIP,FirstName,MiddleName,LastName,DOB,ExpYrVisits,Phone,EMail,Coverage,MedicalTrialId, DoctorID")] Patient patient)
    {
        try
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateException dex)
        {
            string message = dex.GetBaseException().Message;

            if (message.Contains("UNIQUE") && message.Contains("Patients.OHIP"))
            {
                ModelState.AddModelError("OHIP", "Unable to save hanges. Remember, " + "you cannot have duplicate OHIP numbers");
            }
            else if (message.Contains("UNIQUE") && message.Contains("Patients.DOB"))
            {
                ModelState.AddModelError("", "Unable to save changes. Remember, " + "you cannot have duplicate combinations of First Name, Last Name, and Date of Birth.");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administartor.");
            }
        }

            //if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Patients.OHIP"))
            //{
            //    ModelState.AddModelError("OHIP", "Unable to save changes. Remember, you cannot have duplicate OHIP numbers.");
            //}   
            // The dex.GetBaseExecption is being given over to the value stored in message


        PopulateDropDownLists(patient);
        return View(patient);
    }

    // GET: PATIENTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound();
        }

        PopulateDropDownLists(patient);
        return View(patient);
    }

    // POST: PATIENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        //Go get the patient to update
        var patientToUpdate = await _context.Patients.FirstOrDefaultAsync(p => p.ID == id);

        //Check that you got it or exit with a not found error
        if (patientToUpdate == null)
        {
            return NotFound();
        }

        //Try updating it with the values posted
        if (await TryUpdateModelAsync<Patient>(patientToUpdate, "", 
            p => p.OHIP, p => p.FirstName, p => p.MiddleName, p => p.LastName, p => p.DOB,
            p => p.ExpYrVisits, p => p.Phone, p => p.EMail, p => p.Coverage, p => p.MedicalTrialID, p => p.DoctorID))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Only if everything goes through
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(patientToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;

                if (message.Contains("UNIQUE") && message.Contains("Patients.OHIP"))
                {
                    ModelState.AddModelError("OHIP", "Unable to save hanges. Remember, " + "you cannot have duplicate OHIP numbers");
                }
                else if (message.Contains("UNIQUE") && message.Contains("Patients.DOB"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, " + "you cannot have duplicate combinations of First Name, Last Name, and Date of Birth.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administartor.");
                }
            }
        }
        PopulateDropDownLists(patientToUpdate);
        return View(patientToUpdate); // Keep the user on the form, with the values they need to fix
    }

    // GET: PATIENTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .Include(p => p.MedicalTrial)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    // POST: PATIENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .Include(p => p.MedicalTrial)
            .FirstOrDefaultAsync(m => m.ID == id);

        try
        {
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            //Note: there is really no reason a delete should fail if you can "talk" to the database.
            ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
        }
        return View(patient);
    }

    // Populates the Doctor dropdown and selects the patient's current Doctor when editing.
    private void PopulateDropDownLists(Patient? patient = null)
    {
        ViewData["DoctorID"] = DoctorSelectList(patient?.DoctorID);
        ViewData["MedicalTrialID"] = MedicalTrialList(patient?.MedicalTrialID);
    }

    //This is a twist on the PopulateDropDownLists approach
    // Create methods that return each SelectList separately
    // and one method to put them all into ViewData.
    //This approach allows for AJAX requests to refresh
    //DDL Data at a later date.
    private SelectList DoctorSelectList(int? selectedId)
    {
        return new SelectList(_context.Doctors
        .OrderBy(d => d.LastName)
        .ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
    }
    public SelectList MedicalTrialList(int? selectedId)
    {
        return new SelectList(_context.MedicalTrials
            .OrderBy(m => m.TrialName), "ID", "TrialName", selectedId);
    }
    private bool PatientExists(int? id)
    {
        return _context.Patients.Any(e => e.ID == id);
    }
}
