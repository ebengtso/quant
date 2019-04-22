namespace QuantConnect.Lean.Engine.DataFeeds
{
	using System;

	using Newtonsoft.Json;

	public partial class Instrument
	{
		[JsonProperty( "instrument" )]
		public string PurpleInstrument { get; set; }

		[JsonProperty( "granularity" )]
		public string Granularity { get; set; }

		[JsonProperty( "candles" )]
		public Candle[] Candles { get; set; }
	}

	public partial class Candle
	{
		[JsonProperty( "complete" )]
		public bool Complete { get; set; }

		[JsonProperty( "volume" )]
		public long Volume { get; set; }

		[JsonProperty( "time" )]
		public DateTime Time { get; set; }

		[JsonProperty( "bid" )]
		public Ask Bid { get; set; }

		[JsonProperty( "ask" )]
		public Ask Ask { get; set; }
	}

	public partial class Ask
	{
		[JsonProperty( "o" )]
		public string O { get; set; }

		[JsonProperty( "h" )]
		public string H { get; set; }

		[JsonProperty( "l" )]
		public string L { get; set; }

		[JsonProperty( "c" )]
		public string C { get; set; }
	}

	public partial class Instrument
	{
		public static Instrument FromJson( string json ) => JsonConvert.DeserializeObject<Instrument>( json, Converter.Settings );
	}

	public static class Serialize
	{
		public static string ToJson( this Instrument self ) => JsonConvert.SerializeObject( self, Converter.Settings );
	}

	public class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
		};
	}
}
