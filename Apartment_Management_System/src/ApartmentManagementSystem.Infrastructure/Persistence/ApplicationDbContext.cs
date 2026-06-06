using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ApartmentManagementSystem.Domain.Entities;
using ApartmentManagementSystem.Domain.Entities.Base;
using ApartmentManagementSystem.Domain.Entities.Auth;
using ApartmentManagementSystem.Domain.Entities.Communication;
using ApartmentManagementSystem.Domain.Entities.Customers;
using ApartmentManagementSystem.Domain.Entities.Financial;
using ApartmentManagementSystem.Domain.Entities.Marketing;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

	// Auth Module DbSets
	public DbSet<User> Users => Set<User>();
	public DbSet<Role> Roles => Set<Role>();
	public DbSet<UserRole> UserRoles => Set<UserRole>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
	public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();
	public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
	public DbSet<BackupCode> BackupCodes => Set<BackupCode>();
	public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();

	// Settings Module DbSets
	public DbSet<Branding> Brandings => Set<Branding>();

	// Customer Module DbSets
	public DbSet<Customer> Customers => Set<Customer>();
	public DbSet<Address> Addresses => Set<Address>();
	public DbSet<Lease> Leases => Set<Lease>();
	public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();
	public DbSet<CustomerNote> CustomerNotes => Set<CustomerNote>();
	public DbSet<LeaseDocument> LeaseDocuments => Set<LeaseDocument>();
	public DbSet<MoveChecklist> MoveChecklists => Set<MoveChecklist>();
	public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();

	// Product Module DbSets
	public DbSet<Product> Products => Set<Product>();
	public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
	public DbSet<ProductPhoto> ProductPhotos => Set<ProductPhoto>();
	public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
	public DbSet<Property> Properties => Set<Property>();
	public DbSet<PropertyOwner> PropertyOwners => Set<PropertyOwner>();
	public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();

	// Marketing
	public DbSet<Listing> Listings => Set<Listing>();
	public DbSet<ListingPhoto> ListingPhotos => Set<ListingPhoto>();

	// Financial
	public DbSet<Invoice> Invoices => Set<Invoice>();
	public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
	public DbSet<Payment> Payments => Set<Payment>();
	public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
	public DbSet<PaymentMatch> PaymentMatches => Set<PaymentMatch>();

	// Communication
	public DbSet<Conversation> Conversations => Set<Conversation>();
	public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
	public DbSet<Message> Messages => Set<Message>();
	public DbSet<Announcement> Announcements => Set<Announcement>();
	public DbSet<AnnouncementDelivery> AnnouncementDeliveries => Set<AnnouncementDelivery>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// Force every DateTime read/written through Npgsql to UTC Kind.
		// PostgreSQL 'timestamp with time zone' rejects DateTime with Kind=Unspecified.
		var utcDateTime = new ValueConverter<DateTime, DateTime>(
			v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
			v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
		var utcNullableDateTime = new ValueConverter<DateTime?, DateTime?>(
			v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
			v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

		foreach (var entityType in builder.Model.GetEntityTypes())
		{
			foreach (var property in entityType.GetProperties())
			{
				if (property.ClrType == typeof(DateTime))
					property.SetValueConverter(utcDateTime);
				else if (property.ClrType == typeof(DateTime?))
					property.SetValueConverter(utcNullableDateTime);
			}
		}
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;
		foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
		{
			if (entry.State == EntityState.Added)
				entry.Entity.CreatedAt = now;
			if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
				entry.Entity.UpdatedAt = now;
		}
		return await base.SaveChangesAsync(cancellationToken);
	}


}

