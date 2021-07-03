using System;
using InterfaceGenerator;

namespace Steamboat.Util.Services
{
    [GenerateAutoInterface]
    internal class GuidProvider : IGuidProvider
    {
        /// <summary>
        /// Equivalent to <see cref="Guid.NewGuid"/> but mockable.
        /// </summary>
        public Guid Create()
        {
            return Guid.NewGuid();
        }
    }
}
