using Dapper;
using System.Data;

namespace Infrastructure.Persistence.Dapper.TypeHandlers;

/// <summary>
/// 讓 Dapper 能將 SQL Server 的 date / datetime 欄位對應到 C# DateOnly。
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public static readonly DateOnlyTypeHandler Instance = new();

    private DateOnlyTypeHandler() { }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) =>
        DateOnly.FromDateTime(Convert.ToDateTime(value));
}
