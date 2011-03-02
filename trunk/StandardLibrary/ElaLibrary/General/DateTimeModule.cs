using System;
using Ela.Linking;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
	public sealed class DateTimeModule : ForeignModule
	{
		#region Construction
		public DateTimeModule()
		{

		}
		#endregion


		#region Methods
		public override void Initialize()
		{
			Add<ElaDateTime>("now", Now);
			Add<ElaDateTime>("today", Today);
			Add<ElaDateTime,ElaDateTime,ElaDateTime>("add", Add);
			Add<Int64,ElaDateTime,ElaDateTime>("addTicks", AddTicks);
			Add<Double,ElaDateTime,ElaDateTime>("addMilliseconds", AddMilliseconds);
			Add<Double,ElaDateTime,ElaDateTime>("addSeconds", AddSeconds);
			Add<Double,ElaDateTime,ElaDateTime>("addMinutes", AddMinutes);
			Add<Double,ElaDateTime,ElaDateTime>("addHours", AddHours);
			Add<Double,ElaDateTime,ElaDateTime>("addDays", AddDays);
			Add<Int32,ElaDateTime,ElaDateTime>("addMonths", AddMonths);
			Add<Int32,ElaDateTime,ElaDateTime>("addYears", AddYears);
			Add<ElaDateTime,Int32>("years", Years);
			Add<ElaDateTime,Int32>("months", Months);
			Add<ElaDateTime,Int32>("days", Days);
			Add<ElaDateTime,Int32>("hours", Hours);
			Add<ElaDateTime,Int32>("minutes", Minutes);
			Add<ElaDateTime,Int32>("seconds", Seconds);
			Add<ElaDateTime,Int32>("milliseconds", Milliseconds);
			Add<ElaDateTime,Int64>("ticks", Ticks);
			Add<ElaDateTime,ElaVariant>("dayOfWeek", GetDayOfWeek);
			Add<ElaDateTime,Int32>("dayOfYear", GetDayOfYear);
			Add<ElaDateTime,ElaDateTime>("date", GetDate);
			Add<String,String,ElaDateTime>("parse", Parse);
			Add<ElaDateTime,String>("toLocaleString", ToLocaleString);
			Add<String,ElaDateTime,String>("toLocaleStringFormat", ToLocaleStringFormat);
			Add<ElaDateTime,ElaDateTime,ElaRecord>("diff", Diff);
			Add<ElaDateTime,ElaDateTime,Int32>("diffSeconds", DiffSeconds);
			Add<ElaDateTime,ElaDateTime,Int32>("diffMilliseconds", DiffMilliseconds);
			Add<ElaDateTime,ElaDateTime,Int64>("diffTicks", DiffTicks);
		}


		public ElaDateTime Now()
		{
			return new ElaDateTime(DateTime.Now);
		}


		public ElaDateTime Today()
		{
			return new ElaDateTime(DateTime.Today);
		}


		public ElaDateTime Add(ElaDateTime toAdd, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).Add(new TimeSpan(toAdd.Ticks)));
		}


		public ElaDateTime AddTicks(long ticks, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddTicks(ticks));	
		}


		public ElaDateTime AddMilliseconds(double ms, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddMilliseconds(ms));
		}


		public ElaDateTime AddSeconds(double sec, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddSeconds(sec));
		}


		public ElaDateTime AddMinutes(double mins, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddMinutes(mins));
		}


		public ElaDateTime AddHours(double hours, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddHours(hours));
		}


		public ElaDateTime AddDays(double days, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddDays(days));
		}


		public ElaDateTime AddMonths(int months, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddMonths(months));
		}


		public ElaDateTime AddYears(int years, ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).AddYears(years));
		}


		public int Years(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Year;
		}


		public int Months(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Month;
		}


		public int Days(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Day;
		}


		public int Hours(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Hour;
		}


		public int Minutes(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Minute;
		}


		public int Seconds(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Second;
		}


		public int Milliseconds(ElaDateTime val)
		{
			return new DateTime(val.Ticks).Millisecond;
		}


		public long Ticks(ElaDateTime val)
		{
			return val.Ticks;
		}


		public ElaVariant GetDayOfWeek(ElaDateTime val)
		{
			var dw = new DateTime(val.Ticks).DayOfWeek;

			switch (dw)
			{
				case DayOfWeek.Friday: return new ElaVariant("Fri");
				case DayOfWeek.Monday: return new ElaVariant("Mon");
				case DayOfWeek.Saturday: return new ElaVariant("Sat");
				case DayOfWeek.Sunday: return new ElaVariant("Sun");
				case DayOfWeek.Thursday: return new ElaVariant("Thu");
				case DayOfWeek.Tuesday: return new ElaVariant("Tue");
				case DayOfWeek.Wednesday: return new ElaVariant("Wed");
				default: return ElaVariant.None();
			}
		}


		public int GetDayOfYear(ElaDateTime val)
		{
			return new DateTime(val.Ticks).DayOfYear;
		}


		public ElaDateTime GetDate(ElaDateTime val)
		{
			return new ElaDateTime(new DateTime(val.Ticks).Date.Ticks);
		}
			

		public ElaDateTime Parse(string format, string value)
		{
			return new ElaDateTime(DateTime.ParseExact(value, format, ElaDateTime.Culture)) ;
		}


		public string ToLocaleString(ElaDateTime val)
		{
			return new DateTime(val.Ticks).ToLocalTime().ToString(ElaDateTime.DEFAULT_FORMAT, ElaDateTime.Culture);
		}


		public string ToLocaleStringFormat(string format, ElaDateTime val)
		{
			return new DateTime(val.Ticks).ToLocalTime().ToString(format, ElaDateTime.Culture);
		}


		public ElaRecord Diff(ElaDateTime left, ElaDateTime right)
		{
			var ts = new DateTime(left.Ticks) - new DateTime(right.Ticks);
			return new ElaRecord(
				new ElaRecordField("ticks", ts.Ticks, false),
				new ElaRecordField("milliseconds", (Int32)ts.TotalMilliseconds, false),
				new ElaRecordField("seconds", (Int32)ts.TotalSeconds, false),
				new ElaRecordField("minutes", (Int32)ts.TotalMinutes, false),
				new ElaRecordField("hours", (Int32)ts.TotalHours, false),
				new ElaRecordField("days", (Int32)ts.TotalDays, false));
		}


		public int DiffSeconds(ElaDateTime left, ElaDateTime right)
		{
			return (Int32)(new DateTime(left.Ticks) - new DateTime(right.Ticks)).TotalSeconds;
		}


		public int DiffMilliseconds(ElaDateTime left, ElaDateTime right)
		{
			return (Int32)(new DateTime(left.Ticks) - new DateTime(right.Ticks)).TotalMilliseconds;
		}


		public long DiffTicks(ElaDateTime left, ElaDateTime right)
		{
			return left.Ticks - right.Ticks;
		}
		#endregion
	}
}