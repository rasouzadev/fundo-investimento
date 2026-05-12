using Dapper;
using System.Data;

namespace FundoInvestimento.Infrastructure.Data.Handlers;

/// <summary>
/// Handler para o Dapper a converter TimeOnly para DbType.Time e vice-versa.
/// </summary>
public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }

    public override TimeOnly Parse(object value)
    {
        if (value is TimeOnly timeOnly)
            return timeOnly;

        if (value is TimeSpan timeSpan)
            return TimeOnly.FromTimeSpan(timeSpan);

        return TimeOnly.Parse(value.ToString()!);
    }
}