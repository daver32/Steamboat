using System;
using System.Data;
using Dapper;

namespace Steamboat.Data.TypeHandlers
{
    // https://stackoverflow.com/questions/5898988/map-string-to-guid-with-dapper
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid guid)
        {
            parameter.Value = guid.ToString();
        }
        
        public override Guid Parse(object value)
        {
            // Dapper may pass a Guid instead of a string
            if (value is Guid guid)
                return guid;

            return new Guid((string)value);
        }
    }
}