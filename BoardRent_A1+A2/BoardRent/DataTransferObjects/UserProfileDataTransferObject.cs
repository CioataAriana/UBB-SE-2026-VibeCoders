// <copyright file="UserProfileDataTransferObject.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardRent.DataTransferObjects
{
    using System;

    /// <summary>
    /// Represents the data transfer object for a user's profile information.
    /// </summary>
    public class UserProfileDataTransferObject
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the URL or local path to the user's avatar image.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the role details associated with the user.
        /// </summary>
        public RoleDataTransferObject Role { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's account is suspended.
        /// </summary>
        public bool IsSuspended { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's account is temporarily locked.
        /// </summary>
        public bool IsLocked { get; set; }

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
    }
}