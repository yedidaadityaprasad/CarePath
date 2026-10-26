using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CarePath.Infrastructure;

/// <summary>
/// Extension methods for storing flash messages in TempData.
/// .NET 10: ITempDataDictionary is unchanged; cookie-based TempData provider is the default.
/// </summary>
public static class TempDataExtensions
{
    public const string SuccessKey = "Flash.Success";
    public const string ErrorKey = "Flash.Error";

    public static void Success(this ITempDataDictionary tempData, string message)
        => tempData[SuccessKey] = message;

    public static void Error(this ITempDataDictionary tempData, string message)
        => tempData[ErrorKey] = message;
}
