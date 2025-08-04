using BBWM.Core.DTO;
using System;

namespace BBWM.SSRS
{
    public class CatalogDTO : IDTO<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
