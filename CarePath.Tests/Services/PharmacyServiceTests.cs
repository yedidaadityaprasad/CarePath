using CarePath.Exceptions;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using CarePath.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarePath.Tests.Services;

public class PharmacyServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private async Task<(AdmittedPatient Admitted, Drug Drug, int OrderId)> MedicationOrderAsync(int quantity)
    {
        var admitted = await _ctx.Data.AdmitAsync(_ctx);
        var drug = _ctx.Data.Drug("Ceftriaxone", 45m);
        var orderId = await _ctx.Orders.PlaceMedicationOrderAsync(new MedicationOrderFormViewModel
        {
            AdmissionId = admitted.AdmissionId,
            OrderingPractitionerId = admitted.Physician.Id,
            DrugId = drug.Id,
            Dose = "1 g",
            Route = "IV",
            Frequency = "Daily",
            Quantity = quantity
        });
        return (admitted, drug, orderId);
    }

    [Fact]
    public async Task Dispense_decrements_ward_stock_and_completes_order()
    {
        var (admitted, drug, orderId) = await MedicationOrderAsync(4);
        var stock = _ctx.Data.Stock(admitted.Ward, drug, 10);

        await _ctx.Pharmacy.DispenseAsync(orderId, "Given to ward nurse");

        using var db = _ctx.Fresh();
        Assert.Equal(6, (await db.WardStocks.SingleAsync(s => s.Id == stock.Id)).QuantityOnHand);
        var order = await db.ClinicalOrders.Include(o => o.Dispense).SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(4, order.Dispense!.Quantity);
        Assert.Equal(45m, order.Dispense.UnitPrice);
        var movement = await db.StockMovements.SingleAsync(m => m.WardStockId == stock.Id && m.Type == StockMovementType.Dispense);
        Assert.Equal(-4, movement.QuantityChange);
        Assert.Equal(6, movement.QuantityAfter);
    }

    [Fact]
    public async Task Dispense_more_than_on_hand_is_rejected_and_stock_unchanged()
    {
        var (admitted, drug, orderId) = await MedicationOrderAsync(12);
        var stock = _ctx.Data.Stock(admitted.Ward, drug, 10);

        await Assert.ThrowsAsync<InsufficientStockException>(() => _ctx.Pharmacy.DispenseAsync(orderId, null));

        using var db = _ctx.Fresh();
        Assert.Equal(10, (await db.WardStocks.SingleAsync(s => s.Id == stock.Id)).QuantityOnHand);
        Assert.Equal(OrderStatus.Placed, (await db.ClinicalOrders.SingleAsync(o => o.Id == orderId)).Status);
    }

    [Fact]
    public async Task Dispense_when_ward_does_not_stock_the_drug_is_rejected()
    {
        var (_, _, orderId) = await MedicationOrderAsync(1);

        await Assert.ThrowsAsync<InsufficientStockException>(() => _ctx.Pharmacy.DispenseAsync(orderId, null));
    }

    [Fact]
    public async Task Restock_adds_quantity_and_movement()
    {
        var ward = _ctx.Data.Ward();
        var stock = _ctx.Data.Stock(ward, _ctx.Data.Drug(), 3);

        await _ctx.Pharmacy.RestockAsync(new RestockViewModel { StockId = stock.Id, Quantity = 50, Reference = "DN-2291" });

        using var db = _ctx.Fresh();
        var saved = await db.WardStocks.Include(s => s.Movements).SingleAsync(s => s.Id == stock.Id);
        Assert.Equal(53, saved.QuantityOnHand);
        Assert.Contains(saved.Movements, m => m.Type == StockMovementType.Restock && m.Reference == "DN-2291");
        Assert.NotNull(saved.LastRestockedAt);
    }

    [Fact]
    public async Task Duplicate_stock_line_for_ward_and_drug_is_rejected()
    {
        var ward = _ctx.Data.Ward();
        var drug = _ctx.Data.Drug();
        _ctx.Data.Stock(ward, drug, 5);

        await Assert.ThrowsAsync<DuplicateValueException>(() =>
            _ctx.Pharmacy.CreateStockAsync(new WardStockFormViewModel { WardId = ward.Id, DrugId = drug.Id, InitialQuantity = 1, ReorderLevel = 1 }));
    }
}
