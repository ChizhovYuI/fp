using System;
using System.Collections.Generic;
using System.Linq;

namespace FP
{
	public interface ISumFormatter
	{
		string Format(IEnumerable<int> nums, int sum);
	}

	public class HexSumFormatter : ISumFormatter
	{
		public string Format(IEnumerable<int> nums, int sum)
		{
			return $"Sum({string.Join(" ", nums.Select(n => Convert.ToString(n, 16)))}) = {Convert.ToString(sum, 16)}";
		}
	}
}