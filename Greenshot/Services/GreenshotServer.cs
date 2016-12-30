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

using System;
using System.IO;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Dapplo.Addons;
using Dapplo.Log;
using Dapplo.Utils;
using Greenshot.Addon.Core;
using Greenshot.CaptureCore;
using Greenshot.Core.Enumerations;
using System.Windows;
using Greenshot.Core.Interfaces;

#endregion

namespace Greenshot.Services
{
	/// <summary>
	///     This startup/shutdown action starts the Greenshot "server", which allows to open files etc.
	/// </summary>
	[StartupAction(StartupOrder = (int) GreenshotStartupOrder.Addon)]
	[ShutdownAction]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class GreenshotServer : IGreenshotContract, IStartupAction, IShutdownAction, IErrorHandler, IDispatchMessageInspector, IEndpointBehavior
	{
		private const string PipeBaseEndpoint = "net.pipe://localhost/Greenshot/Server_";
		private static readonly LogSource Log = new LogSource();
		private ServiceHost _host;

		/// <summary>
		///     This is the endpoint where the server is running
		/// </summary>
		public static string EndPoint => $"{PipeBaseEndpoint}{Identity}";

		private static string Identity => WindowsIdentity.GetCurrent().User?.Value;

		/// <summary>
		///     Defines if the server is started
		/// </summary>
		public bool IsStarted { get; set; }

		/// <summary>
		///     IShutdownAction entry, This stops the Greenshot server
		/// </summary>
		public void Shutdown()
		{
			Log.Debug().WriteLine("Stopping Greenshot server");
			if (_host != null)
			{
				_host.Close();
				_host = null;
			}
		}

		/// <summary>
		///     IStartupAction entry for starting
		/// </summary>
		public void Start()
		{
			Log.Debug().WriteLine("Starting Greenshot server");
			try
			{
				_host = new ServiceHost(this, new Uri(PipeBaseEndpoint));
				Log.Debug().WriteLine("Starting Greenshot server with endpoints:");

				// Add ServiceMetadataBehavior
				_host.Description.Behaviors.Add(new ServiceMetadataBehavior {HttpsGetEnabled = false});

				// Our IGreenshotContract endpoint:
				var serviceEndpointGreenshotContract = _host.AddServiceEndpoint(typeof(IGreenshotContract), new NetNamedPipeBinding(), EndPoint);
				Log.Debug().WriteLine("Added endpoint: address=\"{4:l}\", contract=\"{0:l}\", contractNamespace=\"{1:l}\", binding=\"{2:l}_{0:l}\", bindingNamespace=\"{3:l}\"", serviceEndpointGreenshotContract.Contract.Name, serviceEndpointGreenshotContract.Contract.Namespace, serviceEndpointGreenshotContract.Binding.Name, serviceEndpointGreenshotContract.Binding.Namespace, serviceEndpointGreenshotContract.ListenUri.AbsoluteUri);

				// Add error / request logging
				serviceEndpointGreenshotContract.EndpointBehaviors.Add(this);

				// The MetadataExchangeBindings endpoint
				var serviceEndpointMex = _host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexNamedPipeBinding(), EndPoint + "/mex");
				Log.Debug().WriteLine("Added endpoint: address=\"{4}\", contract=\"{0}\", contractNamespace=\"{1}\", binding=\"{2}\", bindingNamespace=\"{3}\"", serviceEndpointMex.Contract.Name, serviceEndpointMex.Contract.Namespace, serviceEndpointMex.Binding.Name, serviceEndpointMex.Binding.Namespace, serviceEndpointMex.ListenUri.AbsoluteUri);
				_host.Open();
				IsStarted = true;
				Log.Debug().WriteLine("Started Greenshot server");
			}
			catch (Exception ex)
			{
				Log.Error().WriteLine(ex, "Couldn't create Greenshot server");
				throw;
			}
		}

		#region IGreenshotContract

		/// <summary>
		///     Exit Greenshot
		/// </summary>
		public void Exit()
		{
			Application.Current.Shutdown();
		}

		/// <summary>
		///     Open a file into Greenshot
		/// </summary>
		/// <param name="filename"></param>
		public void OpenFile(string filename)
		{
			Log.Info().WriteLine("Open file requested for: {0}", filename);

			if (File.Exists(filename))
			{
				UiContext.RunOn(async () => await CaptureHelper.CaptureFileAsync(filename));
			}
			else
			{
				Log.Warn().WriteLine("No such file: {0}", filename);
			}
		}


		public void CaptureScreen(bool cursor)
		{
			UiContext.RunOn(async () => await CaptureHelper.CaptureFullscreenAsync(true, ScreenCaptureMode.Auto));
		}

		#endregion

		#region IErrorHandler

		public void ProvideFault(Exception exception, MessageVersion messageVersion, ref Message faultMessage)
		{
			Log.Error().WriteLine(exception, faultMessage.ToString());
		}

		public bool HandleError(Exception error)
		{
			return false;
		}

		#endregion

		#region IDispatchMessageInspector

		public object AfterReceiveRequest(ref Message requestMessage, IClientChannel channel, InstanceContext instanceContext)
		{
			if (Log.IsDebugEnabled())
			{
				Log.Debug().WriteLine(requestMessage.ToString());
			}
			return null;
		}

		public void BeforeSendReply(ref Message replyMessage, object correlationState)
		{
			if (Log.IsDebugEnabled())
			{
				Log.Debug().WriteLine(replyMessage.ToString());
			}
		}

		#endregion

		#region IEndpointBehavior

		public void Validate(ServiceEndpoint endpoint)
		{
			// Do nothing
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			// Do nothing
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(this);
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			// Do nothing
		}

		#endregion
	}
}