using System;

namespace fastfoodshop.Services
{
    public class DiscountService
    {
        // Расчёт итоговой стоимости заказа с учётом скидки постоянного клиента.
        // Формула: P = P0 × (1 − d / 100), где P0 — базовая сумма, d — процент скидки.
        // Скидка применяется, если сумма заказа не меньше минимального порога скидки.
        public decimal CalculateTotal(decimal baseSum, decimal discountPercent, decimal minOrderSum)
        {
            if (baseSum <= 0 || discountPercent <= 0 || baseSum < minOrderSum)
                return decimal.Round(baseSum, 2, MidpointRounding.AwayFromZero);

            return decimal.Round(baseSum * (1 - discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        }

        //Размер скидки в рублях для отображения в текущем заказе.
        public decimal CalculateDiscountAmount(decimal baseSum, decimal discountPercent, decimal minOrderSum)
            => decimal.Round(baseSum - CalculateTotal(baseSum, discountPercent, minOrderSum), 2, MidpointRounding.AwayFromZero);

        //Применяется ли скидка к указанной сумме.
        public bool IsDiscountApplied(decimal baseSum, decimal discountPercent, decimal minOrderSum)
            => baseSum > 0 && discountPercent > 0 && baseSum >= minOrderSum;
    }
}
