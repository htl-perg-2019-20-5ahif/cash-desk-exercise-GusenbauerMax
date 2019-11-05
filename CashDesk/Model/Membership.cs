using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk.Model
{
    public class Membership : IMembership
    {
        public int MembershipId { get; set; }

        [Required]
        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public Boolean IsActive { get; set; }

        public int MemberId { get; set; }

        [Required]
        public Member Member { get; set; }

        public List<Deposit> Deposits { get; set; }

        IMember IMembership.Member => Member;
    }
}
