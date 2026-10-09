using System.Text.Json.Serialization;
using FlowDesk.Application.Models.Dtos;

namespace FlowDesk.Application.Models.Responses;

// Envelope di tutte le risposte: { success, result, errors, totResultNumber }.
public abstract class BaseResponse<T>
{
    public bool Success { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ApplicationErrorDto>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Result { get; set; }

    public BaseResponse<T> WithSuccess()
    {
        Success = true;
        return this;
    }

    public BaseResponse<T> SetResult(T result)
    {
        Result = result;
        return this;
    }

    public BaseResponse<T> WithErrors(IEnumerable<ApplicationErrorDto> errors)
    {
        Success = false;
        Errors = errors.ToList();
        return this;
    }

    public BaseResponse<T> WithError(string field, string message) =>
        WithErrors([new ApplicationErrorDto { Field = field, Message = message }]);
}
