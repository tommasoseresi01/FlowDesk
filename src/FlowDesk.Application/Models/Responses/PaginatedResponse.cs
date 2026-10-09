namespace FlowDesk.Application.Models.Responses;

public abstract class PaginatedResponse<T> : BaseResponse<T>
{
    public int TotResultNumber { get; set; }

    public PaginatedResponse<T> SetTotResultNumber(int totResultNumber)
    {
        TotResultNumber = totResultNumber;
        return this;
    }
}
