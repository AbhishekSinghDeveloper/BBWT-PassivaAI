using BBWM.Core.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace BBWM.SSRS
{
    public class Catalog : IEntity<Guid>
    {
        /// <summary>
        /// Item ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Report Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Report Path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Type of entity (2 - Report).
        /// </summary>
        public int Type { get; set; }
    }
}
