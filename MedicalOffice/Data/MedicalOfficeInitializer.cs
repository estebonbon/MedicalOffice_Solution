using MedicalOffice.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MedicalOffice.Data
{
    public static class MedicalOfficeInitializer
    {
        /// <summary> 
        /// Prepares the Database and seeds data as required 
        /// </summary> 
        /// <param name="serviceProvider">DI Container</param> 
        /// <param name="DeleteDatabase">Delete the database and start from scratch</param> 
        /// <param name="UseMigrations">Use Migrations or EnsureCreated</param> 
        /// <param name="SeedSampleData">Add optional sample data</param> 
        public static void Initialize(IServiceProvider serviceProvider,
            bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
        {
            using (var context = new MedicalOfficeContext(serviceProvider.GetRequiredService<DbContextOptions<MedicalOfficeContext>>()))
            {

                #region Prepare the Database
                try
                {
                    //Note: .CanConnect() will return false if the database is not there!
                    if (DeleteDatabase || !context.Database.CanConnect())
                    {
                        context.Database.EnsureDeleted(); //Delete the existing version
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Create the database and apply all migrations
                        }
                        else
                        {
                            context.Database.EnsureCreated(); //Create and update the database as per the Model
                        }
                        //Here is a good place to create any additional database objects such as Triggers or Views 
                        //----------------------------------------------------------------------------------------
                    }
                    else //The database is already created
                    {
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Apply all migrations
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                #region Seed Required Data
                try
                {
                    // Add some Medical Trials
                    if (!context.MedicalTrials.Any())
                    {
                        context.MedicalTrials.AddRange(
                            new MedicalTrial
                            {
                                TrialName = "UOT - Leukemia Treatment"
                            },
                            new MedicalTrial
                            {
                                TrialName = "HyGIeaCare Center - Microbiome Analysis of Constipated Versus Non-Constipated Patients"
                            },
                            new MedicalTrial
                            {
                                TrialName = "Sunnybrook - Trial of BNT162b2 versus mRNA-1273 COVID-19 Vaccine Boosters in Chronic Kidney Disease and Dialysis Patients With Poor Humoral Response following COVID-19 Vaccination"
                            },
                            new MedicalTrial
                            {
                                TrialName = "Altimmune - Evaluate the Effect of Position and Duration on the Safety and Immunogenicity of Intranasal AdCOVID Administration"
                            },
                            new MedicalTrial
                            {
                                TrialName = "TUK - Hair Loss Treatment"
                            });
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                //Seed meanigless data as sample data during development
                #region Seed Sample Data

                if (SeedSampleData)
                {
                    //To randomly generate data
                    Random random = new Random();
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

                    try
                        {
                            //Add more Doctors
                            if (context.Doctors.Count() < 4)//Don't add a second time
                            {
                                string[] firstNames = new string[] { "Woodstock", "Violet", "Charlie", "Lucy", "Linus", "Franklin", "Marcie", "Schroeder" };
                                string[] lastNames = new string[] { "Hightower", "Broomspun", "Jones" };

                                //Loop through names and add more
                                foreach (string lastName in lastNames)
                                {
                                    foreach (string firstname in firstNames)
                                    {
                                        //Construct some details
                                        Doctor a = new Doctor()
                                        {
                                            FirstName = firstname,
                                            LastName = lastName,
                                            //Take second character of the last name and make it the middle name
                                            MiddleName = lastName[1].ToString().ToUpper(),
                                        };
                                        context.Doctors.Add(a);
                                    }
                                }
                                context.SaveChanges();
                            }



                        }
                        catch (Exception ex)
                        {

                            Debug.WriteLine(ex.GetBaseException().Message);
                        }
                    }
                #endregion
            }
        }
    }
}
