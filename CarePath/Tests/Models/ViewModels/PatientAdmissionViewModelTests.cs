using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CarePath.Models.ViewModels;
using CarePath.Models.Entities;

namespace CarePath.Tests.Models.ViewModels;

public class PatientViewModelTests
{
    // ── PatientIndexViewModel ──────────────────────────────────────────────────

    [Fact]
    public void PatientIndexViewModel_DefaultPage_IsOne()
    {
        var vm = new PatientIndexViewModel();
        Assert.Equal(1, vm.Page);
    }

    [Fact]
    public void PatientIndexViewModel_DefaultSearch_IsNull()
    {
        var vm = new PatientIndexViewModel();
        Assert.Null(vm.Search);
    }

    [Fact]
    public void PatientIndexViewModel_DefaultIncludeInactive_IsFalse()
    {
        var vm = new PatientIndexViewModel();
        Assert.False(vm.IncludeInactive);
    }

    [Fact]
    public void PatientIndexViewModel_Results_DefaultsToEmpty()
    {
        var vm = new PatientIndexViewModel();
        Assert.NotNull(vm.Results);
        Assert.Empty(vm.Results.Items);
    }

    // ── PatientFormViewModel ───────────────────────────────────────────────────

    [Fact]
    public void PatientFormViewModel_IsEdit_FalseWhenIdIsZero()
    {
        var vm = new PatientFormViewModel { Id = 0 };
        Assert.False(vm.IsEdit);
    }

    [Fact]
    public void PatientFormViewModel_IsEdit_TrueWhenIdIsPositive()
    {
        var vm = new PatientFormViewModel { Id = 5 };
        Assert.True(vm.IsEdit);
    }

    [Fact]
    public void PatientFormViewModel_DefaultCountry_IsIndia()
    {
        var vm = new PatientFormViewModel();
        Assert.Equal("India", vm.Country);
    }

    // ── PatientDetailsViewModel ────────────────────────────────────────────────

    [Fact]
    public void PatientDetailsViewModel_OpenAdmission_ReturnsOpenAdmission()
    {
        var openAdmission = new Admission { Status = AdmissionStatus.Open };
        var closedAdmission = new Admission { Status = AdmissionStatus.Discharged };

        var vm = new PatientDetailsViewModel
        {
            Patient = new Patient(),
            Admissions = new List<Admission> { closedAdmission, openAdmission }
        };

        Assert.Equal(openAdmission, vm.OpenAdmission);
    }

    [Fact]
    public void PatientDetailsViewModel_OpenAdmission_ReturnsNullWhenNoOpenAdmission()
    {
        var vm = new PatientDetailsViewModel
        {
            Patient = new Patient(),
            Admissions = new List<Admission>
            {
                new Admission { Status = AdmissionStatus.Discharged }
            }
        };

        Assert.Null(vm.OpenAdmission);
    }

    [Fact]
    public void PatientDetailsViewModel_DefaultCollections_AreEmpty()
    {
        var vm = new PatientDetailsViewModel { Patient = new Patient() };
        Assert.Empty(vm.Coverages);
        Assert.Empty(vm.Admissions);
        Assert.Empty(vm.UpcomingAppointments);
    }

    // ── AllergyFormViewModel ───────────────────────────────────────────────────

    [Fact]
    public void AllergyFormViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new AllergyFormViewModel();
        Assert.Equal(0, vm.PatientId);
        Assert.Equal(string.Empty, vm.PatientName);
        Assert.Null(vm.Substance);
        Assert.Null(vm.Reaction);
        Assert.Null(vm.Severity);
    }
}

public class AdmissionViewModelTests
{
    // ── AdmissionIndexViewModel ────────────────────────────────────────────────

    [Fact]
    public void AdmissionIndexViewModel_DefaultStatus_IsOpen()
    {
        var vm = new AdmissionIndexViewModel();
        Assert.Equal(AdmissionStatus.Open, vm.Status);
    }

    [Fact]
    public void AdmissionIndexViewModel_DefaultPage_IsOne()
    {
        var vm = new AdmissionIndexViewModel();
        Assert.Equal(1, vm.Page);
    }

    [Fact]
    public void AdmissionIndexViewModel_DefaultSearch_IsNull()
    {
        var vm = new AdmissionIndexViewModel();
        Assert.Null(vm.Search);
    }

    // ── AdmitPatientViewModel ──────────────────────────────────────────────────

    [Fact]
    public void AdmitPatientViewModel_DefaultProperties_AreNull()
    {
        var vm = new AdmitPatientViewModel();
        Assert.Null(vm.PatientId);
        Assert.Null(vm.BedId);
        Assert.Null(vm.AttendingPractitionerId);
        Assert.Null(vm.AdmittedAt);
        Assert.Null(vm.ReasonForAdmission);
    }

    // ── DischargeViewModel ─────────────────────────────────────────────────────

    [Fact]
    public void DischargeViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new DischargeViewModel();
        Assert.Equal(0, vm.AdmissionId);
        Assert.Equal(string.Empty, vm.AdmissionNumber);
        Assert.Equal(string.Empty, vm.PatientName);
        Assert.Null(vm.DischargedAt);
        Assert.Null(vm.Disposition);
        Assert.Null(vm.FinalDiagnosis);
        Assert.Null(vm.Summary);
    }

    // ── TransferViewModel ──────────────────────────────────────────────────────

    [Fact]
    public void TransferViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new TransferViewModel();
        Assert.Equal(0, vm.AdmissionId);
        Assert.Equal(string.Empty, vm.AdmissionNumber);
        Assert.Null(vm.NewBedId);
        Assert.Null(vm.Reason);
    }

    // ── AdmissionNoteViewModel ─────────────────────────────────────────────────

    [Fact]
    public void AdmissionNoteViewModel_DefaultProperties_AreCorrect()
    {
        var vm = new AdmissionNoteViewModel();
        Assert.Equal(0, vm.AdmissionId);
        Assert.Equal(string.Empty, vm.AdmissionNumber);
        Assert.Null(vm.Text);
    }

    // ── VitalsViewModel ────────────────────────────────────────────────────────

    [Fact]
    public void VitalsViewModel_DefaultProperties_AreNull()
    {
        var vm = new VitalsViewModel();
        Assert.Null(vm.TemperatureCelsius);
        Assert.Null(vm.PulseBpm);
        Assert.Null(vm.SystolicBp);
        Assert.Null(vm.DiastolicBp);
        Assert.Null(vm.RespiratoryRate);
        Assert.Null(vm.OxygenSaturation);
    }

    // ── AdmissionDetailsViewModel ──────────────────────────────────────────────

    [Fact]
    public void AdmissionDetailsViewModel_DefaultLengthOfStay_IsZero()
    {
        var vm = new AdmissionDetailsViewModel { Admission = new Admission() };
        Assert.Equal(0, vm.LengthOfStayDays);
    }
}
