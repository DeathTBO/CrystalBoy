using System;
using Gtk;

namespace CrystalBoy.Emulator.GTK.Widgets
{
	public class NumberEntry : Entry
	{
		int value;
		
		public int Value
		{
			get
			{
				return value;
			}
			
			set
			{
				this.value = value;

				Text = this.value.ToString();
			}
		}
		
		public int MaxValue
		{
			get;
			set;
		}

		public int MinValue
		{
			get;
			set;
		}

		public event EventHandler OnValueChanged;
		
		public NumberEntry() : base()
		{
			
		}
		
		public NumberEntry(int value, int minValue, int maxValue) : base()
		{
			MinValue = minValue;

			MaxValue = maxValue;
			
			Value = value;
		}

		protected override void OnTextInserted(string new_text, ref int position)
		{
			string text = Text + new_text;
			
			if (!OnlyContainsNumbers(text, out int numericValue))
				return;

			if (numericValue > MaxValue)
				return;

			if (numericValue < MinValue)
				return;

			value = numericValue;
			
			base.OnTextInserted(new_text, ref position);
			
			OnValueChanged?.Invoke(this, null);
		}

		bool OnlyContainsNumbers(string text, out int num)
		{
			return int.TryParse(text, out num);
		}
	}
}