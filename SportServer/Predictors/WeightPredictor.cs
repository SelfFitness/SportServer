namespace SportServer.Predictors
{
    public class WeightPredictor
    {
        private double a; // Коэффициент наклона

        private double b; // Смещение

        public WeightPredictor()
        {
        }

        // Метод для перевода даты в дни от начальной даты
        private double DateToDays(DateTime date, DateTime startDate)
        {
            return (date - startDate).TotalDays;
        }

        // Метод для обучения модели
        public void Train(List<double> weights, List<DateTime> dates)
        {
            if (weights.Count != dates.Count)
                throw new ArgumentException("Длины списков weights и dates должны быть одинаковыми");

            int n = weights.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            DateTime startDate = dates[0]; // Начальная дата для вычислений

            for (int i = 0; i < n; i++)
            {
                double x = DateToDays(dates[i], startDate);
                double y = weights[i];

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            // Вычисление коэффициентов линейной регрессии
            a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            b = (sumY - a * sumX) / n;
        }

        // Метод для предсказания веса на указанную дату
        public double Predict(DateTime startDate, DateTime futureDate)
        {
            double x = DateToDays(futureDate, startDate);
            return a * x + b;
        }
    }
}