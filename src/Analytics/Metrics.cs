using System;
using System.Net;
using System.Text;
using System.Threading;
using SDG.Unturned;
using System.IO;

// ReSharper disable InconsistentNaming

namespace Essentials.Analytics
{
    /// <summary>
    /// Very very simple metrics system.
    /// </summary>
    internal class Metrics
    {
        internal const string REPORT_URL = "http://leofl.cf/metrics/?t={0}";

        private static bool working;

        internal static void Init()
        {
            working = true;
            var osType = "undefined";
            var arch = "undefined";
            var gamemode = Provider.mode.ToString();
            var pvp = Provider.PvP.ToString().ToLower();
            var cameraMode = Provider.camera.ToString();
            var lang = Provider.language;
            var port = Provider.ServerPort.ToString();

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
                    var httpRequest = (HttpWebRequest) WebRequest.Create( string.Format( REPORT_URL, 1 ) );

                    var dataBuilder = new StringBuilder( "data=" );

                    dataBuilder.Append( string.Join( ";", new[] {
                        osType,
                        arch,
                        gamemode,
                        pvp,
                        cameraMode,
                        lang,
                        port
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
                catch (Exception)
                {
                    // ignored
                }
            } ).Start();
        }

        internal static void ReportPlayer( Rocket.Unturned.Player.UnturnedPlayer player )
        {
            if ( player == null || !working )
            {
                return;
            }

            new Thread(() =>
            {
                try
                {
                    var httpRequest = (HttpWebRequest) WebRequest.Create( string.Format( REPORT_URL, 2 ) );

                    var dataBuilder = new StringBuilder("data=");

                    dataBuilder.Append( string.Join(";", new[] {
                        player.CSteamID.ToString()
                    }) );

                    var data = Encoding.ASCII.GetBytes( dataBuilder.ToString() );

                    httpRequest.Method = "POST";
                    httpRequest.ContentType = "application/x-www-form-urlencoded";
                    httpRequest.ContentLength = data.Length;

                    using (var stream = httpRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }).Start();
        }
    }
}