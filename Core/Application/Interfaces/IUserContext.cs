namespace Application.Interfaces
{
    /// <summary>
    /// Provides access to the current authenticated user's information.
    /// All properties read directly from claims - no async loading needed.
    /// Usage: @inject IUserContext UserContext, then UserContext.Email, UserContext.Id, etc.
    /// </summary>
    public interface IUserContext
    {
        int? Id { get; }
        bool IsAuthenticated { get; }
        string Email { get; }
        string FirstName { get; }

        /// <summary>
        /// Gets the current user's last name, or empty string if not authenticated.
        /// </summary>
        string LastName { get; }

        string FullName { get; }

        /// <summary>
        /// Gets the avatar initials (first letter of first name + first letter of last name)
        /// </summary>
        string Initials { get; }

        /// <summary>
        /// If this user is an Agent, returns the Manager's User ID. Null for top-level Managers.
        /// </summary>
        int? ParentUserId { get; }

        /// <summary>
        /// Returns true if this user is a sub-user (Agent) created by a Manager.
        /// </summary>
        bool IsSubUser { get; }
    }
}