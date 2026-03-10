using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; protected set; }



        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
