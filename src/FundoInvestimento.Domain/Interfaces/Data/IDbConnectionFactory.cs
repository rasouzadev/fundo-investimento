using System.Data;

namespace FundoInvestimento.Domain.Interfaces.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}