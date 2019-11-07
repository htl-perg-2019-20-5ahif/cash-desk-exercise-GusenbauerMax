using CashDesk.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        private CashDeskDataContext dataContext;

        /// <inheritdoc />
        public Task InitializeDatabaseAsync()
        {
            if (dataContext != null)
            {
                throw new InvalidOperationException("Already called");
            }
            dataContext = new CashDeskDataContext();

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called");
            }

            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentException("Firstname must not be null or empty");
            }

            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentException("lastname must not be null or empty");
            }

            if (birthday == null)
            {
                throw new ArgumentException("Birthday must not be null");
            }

            if ((await dataContext.Members
                .Where(m => m.LastName.Equals(lastName))
                .ToArrayAsync()).Length != 0)
            {
                throw new DuplicateNameException();
            }

            var member = new Member { FirstName = firstName, LastName = lastName, Birthday = birthday };

            await dataContext.Members.AddAsync(member);
            await dataContext.SaveChangesAsync();

            return member.MemberNumber;
        }

        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber)
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called");
            }

            var Member = dataContext.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber);

            if (Member == null)
            {
                throw new ArgumentException();
            }

            dataContext.Members.Remove(Member);
            await dataContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called");
            }

            var Member = dataContext.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber);
            if (Member == null)
            {
                throw new ArgumentException();
            }

            var Membership = dataContext.Memberships
                                .FirstOrDefault(m => m.MemberId == memberNumber);

            if (await dataContext.Memberships
                    .AnyAsync(m => m.Member.MemberNumber == memberNumber && m.IsActive == true))
            {
                throw new AlreadyMemberException("already an active member");
            }

            var NewMembership = new Membership
            {
                Member = Member,
                Begin = DateTime.Now,
                IsActive = true
            };

            await dataContext.Memberships.AddAsync(NewMembership);
            await dataContext.SaveChangesAsync();

            return NewMembership;
        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called");
            }

            if ((dataContext.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber)) == null)
            {
                throw new ArgumentException();
            }

            if ((dataContext.Memberships
                    .FirstOrDefault(m => m.Member.MemberNumber == memberNumber)) == null)
            {
                throw new NoMemberException("currently not an active member");
            }

            var Membership = dataContext.Memberships.First(m => m.MemberId == memberNumber);
            Membership.End = DateTime.Now;
            Membership.IsActive = false;

            await dataContext.SaveChangesAsync();

            return Membership;
        }

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called");
            }

            var Member = dataContext.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber);
            if (Member == null || amount <= 0)
            {
                throw new ArgumentException();
            }

            if ((dataContext.Memberships
                    .FirstOrDefault(m => m.Member.MemberNumber == memberNumber)) == null)
            {
                throw new NoMemberException("currently not an active member");
            }

            var Deposit = new Deposit
            {
                Amount = amount,
                Membership = dataContext.Memberships.First(m => m.MemberId == memberNumber)
            };

            await dataContext.Deposits.AddAsync(Deposit);
            await dataContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("InitializeDatabaseAsync has not been called");
            }

            var statistics = new List<IDepositStatistics>();

            return (await dataContext.Deposits.Include("Membership.Member").ToArrayAsync())
               .GroupBy(d => new { d.Membership.Begin.Year, d.Membership.Member })
               .Select(i => new DepositStatistics
               {
                   Year = i.Key.Year,
                   Member = i.Key.Member,
                   TotalAmount = i.Sum(d => d.Amount)
               });

        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (dataContext != null)
            {
                dataContext.Dispose();
                dataContext = null;
            }
        }
    }
}
