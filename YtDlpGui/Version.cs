namespace YtDlpGui;

public struct Version : IComparable
{
	public int[] Values { get; }

	public Version(int[] values)
	{
		Values = values;
	}

	public static Version Parse(string str)
	{
		int[] values = str.Split('.').Select(int.Parse).ToArray();
		return new Version(values);
	}

	public int CompareTo(object? obj)
	{
		if (obj is not Version oth)
			throw new Exception();
		int len = Math.Min(Values.Length, oth.Values.Length);
		for (int i = 0; i < len; i++)
			if (Values[i] != oth.Values[i])
				return Values[i] - oth.Values[i];
		return Values.Length - oth.Values.Length;
	}

	public override string ToString()
	{
		return string.Join('.', Values);
	}
}
