//  Greenshot - a free and open source screenshot tool
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
using Dapplo.Addons;
using Greenshot.Addon.Core;
using Greenshot.Addon.Office.Destinations;
using Greenshot.Core.Interfaces;

#endregion

namespace Greenshot.Addon.Office
{
	/// <summary>
	///     This takes care of exporting the different office destinations
	/// </summary>
	[StartupAction(StartupOrder = (int) GreenshotStartupOrder.Addon)]
	public class OfficeDestinationExporter : IStartupAction
	{
		[Import]
		private IOfficeConfiguration OfficeConfiguration { get; set; }

		[Import]
		private IServiceExporter ServiceExporter { get; set; }

		[Import]
		private IServiceLocator ServiceLocator { get; set; }

		/// <summary>
		///     Export all destinations
		/// </summary>
		public void Start()
		{
			if (WordDestination.IsActive)
			{
				var wordDestination = new WordDestination();
				ServiceLocator.FillImports(wordDestination);
				ServiceExporter.Export<IDestination>(wordDestination);
			}
			if (ExcelDestination.IsActive)
			{
				var excelDestination = new ExcelDestination();
				ServiceLocator.FillImports(excelDestination);
				ServiceExporter.Export<IDestination>(excelDestination);
			}
			if (OutlookDestination.IsActive)
			{
				var outlookDestination = new OutlookDestination();
				ServiceLocator.FillImports(outlookDestination);
				ServiceExporter.Export<IDestination>(outlookDestination);
			}
			if (PowerpointDestination.IsActive)
			{
				var powerpointDestination = new PowerpointDestination();
				ServiceLocator.FillImports(powerpointDestination);
				ServiceExporter.Export<IDestination>(powerpointDestination);
			}
			if (OneNoteDestination.IsActive)
			{
				var oneNoteDestination = new OneNoteDestination();
				ServiceLocator.FillImports(oneNoteDestination);
				ServiceExporter.Export<IDestination>(oneNoteDestination);
			}
		}
	}
}