// <copyright file="RegisterDataTransferObject.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardRent.DataTransferObjects
{
    using System;

    /// <summary>
    /// Data transfer object used for user registration requests.
    /// </summary>
    public class RegisterDataTransferObject
    {
        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the username chosen by the user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the new account.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the password confirmation to ensure they match.
        /// </summary>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the user (optional).
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the country for the user's address.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the city for the user's address.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the street name for the user's address.
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the street number for the user's address.
        /// </summary>
        public string StreetNumber { get; set; }
    }
}