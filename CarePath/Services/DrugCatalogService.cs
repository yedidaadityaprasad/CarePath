using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class DrugCatalogService : IDrugCatalogService
{
    private readonly ApplicationDbContext _db;

    public DrugCatalogService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Drug>> ListAsync(string? search, bool includeInactive)
    {
        var query = _db.Drugs.AsNoTracking().Include(d => d.Category).AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var upper = term.ToUpperInvariant();
            query = query.Where(d => d.Code.StartsWith(upper) || d.Name.Contains(term));
        }
        return await query.OrderBy(d => d.Name).ThenBy(d => d.Strength).ToListAsync();
    }

    public async Task<DrugDetailsViewModel> GetDetailsAsync(int id)
    {
        var drug = await _db.Drugs.AsNoTracking().Include(d => d.Category).GetRequiredAsync(id, "Drug");
        var stock = await _db.WardStocks.AsNoTracking()
            .Include(s => s.Ward)
            .Where(s => s.DrugId == id)
            .OrderBy(s => s.Ward.Code)
            .ToListAsync();
        return new DrugDetailsViewModel { Drug = drug, Stock = stock };
    }

    public async Task<DrugFormViewModel> GetForEditAsync(int id)
    {
        var d = await _db.Drugs.AsNoTracking().GetRequiredAsync(id, "Drug");
        return new DrugFormViewModel
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            DrugCategoryId = d.DrugCategoryId,
            Strength = d.Strength,
            Form = d.Form,
            UnitPrice = d.UnitPrice
        };
    }

    public async Task<int> CreateAsync(DrugFormViewModel model)
    {
        var code = await ValidateAsync(model, null);
        var drug = new Drug { Code = code, IsActive = true };
        Apply(drug, model);
        _db.Drugs.Add(drug);
        await _db.SaveChangesAsync();
        return drug.Id;
    }

    public async Task UpdateAsync(DrugFormViewModel model)
    {
        var drug = await _db.Drugs.GetRequiredAsync(model.Id, "Drug");
        drug.Code = await ValidateAsync(model, drug.Id);
        Apply(drug, model);
        await _db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool active)
    {
        var drug = await _db.Drugs.GetRequiredAsync(id, "Drug");
        if (drug.IsActive == active)
        {
            throw new InvalidStatusTransitionException("Drug", active ? "already active" : "already discontinued", active ? "reinstate" : "discontinue");
        }
        if (!active)
        {
            var pending = await _db.ClinicalOrders.CountAsync(o =>
                o.Type == OrderType.Medication &&
                (o.Status == OrderStatus.Placed || o.Status == OrderStatus.InProgress) &&
                o.MedicationDetail!.DrugId == id);
            if (pending > 0)
            {
                throw new DomainValidationException($"{drug.Name} has {pending} pending medication order(s). Dispense or cancel them before discontinuing.");
            }
        }
        drug.IsActive = active;
        await _db.SaveChangesAsync();
    }

    private async Task<string> ValidateAsync(DrugFormViewModel model, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            throw new DomainValidationException("Code is required.", nameof(model.Code));
        }
        var code = model.Code.Trim().ToUpperInvariant();
        if (await _db.Drugs.AnyAsync(d => d.Code == code && d.Id != excludeId))
        {
            throw new DuplicateValueException("Drug", nameof(model.Code), "code", code);
        }
        if (!await _db.DrugCategories.AnyAsync(c => c.Id == model.DrugCategoryId))
        {
            throw new EntityNotFoundException("Drug category", model.DrugCategoryId ?? 0, nameof(model.DrugCategoryId));
        }
        if (model.UnitPrice is null or < 0)
        {
            throw new DomainValidationException("Unit price must be zero or more.", nameof(model.UnitPrice));
        }
        return code;
    }

    private static void Apply(Drug drug, DrugFormViewModel model)
    {
        drug.Name = (model.Name ?? string.Empty).Trim();
        drug.DrugCategoryId = model.DrugCategoryId!.Value;
        drug.Strength = (model.Strength ?? string.Empty).Trim();
        drug.Form = (model.Form ?? string.Empty).Trim();
        drug.UnitPrice = model.UnitPrice!.Value;
    }
}
