using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    internal static class ActivityLogFactory
    {
        public static ActivityLog Create(
            IUserContext userContext,
            string action,
            string entityName,
            string entityId,
            string description)
        {
            return new ActivityLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                UserId = userContext.Id?.ToString() ?? string.Empty,
                UserName = GetUserName(userContext),
                Timestamp = DateTime.Now
            };
        }

        private static string GetUserName(IUserContext userContext)
        {
            if (!string.IsNullOrWhiteSpace(userContext.FullName))
            {
                return userContext.FullName;
            }

            if (!string.IsNullOrWhiteSpace(userContext.Email))
            {
                return userContext.Email;
            }

            return "System";
        }
    }
}
