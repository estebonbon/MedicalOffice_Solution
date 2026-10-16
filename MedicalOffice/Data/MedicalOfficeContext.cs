using MedicalOffice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace MedicalOffice.Data
{
    public class MedicalOfficeContext : DbContext
    {
        public MedicalOfficeContext(DbContextOptions<MedicalOfficeContext> options)
        : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<MedicalTrial> MedicalTrials { get; set; }

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
                .HasIndex(p => new { p.DOB, p.LastName, p.FirstName})
                .IsUnique();
        }
    }
}
