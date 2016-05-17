﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FP
{
	public class Summator
	{
		private readonly ISumFormatter formatter;
		private readonly Func<DataSource> openDatasource;
		private readonly string outputFilename;
		/*
		Отрефакторите код.
			1. Отделите максимум логики от побочных эффектов.
			2. Создайте нужные вам методы.
			3. Сделайте так, чтобы максимум кода оказалось внутри универсальных методов, потенциально полезных в других местах программы.
		*/

		public Summator(Func<DataSource> openDatasource, ISumFormatter formatter, string outputFilename)
		{
			this.openDatasource = openDatasource;
			this.formatter = formatter;
			this.outputFilename = outputFilename;
		}


		public void Process()
		{
            using (var input = openDatasource())
			{
                var c = 0;
                var allResult = GetAllRecords(input).Select(ConvertToInt).Select(GetResult).AfterEvery(++c, WriteProgress);
                File.WriteAllLines(outputFilename, allResult);
            }
		}

	    private static void WriteProgress(int c)
	    {
	        if (c % 100 == 0)
	            Console.WriteLine($"processed {c} items");
	    }

        private string GetResult(IEnumerable<int> args) => formatter.Format(args, args.Sum());

	    private static IEnumerable<int> ConvertToInt(string[] record) => record.Select(part => Convert.ToInt32(part, 16));

	    private static IEnumerable<string[]> GetAllRecords(DataSource input)
	    {
	        string[] record;
	        while ((record = input.NextRecord()) != null)
	            yield return record;
	    }

	    public void ProcessRefactored()
		{
			SumRecords(openDatasource(), formatter, outputFilename);
		}

		public static void SumRecords(
			DataSource dataSource,
			ISumFormatter formatter,
			string outputFilename)
		{
			var res = SumRecords(dataSource, formatter)
				.AfterEvery(100, c => Console.WriteLine("processed {0} items", c));
			File.WriteAllLines(outputFilename, res);
		}

		public static IEnumerable<string> SumRecords(
			DataSource dataSource,
			ISumFormatter formatter)
		{
			return dataSource.ReadIntRecords(16)
				.Select(args => formatter.Format(args, args.Sum()));
		}
	}

	public static class DataSourceExtensions
	{
		public static IEnumerable<string[]> ReadRecords(this DataSource data)
		{
			return Enumeration.RepeatUntilNull(data.NextRecord);
		}

		public static IEnumerable<int[]> ReadIntRecords(this DataSource data, int radix)
		{
			return data.ReadRecords()
				.Select(record => record.Select(f => Convert.ToInt32(f, radix)).ToArray());
		}
	}

	public static class Enumeration
	{
		public static IEnumerable<T> RepeatUntilNull<T>(Func<T> get)
		{
			return Repeat(get).TakeWhile(i => i != null);
		}

		public static IEnumerable<T> Repeat<T>(Func<T> get)
		{
			while (true) yield return get();
			// ReSharper disable once IteratorNeverReturns
		}

		public static IEnumerable<T> AfterEvery<T>(
			this IEnumerable<T> items,
			int period,
			Action<int> afterNth)
		{
			var n = 0;
			foreach (var item in items)
			{
				n++;
				yield return item;
				if (n % period == 0) afterNth(n);
			}
		}
	}
}