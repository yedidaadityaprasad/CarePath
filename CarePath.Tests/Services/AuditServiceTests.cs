using CarePath.Exceptions;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Xunit;

namespace CarePath.Tests.Services;

public class AuditServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task Recorded_entries_are_searchable_by_entity_and_user()
    {
        _ctx.Audit.Record("Admit", "Admission", 42, "ADM-1 admitted");
        _ctx.Clock.Advance(TimeSpan.FromMinutes(5));
        _ctx.User.UserName = "dr.rao@carepath.test";
        _ctx.Audit.Record("Discharge", "Admission", 42, "ADM-1 discharged");
        _ctx.Audit.Record("Restock", "WardStock", 7);
        await _ctx.Db.SaveChangesAsync();

        var forAdmission = await _ctx.Audit.SearchAsync(new AuditIndexViewModel { EntityName = "Admission", EntityId = "42" });
        Assert.Equal(2, forAdmission.TotalCount);
        Assert.Equal("Discharge", forAdmission.Items[0].Action);

        var byUser = await _ctx.Audit.SearchAsync(new AuditIndexViewModel { UserName = "dr.rao" });
        Assert.Equal(2, byUser.TotalCount);

        var details = await _ctx.Audit.GetDetailsAsync(forAdmission.Items[0].Id);
        Assert.Single(details.RelatedEntries);
        Assert.Equal(new[] { "Admission", "WardStock" }, await _ctx.Audit.GetEntityNamesAsync());
    }

    [Fact]
    public void Recording_before_entity_is_saved_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(() => _ctx.Audit.Record("Register", "Patient", 0));
    }

    [Fact]
    public async Task Search_with_to_before_from_is_rejected()
    {
        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _ctx.Audit.SearchAsync(new AuditIndexViewModel { From = new DateTime(2026, 3, 10), To = new DateTime(2026, 3, 1) }));
        Assert.Equal(nameof(AuditIndexViewModel.To), ex.PropertyName);
    }

    [Fact]
    public async Task Missing_entry_raises_not_found()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _ctx.Audit.GetDetailsAsync(999));
    }
}
