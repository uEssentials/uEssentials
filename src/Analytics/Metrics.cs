using System;
using System.Net;
using System.Text;
using System.Threading;
using Essentials.Api;
using SDG.Unturned;

// ReSharper disable InconsistentNaming

namespace Essentials.Analytics
{
    /// <summary>
    /// Very very simple metrics system.
    /// </summary>
    internal class Metrics
    {
        internal const string REPORT_URL = "http://ess-metrics.ga/?report";

        internal static void Init()
        {
            var osType = "undefined";
            var arch = "undefined";
            var gamemode = Provider.mode.ToString();
            var appId = Provider.APP_ID.m_AppId.ToString();
            var pvp = Provider.PvP.ToString().ToLower();
            var version = Provider.Version;
            var cameraMode = Provider.camera.ToString();
            var lang = Provider.language;

            try
            {
                osType = Environment.OSVersion.ToString();
            }
            catch (Exception) { /* ignored */ }

            try
            {
                arch = Environment.GetEnvironmentVariable( "PROCESSOR_ARCHITECTURE" );
            }
            catch (Exception) { /* ignored */ }

            new Thread( () =>
            {
                try
                {
                    var httpRequest = (HttpWebRequest) WebRequest.Create( REPORT_URL );

                    var dataBuilder = new StringBuilder( "data=" );

                    dataBuilder.Append( string.Join( ";", new[]
                    {
                        osType,
                        arch,
                        gamemode,
                        appId,
                        pvp,
                        version,
                        cameraMode,
                        lang
                    }) );

                    var data = Encoding.ASCII.GetBytes( dataBuilder.ToString() );

                    httpRequest.Method = "POST";
                    httpRequest.ContentType = "application/x-www-form-urlencoded";
                    httpRequest.ContentLength = data.Length;

                    using (var stream = httpRequest.GetRequestStream())
                    {
                        stream.Write( data, 0, data.Length );
                    }
                }
                catch (Exception ex)
                { 
                    EssProvider.Logger.LogWarning( $"Could not connect to metrics server, error: {ex.Message}" );
                    EssProvider.Logger.LogWarning( "If possible, report to developer at " +
                                                  "https://github.com/uEssentials/uEssentials/issues" );
                }
            } ).Start();
        }
    }
}