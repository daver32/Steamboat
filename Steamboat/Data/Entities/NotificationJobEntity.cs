﻿namespace Steamboat.Data.Entities
{
    internal record NotificationJobEntity
    {
        public int Id { get; init; }
        public string AppName { get; init; } = null!;
        public int AppId { get; init; } 
    }
}