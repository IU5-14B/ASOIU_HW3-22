namespace ASOIU_3.Services;

internal readonly record struct ServiceResult(bool IsSuccess, string Message)
{
    public static ServiceResult Ok(string message) => new(true, message);

    public static ServiceResult Fail(string message) => new(false, message);
}
