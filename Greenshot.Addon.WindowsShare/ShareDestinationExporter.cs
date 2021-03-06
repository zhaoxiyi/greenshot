﻿//  Greenshot - a free and open source screenshot tool
//  Copyright (C) 2007-2017 Thomas Braun, Jens Klingen, Robin Krom
// 
//  For more information see: http://getgreenshot.org/
//  The Greenshot project is hosted on GitHub: https://github.com/greenshot
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 1 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

#region Usings

using System.ComponentModel.Composition;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Dapplo.Addons;
using Dapplo.Log;
using Greenshot.Addon.Core;
using Greenshot.Core.Interfaces;

#endregion

namespace Greenshot.Addon.WindowsShare
{
	/// <summary>
	///     Export the Share destination, if Windows 10 is used
	/// </summary>
	[StartupAction(StartupOrder = (int) GreenshotStartupOrder.Addon)]
	public class ShareDestinationExporter : IStartupAction
	{
		private static readonly LogSource Log = new LogSource();

		[Import]
		private IServiceExporter ServiceExporter { get; set; }

		[Import]
		private IServiceLocator ServiceLocator { get; set; }

		/// <summary>
		///     Export the destination if the factory can be created
		/// </summary>
		public void Start()
		{
			try
			{
				// Only export if the factory can be intiated
				WindowsRuntimeMarshal.GetActivationFactory(typeof(DataTransferManager));
				var shareDestination = new ShareDestination();
				ServiceLocator.FillImports(shareDestination);

				ServiceExporter.Export<IDestination>(shareDestination);
			}
			catch
			{
				// Ignore
				Log.Info().WriteLine("Share button disabled.");
			}
		}
	}
}