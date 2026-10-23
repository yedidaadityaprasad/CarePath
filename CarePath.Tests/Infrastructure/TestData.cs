using CarePath.Data;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Tests.Infrastructure;

public sealed record AdmittedPatient(int AdmissionId, Patient Patient, Practitioner Physician, Ward Ward, Bed Bed, InsuranceCoverage Coverage);

/// <summary>Inserts reference data directly so each test only exercises the service under test.</summary>
public sealed class TestData
{
    private readonly ApplicationDbContext _db;
    private readonly FakeClock _clock;
    private int _sequence;

    public TestData(ApplicationDbContext db, FakeClock clock)
    {
        _db = db;
        _clock = clock;
    }

    private int Next() => ++_sequence;

    public Patient Patient(string? mrn = null, bool active = true, string lastName = "Sharma")
    {
        var n = Next();
        var patient = new Patient
        {
            MedicalRecordNumber = mrn ?? $"MRN-{1000 + n}",
            FirstName = $"Asha{n}",
            LastName = lastName,
            DateOfBirth = new DateTime(1980, 5, 17),
            Gender = Gender.Female,
            IsActive = active,
            RegisteredAt = _clock.Now.AddMonths(-6)
        };
        _db.Patients.Add(patient);
        _db.SaveChanges();
        return patient;
    }

    public Payer Payer(string? code = null, decimal defaultPercent = 80m, bool active = true)
    {
        var n = Next();
        var payer = new Payer
        {
            Code = code ?? $"PAY{n}",
            Name = $"Payer {n}",
            DefaultCoveragePercent = defaultPercent,
            IsActive = active
        };
        _db.Payers.Add(payer);
        _db.SaveChanges();
        return payer;
    }

    public InsuranceCoverage Coverage(Patient patient, Payer payer, decimal percent = 80m, DateTime? from = null, DateTime? to = null)
    {
        var coverage = new InsuranceCoverage
        {
            PatientId = patient.Id,
            PayerId = payer.Id,
            MemberId = $"M-{Next():0000}",
            EffectiveFrom = from ?? _clock.Now.Date.AddMonths(-3),
            EffectiveTo = to,
            CoveragePercent = percent,
            IsActive = true,
            CreatedAt = _clock.Now
        };
        _db.InsuranceCoverages.Add(coverage);
        _db.SaveChanges();
        return coverage;
    }

    public Practitioner Practitioner(PractitionerType type = PractitionerType.Physician, bool active = true)
    {
        var n = Next();
        var practitioner = new Practitioner
        {
            FirstName = $"Ravi{n}",
            LastName = "Menon",
            Type = type,
            LicenseNumber = $"LIC-{5000 + n}",
            IsActive = active,
            CreatedAt = _clock.Now.AddYears(-1)
        };
        _db.Practitioners.Add(practitioner);
        _db.SaveChanges();
        return practitioner;
    }

    public Ward Ward(decimal dailyRate = 200m, bool closed = false)
    {
        var n = Next();
        var ward = new Ward
        {
            Code = $"W{n}",
            Name = $"Ward {n}",
            DepartmentId = 1,
            Floor = 2,
            DailyRate = dailyRate,
            IsClosed = closed
        };
        _db.Wards.Add(ward);
        _db.SaveChanges();
        return ward;
    }

    public Bed Bed(Ward ward, string? number = null, BedStatus status = BedStatus.Available)
    {
        var bed = new Bed { WardId = ward.Id, Ward = ward, BedNumber = number ?? $"B{Next()}", Status = status };
        _db.Beds.Add(bed);
        _db.SaveChanges();
        return bed;
    }

    public LabTestDefinition NumericLabTest(decimal low = 12m, decimal high = 16m, decimal price = 25m)
    {
        var n = Next();
        var test = new LabTestDefinition
        {
            Code = $"HB{n}",
            Name = $"Haemoglobin {n}",
            SpecimenTypeId = 1,
            ResultType = ResultValueType.Numeric,
            Unit = "g/dL",
            ReferenceLow = low,
            ReferenceHigh = high,
            Price = price,
            IsActive = true
        };
        _db.LabTestDefinitions.Add(test);
        _db.SaveChanges();
        return test;
    }

    public Drug Drug(string name = "Paracetamol", decimal unitPrice = 1.50m, bool active = true)
    {
        var drug = new Drug
        {
            Code = $"D{Next()}",
            Name = name,
            DrugCategoryId = 1,
            Strength = "500 mg",
            Form = "Tablet",
            UnitPrice = unitPrice,
            IsActive = active
        };
        _db.Drugs.Add(drug);
        _db.SaveChanges();
        return drug;
    }

    public WardStock Stock(Ward ward, Drug drug, int quantity, int reorderLevel = 5)
    {
        var stock = new WardStock { WardId = ward.Id, DrugId = drug.Id, ReorderLevel = reorderLevel };
        stock.Apply(StockMovementType.Opening, quantity, _clock.Now.AddDays(-10), "seed", "Opening balance");
        _db.WardStocks.Add(stock);
        _db.SaveChanges();
        return stock;
    }

    public AdmitPatientViewModel AdmitForm(Patient patient, Bed bed, Practitioner physician, DateTime? admittedAt = null) => new()
    {
        PatientId = patient.Id,
        BedId = bed.Id,
        AttendingPractitionerId = physician.Id,
        AdmittedAt = admittedAt ?? _clock.Now.AddDays(-2),
        ReasonForAdmission = "Community acquired pneumonia",
        DiagnosisCode = "j18.9",
        DiagnosisDescription = "Pneumonia, unspecified organism"
    };

    /// <summary>Creates a covered patient and admits them through the real admission workflow.</summary>
    public async Task<AdmittedPatient> AdmitAsync(ServiceTestContext context, decimal dailyRate = 200m, decimal coveragePercent = 80m, DateTime? admittedAt = null)
    {
        var patient = Patient();
        var coverage = Coverage(patient, Payer(), coveragePercent);
        var physician = Practitioner();
        var ward = Ward(dailyRate);
        var bed = Bed(ward);
        var admissionId = await context.Admissions.AdmitAsync(AdmitForm(patient, bed, physician, admittedAt));
        return new AdmittedPatient(admissionId, patient, physician, ward, bed, coverage);
    }

    public DischargeViewModel DischargeForm(int admissionId, DateTime? dischargedAt = null) => new()
    {
        AdmissionId = admissionId,
        DischargedAt = dischargedAt ?? _clock.Now,
        Disposition = "Home",
        FinalDiagnosis = "J18.9 Pneumonia",
        Summary = "Responded to IV antibiotics; afebrile for 48 hours.",
        FollowUpInstructions = "GP review in one week"
    };
}
