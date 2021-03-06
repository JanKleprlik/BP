﻿using Yoti.Shared.ViewModels;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yoti.Shared.Views
{
    public sealed partial class LyricsShowDialog : ContentDialog
    {

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="song">Song whose lyrics are to be displayed.</param>
        public LyricsShowDialog(Song song)
        {
            this.InitializeComponent();
			LyricsShowViewModel = new LyricsShowViewModel(song);
        }

		/// <summary>
		/// Lyrics show view model
		/// </summary>
		public LyricsShowViewModel LyricsShowViewModel;

    }
}
