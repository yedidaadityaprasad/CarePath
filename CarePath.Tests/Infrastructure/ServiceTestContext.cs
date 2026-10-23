using CarePath.Data;
using CarePath.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Tests.Infrastructure;

/// <summary>
/// One in-memory SQLite database per test. The connection stays open for the lifetime of the
/// context because the database disappears when the last connection closes.
/// </summary>
public sealed class ServiceTestContext : IDisposable
{
    public static readonly DateTime DefaultNow = new(2026, 3, 10, 9, 0, 0);

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ServiceTestContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new SqliteApplicationDbContext(_options);
        Db.Database.EnsureCreated();
        Clock = new FakeClock(DefaultNow);
        User = new FakeCurrentUser("nurse.jones@carepath.test");
        Audit = new AuditService(Db, User, Clock);
        Data = new TestData(Db, Clock);
    }

    public ApplicationDbContext Db { get; }
    public FakeClock Clock { get; }
    public FakeCurrentUser User { get; }
    public AuditService Audit { get; }
    public TestData Data { get; }

    public PatientService Patients => new(Db, Clock, User, Audit);
    public PayerService Payers => new(Db);
    public CoverageService Coverage => new(Db, Clock, Audit);
    public PractitionerService Practitioners => new(Db, Clock, User, Audit);
    public WardService Wards => new(Db, Clock, User, Audit);
    public BillingService Billing => new(Db, Clock, User, Audit);
    public AdmissionService Admissions => new(Db, Clock, User, Audit, Coverage, Billing);
    public ShiftService Shifts => new(Db, Clock, User, Audit);
    public AppointmentService Appointments => new(Db, Clock, User);
    public OrderService Orders => new(Db, Clock, User, Audit);
    public LabService Lab => new(Db, Clock, User, Audit);
    public PharmacyService Pharmacy => new(Db, Clock, User, Audit);

    /// <summary>A second context on the same database, for asserting on persisted state without the change tracker.</summary>
    public ApplicationDbContext Fresh() => new SqliteApplicationDbContext(_options);

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
