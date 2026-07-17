using Common.Results;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Infrastructure.Web.Mvc;

public static class TempDataFlashExtensions
{
    public static void SetSuccess(this ITempDataDictionary tempData, string message)
        => tempData[FlashMessageKeys.Success] = message;

    public static void SetError(this ITempDataDictionary tempData, string message)
        => tempData[FlashMessageKeys.Error] = message;

    public static void SetResultMessage(this ITempDataDictionary tempData, Result result, string successMessage, string? failurePrefix = null)
    {
        if (result.IsSuccess)
        {
            tempData.SetSuccess(successMessage);
            return;
        }

        tempData.SetError((failurePrefix ?? string.Empty) + result.Error.Description);
    }

    public static string? GetSuccess(this ITempDataDictionary tempData)
        => tempData[FlashMessageKeys.Success] as string;

    public static string? GetError(this ITempDataDictionary tempData)
        => tempData[FlashMessageKeys.Error] as string;
}
