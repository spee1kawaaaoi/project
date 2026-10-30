modelBuilder.Entity<RequestComment>(e =>
{
    e.ToTable("RequestComments");
    e.HasKey(c => c.Id);

    e.Property(c => c.Text).IsRequired().HasMaxLength(4000);
    e.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

    e.HasOne(c => c.Request)
     .WithMany(r => r.Comments)
     .HasForeignKey(c => c.RequestId)
     .OnDelete(DeleteBehavior.Cascade);

    e.HasOne(c => c.Author)
     .WithMany()
     .HasForeignKey(c => c.AuthorId)
     .OnDelete(DeleteBehavior.Restrict);

  
});
