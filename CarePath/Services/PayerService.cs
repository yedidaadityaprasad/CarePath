using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class PayerService : IPayerService
{
    private readonly ApplicationDbContext _db;

    public PayerService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Payer>> ListAsync(bool includeInactive)
    {
        var query = _db.Payers.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<PayerDetailsViewModel> GetDetailsAsync(int id)
    {
        var payer = await _db.Payers.AsNoTracking().GetRequiredAsync(id, "Payer");
        var activeCoverages = await _db.InsuranceCoverages.CountAsync(c => c.PayerId == id && c.IsActive);
        return new PayerDetailsViewModel { Payer = payer, ActiveCoverageCount = activeCoverages };
    }

    public async Task<PayerFormViewModel> GetForEditAsync(int id)
    {
        var payer = await _db.Payers.AsNoTracking().GetRequiredAsync(id, "Payer");
        return new PayerFormViewModel
        {
            Id = payer.Id,
            Code = payer.Code,
            Name = payer.Name,
            Phone = payer.Phone,
            ClaimsEmail = payer.ClaimsEmail,
            DefaultCoveragePercent = payer.DefaultCoveragePercent
        };
    }

    public async Task<int> CreateAsync(PayerFormViewModel model)
    {
        var (code, name) = Normalize(model);
        await EnsureUniqueAsync(code, name, null);
        var payer = new Payer { Code = code, IsActive = true };
        Apply(payer, model, name);
        _db.Payers.Add(payer);
        await _db.SaveChangesAsync();
        return payer.Id;
    }

    public async Task UpdateAsync(PayerFormViewModel model)
    {
        var payer = await _db.Payers.GetRequiredAsync(model.Id, "Payer");
        var (code, name) = Normalize(model);
        if (!string.Equals(payer.Code, code, StringComparison.Ordinal))
        {
            throw new DomainValidationException("The payer code cannot be changed because it appears on issued invoices.", nameof(model.Code));
        }
        await EnsureUniqueAsync(code, name, payer.Id);
        Apply(payer, model, name);
        await _db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool active)
    {
        var payer = await _db.Payers.GetRequiredAsync(id, "Payer");
        if (payer.IsActive == active)
        {
            throw new InvalidStatusTransitionException("Payer", active ? "already active" : "already inactive", active ? "activate" : "deactivate");
        }
        payer.IsActive = active;
        await _db.SaveChangesAsync();
    }

    private static (string Code, string Name) Normalize(PayerFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            throw new DomainValidationException("Code is required.", nameof(model.Code));
        }
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new DomainValidationException("Name is required.", nameof(model.Name));
        }
        if (model.DefaultCoveragePercent is < 0 or > 100)
        {
            throw new DomainValidationException("Default coverage must be between 0 and 100 percent.", nameof(model.DefaultCoveragePercent));
        }
        return (model.Code.Trim().ToUpperInvariant(), model.Name.Trim());
    }

    private async Task EnsureUniqueAsync(string code, string name, int? excludeId)
    {
        if (await _db.Payers.AnyAsync(p => p.Code == code && p.Id != excludeId))
        {
            throw new DuplicateValueException("Payer", nameof(PayerFormViewModel.Code), "code", code);
        }
        if (await _db.Payers.AnyAsync(p => p.Name == name && p.Id != excludeId))
        {
            throw new DuplicateValueException("Payer", nameof(PayerFormViewModel.Name), "name", name);
        }
    }

    private static void Apply(Payer payer, PayerFormViewModel model, string name)
    {
        payer.Name = name;
        payer.Phone = model.Phone?.Trim();
        payer.ClaimsEmail = model.ClaimsEmail?.Trim();
        payer.DefaultCoveragePercent = model.DefaultCoveragePercent ?? 0;
    }
}
