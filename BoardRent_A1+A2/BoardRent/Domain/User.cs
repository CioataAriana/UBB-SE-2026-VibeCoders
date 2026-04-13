// <copyright file="User.cs" company="BoardRent">
// Copyright (c) 2024 BoardRent. All rights reserved.
// </copyright>

namespace BoardRent.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a user entity in the BoardRent platform.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the URL or local path to the user's avatar image.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's account is suspended.
        /// </summary>
        public bool IsSuspended { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the country of the user's address.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the city of the user's address.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the street name of the user's address.
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the street number of the user's address.
        /// </summary>
        public string StreetNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of roles assigned to this user.
        /// </summary>
        public List<Role> Roles { get; set; } = new List<Role>();
    }
}