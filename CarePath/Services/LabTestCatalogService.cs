using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class LabTestCatalogService : ILabTestCatalogService
{
    private readonly ApplicationDbContext _db;

    public LabTestCatalogService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LabTestDefinition>> ListAsync(bool includeInactive)
    {
        var query = _db.LabTestDefinitions.AsNoTracking().Include(t => t.SpecimenType).AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }
        return await query.OrderBy(t => t.Code).ToListAsync();
    }

    public async Task<LabTestDetailsViewModel> GetDetailsAsync(int id)
    {
        var test = await _db.LabTestDefinitions.AsNoTracking().Include(t => t.SpecimenType).GetRequiredAsync(id, "Lab test");
        var orders = await _db.LabOrderDetails.CountAsync(d => d.LabTestDefinitionId == id);
        return new LabTestDetailsViewModel { Test = test, OrderCount = orders };
    }

    public async Task<LabTestFormViewModel> GetForEditAsync(int id)
    {
        var t = await _db.LabTestDefinitions.AsNoTracking().GetRequiredAsync(id, "Lab test");
        return new LabTestFormViewModel
        {
            Id = t.Id,
            Code = t.Code,
            Name = t.Name,
            SpecimenTypeId = t.SpecimenTypeId,
            ResultType = t.ResultType,
            Unit = t.Unit,
            ReferenceLow = t.ReferenceLow,
            ReferenceHigh = t.ReferenceHigh,
            ReferenceText = t.ReferenceText,
            Price = t.Price
        };
    }

    public async Task<int> CreateAsync(LabTestFormViewModel model)
    {
        var code = await ValidateAsync(model, null);
        var test = new LabTestDefinition { Code = code, IsActive = true };
        Apply(test, model);
        _db.LabTestDefinitions.Add(test);
        await _db.SaveChangesAsync();
        return test.Id;
    }

    public async Task UpdateAsync(LabTestFormViewModel model)
    {
        var test = await _db.LabTestDefinitions.GetRequiredAsync(model.Id, "Lab test");
        var code = await ValidateAsync(model, test.Id);
        if (model.ResultType != test.ResultType && await _db.LabOrderDetails.AnyAsync(d => d.LabTestDefinitionId == test.Id))
        {
            throw new DomainValidationException("The result type cannot change once the test has been ordered; create a new test definition instead.", nameof(model.ResultType));
        }

        test.Code = code;
        Apply(test, model);
        await _db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool active)
    {
        var test = await _db.LabTestDefinitions.GetRequiredAsync(id, "Lab test");
        if (test.IsActive == active)
        {
            throw new InvalidStatusTransitionException("Lab test", active ? "already active" : "already retired", active ? "activate" : "retire");
        }
        test.IsActive = active;
        await _db.SaveChangesAsync();
    }

    private async Task<string> ValidateAsync(LabTestFormViewModel model, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            throw new DomainValidationException("Code is required.", nameof(model.Code));
        }
        var code = model.Code.Trim().ToUpperInvariant();
        if (await _db.LabTestDefinitions.AnyAsync(t => t.Code == code && t.Id != excludeId))
        {
            throw new DuplicateValueException("Lab test", nameof(model.Code), "code", code);
        }
        if (!await _db.SpecimenTypes.AnyAsync(s => s.Id == model.SpecimenTypeId))
        {
            throw new EntityNotFoundException("Specimen type", model.SpecimenTypeId ?? 0, nameof(model.SpecimenTypeId));
        }

        if (model.ResultType == ResultValueType.Numeric)
        {
            if (model.ReferenceLow == null && model.ReferenceHigh == null)
            {
                throw new DomainValidationException("A numeric test needs at least one reference limit.", nameof(model.ReferenceLow));
            }
            if (model.ReferenceLow.HasValue && model.ReferenceHigh.HasValue && model.ReferenceLow > model.ReferenceHigh)
            {
                throw new DomainValidationException("The reference low value must not exceed the reference high value.", nameof(model.ReferenceHigh));
            }
            if (string.IsNullOrWhiteSpace(model.Unit))
            {
                throw new DomainValidationException("A numeric test needs a unit of measure.", nameof(model.Unit));
            }
        }
        else if (string.IsNullOrWhiteSpace(model.ReferenceText))
        {
            throw new DomainValidationException("A text test needs the expected normal result.", nameof(model.ReferenceText));
        }

        if (model.Price is null or < 0)
        {
            throw new DomainValidationException("Price must be zero or more.", nameof(model.Price));
        }
        return code;
    }

    private static void Apply(LabTestDefinition test, LabTestFormViewModel model)
    {
        test.Name = (model.Name ?? string.Empty).Trim();
        test.SpecimenTypeId = model.SpecimenTypeId!.Value;
        test.ResultType = model.ResultType!.Value;
        test.Price = model.Price!.Value;
        if (test.ResultType == ResultValueType.Numeric)
        {
            test.Unit = model.Unit?.Trim();
            test.ReferenceLow = model.ReferenceLow;
            test.ReferenceHigh = model.ReferenceHigh;
            test.ReferenceText = null;
        }
        else
        {
            test.Unit = null;
            test.ReferenceLow = null;
            test.ReferenceHigh = null;
            test.ReferenceText = model.ReferenceText?.Trim();
        }
    }
}
