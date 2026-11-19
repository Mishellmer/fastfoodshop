using System;

namespace fastfoodshop.Services
{
    public class DiscountService
    {
        // Расчёт итоговой стоимости заказа.
        public decimal CalculateTotal(decimal baseSum, decimal discountPercent, decimal minOrderSum)
        {
            if (baseSum <= 0 || discountPercent <= 0 || baseSum < minOrderSum)
                return decimal.Round(baseSum, 2, MidpointRounding.AwayFromZero);

            return decimal.Round(baseSum * (1 - discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalculateDiscountAmount(decimal baseSum, decimal discountPercent, decimal minOrderSum)
            => decimal.Round(baseSum - CalculateTotal(baseSum, discountPercent, minOrderSum), 2, MidpointRounding.AwayFromZero);

        public bool IsDiscountApplied(decimal baseSum, decimal discountPercent, decimal minOrderSum)
            => baseSum > 0 && discountPercent > 0 && baseSum >= minOrderSum;
    }
}
