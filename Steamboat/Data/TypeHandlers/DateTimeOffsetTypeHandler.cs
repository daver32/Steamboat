using System;
using System.Data;
using Dapper;

namespace Steamboat.Data.TypeHandlers
{
    internal class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToUnixTimeMilliseconds();
        }

        public override DateTimeOffset Parse(object value)
        {
            if (value is DateTimeOffset dateTimeOffset)
                return dateTimeOffset;

            return DateTimeOffset.FromUnixTimeMilliseconds((long)value).ToUniversalTime();
        }
    }
}