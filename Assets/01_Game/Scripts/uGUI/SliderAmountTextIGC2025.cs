using System;
using UltimateClean;

public class SliderAmountTextIGC2025 : SliderAmountText
{
    public override void SetAmountText(float value)
    {
        // base.SetAmountText(value);

        if (WholeNumber)
            text.text = $"{(int)(value * 100f)}{Suffix}";
        else
            text.text = $"{Math.Round(value, 2)}{Suffix}";
    }
}
