using System.ComponentModel.DataAnnotations;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class AppointmentIndexViewModel
{
    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    [Display(Name = "Practitioner")]
    public int? PractitionerId { get; set; }

    public AppointmentStatus? Status { get; set; }

    public IReadOnlyList<Appointment> Appointments { get; set; } = Array.Empty<Appointment>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Practitioners { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class AppointmentFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Patient")]
    public int? PatientId { get; set; }

    [Required]
    [Display(Name = "Practitioner")]
    public int? PractitionerId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Starts at")]
    public DateTime? StartsAt { get; set; }

    [Required]
    [Range(5, 240)]
    [Display(Name = "Duration (minutes)")]
    public int? DurationMinutes { get; set; } = 30;

    [Required, StringLength(500, MinimumLength = 3)]
    public string? Reason { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Patients { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Practitioners { get; set; } = Enumerable.Empty<SelectListItem>();

    public bool IsEdit => Id > 0;
}

public class CompleteAppointmentViewModel
{
    public int Id { get; set; }

    [StringLength(2000)]
    [Display(Name = "Completion notes")]
    public string? Notes { get; set; }
}

public class AppointmentDetailsViewModel
{
    public Appointment Appointment { get; set; } = null!;
    public CompleteAppointmentViewModel Complete { get; set; } = new();
}
