using MedicalOffice.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using System.Diagnostics;

namespace MedicalOffice.Data
{
    public class MedicalOfficeContext : DbContext
    {
        public MedicalOfficeContext(DbContextOptions<MedicalOfficeContext> options)
        : base(options)
        {
        }

        //This is always where you add new Entitys aka Database Tables
        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<MedicalTrial> MedicalTrials { get; set; }

        public DbSet<PatientCondition> PatientConditions { get; set; }
        public DbSet<Condition> Conditions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Prevent Cascade Delete from Doctor to Patient
            //so we are prevented from deleting a Doctor with
            //Patients assigned
            modelBuilder.Entity<Doctor>()
                .HasMany<Patient>(d => d.Patients)
                .WithOne(p => p.Doctor)
                .HasForeignKey(p => p.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            //Add a unique index to the OHIP Number
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.OHIP)
                .IsUnique();

            //Unique Index composed of DOB, Last and First Name, this unique constraint makes it so the three fields combined must be different from each other
            modelBuilder.Entity<Patient>()
                .HasIndex(p => new { p.DOB, p.LastName, p.FirstName })
                .IsUnique();


            //Many to Many Intersection
            modelBuilder.Entity<PatientCondition>()
                .HasKey(t => new { t.ConditionID, t.PatientID }); //Compsite primary key


            // Refuse to delete a Condition while it still appears
            // in any Patient's medical history (PatientConditions).
            // Note: Allow Cascade Delete from Patient to PatientCondition.

            modelBuilder.Entity<PatientCondition>()
                .HasOne(pc => pc.Condition)
                .WithMany(c => c.PatientConditions)
                .HasForeignKey(pc => pc.ConditionID) /*foreign keys exist to protect references to parent records, not to prevent the child records themselves from being deleted.*/
                .OnDelete(DeleteBehavior.Restrict);


            /*-modelBuilder — The object EF Core gives us to configure our database model.
            - .Entity<PatientCondition>() — Selects the entity we want to configure.
            - .HasKey(...) — Tells EF Core which properties form the primary key.
            It's called Fluent API because you can chain methods together using dots (.).       SHIFT + CTRL + / */
            //CTRL KC or CTRL KU


            
        }
    }
}
