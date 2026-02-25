using System;
using System.Collections.Generic;
using  CapstoneProject.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Database.AppDbContext;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Council> Councils { get; set; }

    public virtual DbSet<CouncilMember> CouncilMembers { get; set; }

    public virtual DbSet<DefenseSchedule> DefenseSchedules { get; set; }

    public virtual DbSet<DefenseScore> DefenseScores { get; set; }

    public virtual DbSet<FinalGrade> FinalGrades { get; set; }

    public virtual DbSet<GradeComplaint> GradeComplaints { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupInvitation> GroupInvitations { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<MentorAssignment> MentorAssignments { get; set; }

    public virtual DbSet<MentorRequest> MentorRequests { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<TopicVersion> TopicVersions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WeekDefinition> WeekDefinitions { get; set; }

    public virtual DbSet<WeeklyEvaluation> WeeklyEvaluations { get; set; }

    public virtual DbSet<WeeklyReport> WeeklyReports { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=LAPTOP-MPFCF155;Database=CapstoneManagement;User Id=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Council>(entity =>
        {
            entity.HasKey(e => e.CouncilId).HasName("PK__Councils__1BBAA5C1277B516D");
        });

        modelBuilder.Entity<CouncilMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CouncilM__3214EC07CB79116F");

            entity.HasOne(d => d.Council).WithMany(p => p.CouncilMembers).HasConstraintName("FK__CouncilMe__Counc__1CBC4616");

            entity.HasOne(d => d.User).WithMany(p => p.CouncilMembers).HasConstraintName("FK__CouncilMe__UserI__1DB06A4F");
        });

        modelBuilder.Entity<DefenseSchedule>(entity =>
        {
            entity.HasKey(e => e.DefenseId).HasName("PK__DefenseS__63CF1C44D5B9F05F");

            entity.Property(e => e.Status).HasDefaultValue("Scheduled");

            entity.HasOne(d => d.Council).WithMany(p => p.DefenseSchedules).HasConstraintName("FK__DefenseSc__Counc__22751F6C");

            entity.HasOne(d => d.Group).WithMany(p => p.DefenseSchedules).HasConstraintName("FK__DefenseSc__Group__2180FB33");
        });

        modelBuilder.Entity<DefenseScore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DefenseS__3214EC074DC0D66C");

            entity.Property(e => e.IsPublished).HasDefaultValue(false);

            entity.HasOne(d => d.CouncilMember).WithMany(p => p.DefenseScores).HasConstraintName("FK__DefenseSc__Counc__2739D489");

            entity.HasOne(d => d.Defense).WithMany(p => p.DefenseScores).HasConstraintName("FK__DefenseSc__Defen__2645B050");
        });

        modelBuilder.Entity<FinalGrade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FinalGra__3214EC07BC0A6230");

            entity.Property(e => e.IsPublished).HasDefaultValue(false);

            entity.HasOne(d => d.Group).WithOne(p => p.FinalGrade).HasConstraintName("FK__FinalGrad__Group__2BFE89A6");
        });

        modelBuilder.Entity<GradeComplaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId).HasName("PK__GradeCom__740D898FD9546DC6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Group).WithMany(p => p.GradeComplaints).HasConstraintName("FK__GradeComp__Group__30C33EC3");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.GradeComplaintResolvedByNavigations).HasConstraintName("FK__GradeComp__Resol__32AB8735");

            entity.HasOne(d => d.SubmittedByNavigation).WithMany(p => p.GradeComplaintSubmittedByNavigations).HasConstraintName("FK__GradeComp__Submi__31B762FC");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__149AF36ADC67C265");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Forming");

            entity.HasOne(d => d.Leader).WithMany(p => p.Groups).HasConstraintName("FK__Groups__LeaderId__66603565");
        });

        modelBuilder.Entity<GroupInvitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PK__GroupInv__033C8DCF43D4832F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupInvitations).HasConstraintName("FK__GroupInvi__Group__71D1E811");

            entity.HasOne(d => d.Receiver).WithMany(p => p.GroupInvitationReceivers).HasConstraintName("FK__GroupInvi__Recei__73BA3083");

            entity.HasOne(d => d.Sender).WithMany(p => p.GroupInvitationSenders).HasConstraintName("FK__GroupInvi__Sende__72C60C4A");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GroupMem__3214EC078A4895B7");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.RoleInGroup).HasDefaultValue("Member");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupMemb__Group__6C190EBB");

            entity.HasOne(d => d.User).WithOne(p => p.GroupMember)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupMemb__UserI__6D0D32F4");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoginHis__3214EC070261AB16");

            entity.Property(e => e.LoginTime).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.LoginHistories).HasConstraintName("FK__LoginHist__UserI__5CD6CB2B");
        });

        modelBuilder.Entity<MentorAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MentorAs__3214EC077D88805E");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Group).WithOne(p => p.MentorAssignment).HasConstraintName("FK__MentorAss__Group__7E37BEF6");

            entity.HasOne(d => d.Mentor).WithMany(p => p.MentorAssignments).HasConstraintName("FK__MentorAss__Mento__7F2BE32F");
        });

        modelBuilder.Entity<MentorRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MentorRe__3214EC07BCE2BF65");

            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Group).WithMany(p => p.MentorRequests).HasConstraintName("FK__MentorReq__Group__787EE5A0");

            entity.HasOne(d => d.Mentor).WithMany(p => p.MentorRequests).HasConstraintName("FK__MentorReq__Mento__797309D9");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC07B8316D54");

            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens).HasConstraintName("FK__PasswordR__UserI__60A75C0F");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__RefreshT__658FEEEA870856C2");

            entity.Property(e => e.IsRevoked).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__59063A47");
        });



        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__Topics__022E0F5DAF8BACA3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CurrentVersion).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue("Draft");

            entity.HasOne(d => d.Group).WithOne(p => p.Topic).HasConstraintName("FK__Topics__GroupId__05D8E0BE");
        });

        modelBuilder.Entity<TopicVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TopicVer__3214EC076FE39582");

            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.TopicVersions).HasConstraintName("FK__TopicVers__Revie__0A9D95DB");

            entity.HasOne(d => d.Topic).WithMany(p => p.TopicVersions).HasConstraintName("FK__TopicVers__Topic__09A971A2");
        });

        modelBuilder.Entity<WeekDefinition>(entity =>
        {
            entity.HasKey(e => e.WeekId).HasName("PK__WeekDefi__C814A5C1CCD2D069");
        });

        modelBuilder.Entity<WeeklyEvaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WeeklyEv__3214EC07EBCFAFD1");

            entity.Property(e => e.ReviewedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Mentor).WithMany(p => p.WeeklyEvaluations).HasConstraintName("FK__WeeklyEva__Mento__17F790F9");

            entity.HasOne(d => d.Report).WithMany(p => p.WeeklyEvaluations).HasConstraintName("FK__WeeklyEva__Repor__17036CC0");
        });

        modelBuilder.Entity<WeeklyReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__WeeklyRe__D5BD480589502696");

            entity.Property(e => e.Status).HasDefaultValue("Submitted");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Group).WithMany(p => p.WeeklyReports).HasConstraintName("FK__WeeklyRep__Group__123EB7A3");

            entity.HasOne(d => d.Week).WithMany(p => p.WeeklyReports).HasConstraintName("FK__WeeklyRep__WeekI__1332DBDC");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
