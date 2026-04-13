// <copyright file="UserRole.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardRent.Domain
{
    using System;

    /// <summary>
    /// Represents the many-to-many relationship between users and roles.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the role.
        /// </summary>
        public Guid RoleId { get; set; }
    }
}