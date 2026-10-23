using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CarePath.Infrastructure;

public static class TempDataExtensions
{
    public const string SuccessKey = "Flash.Success";
    public const string ErrorKey = "Flash.Error";

    public static void Success(this ITempDataDictionary tempData, string message) => tempData[SuccessKey] = message;

    public static void Error(this ITempDataDictionary tempData, string message) => tempData[ErrorKey] = message;
}
