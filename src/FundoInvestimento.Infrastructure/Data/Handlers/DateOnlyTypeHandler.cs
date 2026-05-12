using Dapper;
using System.Data;

namespace FundoInvestimento.Infrastructure.Data.Handlers;

/// <summary>
/// Handler para o Dapper converter DateOnly para DbType.Date e vice-versa.
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value)
    {
        if (value is DateOnly dateOnly)
            return dateOnly;

        if (value is DateTime dateTime)
            return DateOnly.FromDateTime(dateTime);

        return DateOnly.Parse(value.ToString()!);
    }
}