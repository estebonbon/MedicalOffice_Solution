
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
                            .Include(p => p.PatientConditions)
                                .ThenInclude(p => p.Condition)
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
            .Include(p => p.PatientConditions).ThenInclude(p => p.Condition)
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
        Patient patient = new Patient(); /*new Patient() does NOT create a database record. It only creates an object in memory.*/
        PopulateAssignedConditionData(patient);
        PopulateDropDownLists();
        return View();
    }

    // POST: PATIENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken] //Helps to protect site against forgery attacks
    //The bind property acts as an allow list, other properties cannot be modified by the user
    public async Task<IActionResult> Create([Bind("ID,OHIP,FirstName,MiddleName,LastName,DOB,ExpYrVisits,Phone,EMail,Coverage,MedicalTrialId, DoctorID")] Patient patient, string[] selectedOptions)
    {
        try
        {
            //Add the selected conditions
            if (selectedOptions != null)
            {
                foreach (var condition in selectedOptions)
                {
                    var conditionToAdd = new PatientCondition
                    {
                        PatientID = patient.ID,
                        ConditionID = int.Parse(condition)
                    };

                    patient.PatientConditions.Add(conditionToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                //existing Create logic continues here
            }

            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        catch (RetryLimitExceededException /* dex */)
        {
            ModelState.AddModelError("",
                "Unable to save changes after multiple attempts. " +
                "Try again, and if the problem persists, see your system administrator.");
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

        PopulateAssignedConditionData(patient);
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

        var patient = await _context.Patients
            .Include(p => p.MedicalTrial)
            .Include(p => p.PatientConditions)
                .ThenInclude(p => p.Condition)
            .FirstOrDefaultAsync(p => p.ID == id);

        if (patient == null)
        {
            return NotFound();
        }

        PopulateAssignedConditionData(patient);
        PopulateDropDownLists(patient);
        return View(patient);

    }

    // POST: PATIENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string[] selectedOptions)
    {

        //Go get the patient to update
        var patientToUpdate = await _context.Patients
            .Include(p => p.PatientConditions)
            .ThenInclude(p => p.Condition)
            .FirstOrDefaultAsync(p => p.ID == id);


        //Check that you got it or exit with a not found error
        if (patientToUpdate == null)
        {
            return NotFound();
        }

        //Update the medical history
        UpdatePatientConditions(selectedOptions, patientToUpdate);

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
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("",
                    "Unable to save changes after multiple attempts. " +
                    "Try again, and if the problem persists, see your system administrator.");
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

        PopulateAssignedConditionData(patientToUpdate);
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
            .Include(p => p.PatientConditions)
                                .ThenInclude(p => p.Condition)
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
            .Include(p => p.PatientConditions)
                                .ThenInclude(p => p.Condition)
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


    /// <summary>
    /// Prepares a collection of checkbox ViewModel objects for each condition.
    /// Sets Assigned = true for conditions already in the patient's medical history.
    /// </summary>
    /// <param name="patient">The Patient entity with included PatientConditions</param>
    private void PopulateAssignedConditionData(Patient patient)
    {
        //Ensure PatientConditions are loaded before calling this method
        var allConditions = _context.Conditions.ToList();

        var assignedConditionIds = new HashSet<int>(
            patient.PatientConditions.Select(pc => pc.ConditionID));

        var conditionOptions = allConditions.Select(condition => new CheckOptionVM
        {
            ID = condition.ID,
            DisplayText = condition.ConditionName,
            Assigned = assignedConditionIds.Contains(condition.ID)
        }).ToList();

        ViewData["ConditionOptions"] = conditionOptions;
    }


    /// <summary>
    /// Updates the PatientConditions for a Patient to match the selected checkboxes.
    /// </summary>
    /// <param name="selectedOptions">IDs of the selected conditions</param>
    /// <param name="patientToUpdate">The Patient entity to update</param>
    private void UpdatePatientConditions(string[]? selectedOptions, Patient patientToUpdate)
    {
        // If no options are selected, clear all conditions
        if (selectedOptions == null || selectedOptions.Length == 0)
        {
            patientToUpdate.PatientConditions = new List<PatientCondition>();
            return;
        }

        var selectedIds = new HashSet<string>(selectedOptions);

        var currentConditionIds = new HashSet<int>(
            patientToUpdate.PatientConditions.Select(pc => pc.ConditionID));

        foreach (var condition in _context.Conditions)
        {
            string conditionIdStr = condition.ID.ToString();

            bool isSelected = selectedIds.Contains(conditionIdStr);
            bool isCurrentlyAssigned = currentConditionIds.Contains(condition.ID);

            if (isSelected && !isCurrentlyAssigned)
            {
                // Add new condition
                patientToUpdate.PatientConditions.Add(new PatientCondition
                {
                    PatientID = patientToUpdate.ID,
                    ConditionID = condition.ID
                });
            }
            else if (!isSelected && isCurrentlyAssigned)
            {
                // Remove unselected condition
                var conditionToRemove = patientToUpdate.PatientConditions
                    .SingleOrDefault(pc => pc.ConditionID == condition.ID);

                if (conditionToRemove != null)
                {
                    _context.Remove(conditionToRemove);
                }
            }
        }
    }


    private bool PatientExists(int? id)
    {
        return _context.Patients.Any(e => e.ID == id);
    }
}
