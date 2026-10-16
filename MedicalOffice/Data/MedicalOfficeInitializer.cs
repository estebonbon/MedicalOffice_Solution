using MedicalOffice.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MedicalOffice.Data
{
    public static class MedicalOfficeInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MedicalOfficeContext(serviceProvider.GetRequiredService<DbContextOptions<MedicalOfficeContext>>()))
            {
                try
                {
                    // Seed data
                    // Look for any Doctors.  Since we can't have patients without Doctors.
                    if (!context.Doctors.Any())
                    {
                        context.Doctors.AddRange(
                            new Doctor
                            {
                                FirstName = "Gregory",
                                MiddleName = "A",
                                LastName = "House"
                            },
                            new Doctor
                            {
                                FirstName = "Doogie",
                                MiddleName = "R",
                                LastName = "Houser"
                            },
                            new Doctor
                            {
                                FirstName = "Charles",
                                LastName = "Xavier"
                            });
                        context.SaveChanges();
                    }

                    //Add some Medical Trials
                    if (!context.MedicalTrials.Any())
                    {
                        context.MedicalTrials.AddRange(
                        new MedicalTrial
                        {
                            TrialName = "UOT - Leukemia Treatment"
                        }, new MedicalTrial
                        {
                            TrialName = "HyGIeaCare Center - Microbiome Analysis of Constipated Versus Non-Constipated Patients"
                        }, new MedicalTrial
                        {
                            TrialName = "TUK - Hair Loss Treatment"
                        });
                        context.SaveChanges(); // Ensures that the Medical Trial table is created b4 patients satart to access the table
                    }

                    // Seed Patients if there aren't any.
                    if (!context.Patients.Any())
                    {
                        context.Patients.AddRange(
                        new Patient
                        {
                            FirstName = "Fred",
                            MiddleName = "Reginald",
                            LastName = "Flintstone",
                            OHIP = "1231231234",
                            DOB = DateTime.Parse("1955-09-01"),
                            ExpYrVisits = 6,
                            Phone = "9055551212",
                            EMail = "fflintstone@outlook.com",
                            DoctorID = context.Doctors.FirstOrDefault(static d => d.FirstName == "Gregory" && d.LastName == "House").ID,
                            MedicalTrialID = context.MedicalTrials.FirstOrDefault(d => d.TrialName.Contains("UOT")).ID
                        },
                        new Patient
                        {
                            FirstName = "Wilma",
                            MiddleName = "Jane",
                            LastName = "Flintstone",
                            OHIP = "1321321324",
                            DOB = DateTime.Parse("1964-04-23"),
                            ExpYrVisits = 2,
                            Phone = "9055551212",
                            EMail = "wflintstone@outlook.com",
                            DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Gregory" && d.LastName == "House").ID,
                            MedicalTrialID = context.MedicalTrials.FirstOrDefault(d => d.TrialName.Contains("HyGIeaCare")).ID
                        },
                        new Patient
                        {
                            FirstName = "Barney",
                            LastName = "Rubble",
                            OHIP = "3213213214",
                            DOB = DateTime.Parse("1964-02-22"),
                            ExpYrVisits = 2,
                            Phone = "9055551213",
                            EMail = "brubble@outlook.com",
                            Coverage = Coverage.OHIP,
                            DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Doogie" && d.LastName == "Houser").ID
                        },
                        new Patient
                        {
                            FirstName = "Jane",
                            MiddleName = "Samantha",
                            LastName = "Doe",
                            OHIP = "4124124123",
                            ExpYrVisits = 2,
                            Phone = "9055551234",
                            EMail = "jdoe@outlook.com",
                            Coverage = Coverage.OutOfProvince,
                            DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Charles" && d.LastName == "Xavier").ID
                        });
                        context.SaveChanges();
                    }


                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex.GetBaseException().Message);
                }
            }
        }
    }
}
