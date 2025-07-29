using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Student_Management.ViewModels;
using Student_Management.Models;

namespace Student_Management.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
   
 
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }



    public DbSet<Subject> Subjects { get; set; }
    public DbSet<UserSubject> UserSubjects { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define composite key for UserSubject
        modelBuilder.Entity<UserSubject>()
            .HasKey(us => new { us.UserId, us.SubjectId });

        // Define relationships for UserSubject
        modelBuilder.Entity<UserSubject>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserSubject)
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserSubject>()
            .HasOne(us => us.Subject)
            .WithMany(s => s.UserSubjects)
            .HasForeignKey(us => us.SubjectId);
    
    }
}