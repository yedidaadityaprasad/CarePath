using CarePath.Exceptions;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class PayerServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task Create_uppercases_code()
    {
        var id = await _ctx.Payers.CreateAsync(new PayerFormViewModel { Code = "star-h", Name = "Star Health", DefaultCoveragePercent = 75 });

        var payer = await _ctx.Fresh().Payers.SingleAsync(p => p.Id == id);
        Assert.Equal("STAR-H", payer.Code);
        Assert.Equal(75m, payer.DefaultCoveragePercent);
    }

    [Fact]
    public async Task Create_rejects_duplicate_code()
    {
        _ctx.Data.Payer(code: "STAR-H");

        await Assert.ThrowsAsync<DuplicateValueException>(() =>
            _ctx.Payers.CreateAsync(new PayerFormViewModel { Code = "star-h", Name = "Another", DefaultCoveragePercent = 50 }));
    }

    [Fact]
    public async Task Deactivating_twice_is_an_invalid_transition()
    {
        var payer = _ctx.Data.Payer();
        await _ctx.Payers.SetActiveAsync(payer.Id, false);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => _ctx.Payers.SetActiveAsync(payer.Id, false));
    }
}
