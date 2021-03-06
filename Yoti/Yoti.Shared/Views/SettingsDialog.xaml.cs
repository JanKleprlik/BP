﻿using Windows.UI.Xaml.Controls;
using Yoti.Shared.ViewModels;


namespace Yoti.Shared.Views
{
	public sealed partial class SettingsDialog : ContentDialog
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="settingsVM">Reference to settings view model</param>
		public SettingsDialog(SettingsViewModel settingsVM)
		{
			this.InitializeComponent();
			SettingsViewModel = settingsVM;
		}

		/// <summary>
		/// Settings view model
		/// </summary>
		public SettingsViewModel SettingsViewModel;
	}
}
