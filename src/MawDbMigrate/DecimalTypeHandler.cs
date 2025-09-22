using System.Data;
using Dapper;

namespace MawDbMigrate;

public class DecimalTypeHandler : SqlMapper.TypeHandler<decimal>
{
    public override decimal Parse(object value)
    {
        return decimal.Parse(value.ToString()!);
    }

    public override void SetValue(IDbDataParameter parameter, decimal value)
    {
        parameter.Value = value;
    }
}
