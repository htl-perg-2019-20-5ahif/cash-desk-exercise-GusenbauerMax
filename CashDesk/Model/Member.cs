using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk.Model
{
    public class Member : IMember
    {
        [Key]
        public int MemberNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public List<Membership> Memberships { get; set; }
    }
}
