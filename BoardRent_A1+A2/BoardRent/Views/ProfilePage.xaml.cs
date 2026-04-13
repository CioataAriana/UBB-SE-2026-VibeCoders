// <copyright file="ProfilePage.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardRent.Views
{
    using System.Diagnostics;
    using BoardRent.ViewModels;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Represents the user profile page where users can manage their details.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePage"/> class.
        /// </summary>
        public ProfilePage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<ProfileViewModel>();
            this.DataContext = this.ViewModel;

            this.Loaded += async (s, e) =>
            {
                await this.ViewModel.LoadProfile();
                Debug.WriteLine($"Username after load: {this.ViewModel.Username}");
            };
        }

        /// <summary>
        /// Gets the view model associated with this page.
        /// </summary>
        public ProfileViewModel ViewModel { get; }
    }
}