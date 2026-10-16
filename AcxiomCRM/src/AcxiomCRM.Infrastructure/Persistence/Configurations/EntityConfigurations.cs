using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AcxiomCRM.Domain.Entities;
using AcxiomCRM.Infrastructure.Identity;

namespace AcxiomCRM.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerCode).HasMaxLength(30).IsRequired();
        builder.HasIndex(c => c.CustomerCode).IsUnique();

        builder.Property(c => c.Email).HasMaxLength(150).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();

        builder.Property(c => c.Phone).HasMaxLength(20).IsRequired();
        builder.Property(c => c.CustomerName).HasMaxLength(150).IsRequired();
        builder.Property(c => c.CompanyName).HasMaxLength(150);
        builder.Property(c => c.Address).HasMaxLength(250);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.State).HasMaxLength(100);
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(c => c.OwnerId).HasMaxLength(100).IsRequired();

        builder.HasIndex(c => c.OwnerId);
        builder.HasIndex(c => c.Status);
    }
}

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.LeadId);
        builder.Property(l => l.LeadCode).HasMaxLength(30).IsRequired();
        builder.HasIndex(l => l.LeadCode).IsUnique();

        builder.Property(l => l.Email).HasMaxLength(150).IsRequired();
        builder.Property(l => l.Phone).HasMaxLength(20).IsRequired();
        builder.Property(l => l.LeadName).HasMaxLength(150).IsRequired();
        builder.Property(l => l.CompanyName).HasMaxLength(150);
        builder.Property(l => l.Source).HasMaxLength(100);
        builder.Property(l => l.ExpectedValue).HasColumnType("decimal(18,2)");
        builder.Property(l => l.AssignedTo).HasMaxLength(100).IsRequired();

        builder.HasIndex(l => l.AssignedTo);
        builder.HasIndex(l => l.Status);
    }
}

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.HasKey(o => o.OpportunityId);
        builder.Property(o => o.OpportunityName).HasMaxLength(150).IsRequired();
        builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.AssignedTo).HasMaxLength(100).IsRequired();

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Lead)
            .WithMany(l => l.Opportunities)
            .HasForeignKey(o => o.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.AssignedTo);
        builder.HasIndex(o => o.Stage);
        builder.HasIndex(o => o.CustomerId);
    }
}

public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.HasKey(f => f.FollowUpId);
        builder.Property(f => f.Subject).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Remarks).HasMaxLength(1000);
        builder.Property(f => f.AssignedTo).HasMaxLength(100).IsRequired();

        builder.HasOne(f => f.Customer)
            .WithMany(c => c.FollowUps)
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Lead)
            .WithMany(l => l.FollowUps)
            .HasForeignKey(f => f.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.AssignedTo);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.FollowUpDate);
    }
}

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.ActivityId);
        builder.Property(a => a.Subject).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.AssignedTo).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Lead)
            .WithMany(l => l.Activities)
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.AssignedTo);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.AuditLogId);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.RecordId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.IpAddress).HasMaxLength(50);

        builder.HasIndex(a => a.CreatedDate);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.EntityName);
    }
}
