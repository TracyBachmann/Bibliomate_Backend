namespace BackendBiblioMate.Models.Enums
{
    /// <summary>
    /// Defines application roles used for authorization checks.
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// Role for administrators with full access.
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// Role for librarians with permissions to manage library resources.
        /// </summary>
        public const string Librarian = "Librarian";

        /// <summary>
        /// Role for general users with standard borrowing privileges.
        /// </summary>
        public const string User = "User";
    }
}