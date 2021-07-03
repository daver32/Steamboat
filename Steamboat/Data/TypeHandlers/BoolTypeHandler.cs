using System.Data;
using Dapper;

namespace Steamboat.Data.TypeHandlers
{
    internal class BoolTypeHandler : SqlMapper.TypeHandler<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
        }

        public override bool Parse(object value)
        {
            if (value is bool b)
                return b;

            return (long)value != 0;
        }
    }
}