namespace LeaveManagementSystem.Helpers;

public static class PaginationHelper
{
    public static (int skip, int take) GetPaging(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        return ((pageNumber - 1) * pageSize, pageSize);
    }
}
