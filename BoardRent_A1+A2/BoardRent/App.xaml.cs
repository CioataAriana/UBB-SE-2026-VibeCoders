// <copyright file="App.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardRent
{
    using System;
    using BoardRent.Data;
    using BoardRent.Repositories;
    using BoardRent.Services;
    using BoardRent.ViewModels;
    using BoardRent.Views;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public partial class App : Application
    {
        public static Window _window;
        private static Frame _rootFrame;

        public App()
        {
            this.InitializeComponent();

            Ioc.Default.ConfigureServices(
                new ServiceCollection()

                .AddSingleton<AppDbContext>()
                .AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>()

                .AddSingleton<IUserRepository, UserRepository>()
                .AddSingleton<IFailedLoginRepository, FailedLoginRepository>()

                .AddSingleton<IAuthService, AuthService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IAdminService, AdminService>()
                .AddSingleton<IFilePickerService, FilePickerService>()

                .AddTransient<LoginViewModel>()
                .AddTransient<RegisterViewModel>()
                .AddTransient<ProfileViewModel>()
                .AddTransient<AdminViewModel>()

                .BuildServiceProvider());
        }

        public static void NavigateTo(Type pageType, bool clearBackStack = false)
        {
            _rootFrame?.Navigate(pageType);
            if (clearBackStack && _rootFrame != null)
            {
                _rootFrame.BackStack.Clear();
            }
        }

        public static void NavigateBack()
        {
            if (_rootFrame != null && _rootFrame.CanGoBack)
            {
                _rootFrame.GoBack();
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _rootFrame = new Frame();
            _window.Content = _rootFrame;
            _window.Activate();

            var db = new AppDbContext();
            db.EnsureCreated();

            NavigateTo(typeof(LoginPage));
        }
    }
}