using System;
using Dapper;
using Steamboat.Data.TypeHandlers;

namespace Steamboat.Data
{
    internal static class DapperTypeHandlerInitializer
    {
        public static void Init()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            
            SqlMapper.AddTypeHandler(new BoolTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(bool));
            SqlMapper.RemoveTypeMap(typeof(bool?));
            
            SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(DateTimeOffset));
            SqlMapper.RemoveTypeMap(typeof(DateTimeOffset?));
        }
    }
}