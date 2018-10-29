using System;
using System.Collections.Generic;

namespace BogusDataGenerator.Test
{
    public class Human
    {

        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Nationality[] Nationalities { get; set; }

        public DateTime BirthDate { get; set; }

        // Conditional
        public string[] ChildrenNames { get; set; }

        public ICollection<Address> Addresses { get; set; }

        public int? SpouseId { get; set; }
        public Human Spouse { get; set; }

    }
}
