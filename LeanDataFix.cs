using System;
using System.Linq;

using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Securities;
namespace OandaDataProvider
{
	public class LeanDataFix
	{
		/// <summary>
		/// Parses file name into a <see cref="Security"/> and DateTime
		/// </summary>
		/// <param name="fileName">File name to be parsed</param>
		/// <param name="symbol">The symbol as parsed from the fileName</param>
		/// <param name="date">Date of data in the file path. Only returned if the resolution is lower than Hourly</param>
		/// <param name="resolution">The resolution of the symbol as parsed from the filePath</param>
		public static bool TryParsePath( string fileName, out Symbol symbol, out DateTime date, out Resolution resolution )
		{
			symbol = null;
			resolution = Resolution.Daily;
			date = default( DateTime );

			var pathSeparators = new[] { '/', '\\' };
			var securityTypes = Enum.GetNames( typeof( SecurityType ) ).Select( x => x.ToLower() ).ToList();

			try {
				// Removes file extension
				fileName = fileName.Replace( fileName.GetExtension(), "" );

				// remove any relative file path
				while ( fileName.First() == '.' || pathSeparators.Any( x => x == fileName.First() ) ) {
					fileName = fileName.Remove( 0, 1 );
				}

				// split path into components
				var info = fileName.Split( pathSeparators, StringSplitOptions.RemoveEmptyEntries ).ToList();

				// find where the useful part of the path starts - i.e. the securityType
				var startIndex = info.FindIndex( x => securityTypes.Contains( x.ToLower() ) );

				// Gather components useed to create the security
				var market = info[startIndex + 1];
				var ticker = info[startIndex + 3];
				resolution = (Resolution)Enum.Parse( typeof( Resolution ), info[startIndex + 2], true );
				var securityType = (SecurityType)Enum.Parse( typeof( SecurityType ), info[startIndex], true );

				if ( securityType == SecurityType.Option || securityType == SecurityType.Future ) {
					throw new ArgumentException( "LeanData.TryParsePath(): Options and futures are not supported by this method." );
				}

				// If resolution is Daily or Hour, we do not need to set the date and tick type
				if ( resolution < Resolution.Hour ) {
					date = DateTime.ParseExact( info[startIndex + 4].Substring( 0, 8 ) + " -00:00", "yyyyMMdd zzz", null );
				}

				symbol = Symbol.Create( ticker, securityType, market );
			} catch ( Exception ex ) {
				Log.Error( "LeanData.TryParsePath(): Error encountered while parsing the path {0}. Error: {1}", fileName, ex.GetBaseException() );
				return false;
			}

			return true;
		}
	}
}
