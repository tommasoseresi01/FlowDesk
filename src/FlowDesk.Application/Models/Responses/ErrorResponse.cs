namespace FlowDesk.Application.Models.Responses;

// Risposta di errore (401, 403, 404, 422, 500): stesso envelope, senza payload.
public class ErrorResponse : BaseResponse<object>
{
}
