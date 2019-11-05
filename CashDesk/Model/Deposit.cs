using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk.Model
{
    public class Deposit : IDeposit
    {
        public int DepositId { get; set; }

        public decimal Amount { get; set; }

        public int MembershipId { get; set; }

        [Required]
        public Membership Membership { get; set; }

        IMembership IDeposit.Membership => Membership;
    }
}
