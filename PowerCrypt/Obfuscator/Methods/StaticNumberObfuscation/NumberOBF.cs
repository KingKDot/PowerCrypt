namespace PowerCrypt.Obfuscator.Methods.StaticNumberObfuscation
{
    public class NumberOBF
    {
        public static string ObfuscateNumber(string number)
        {
            if (!IsDecimalNumber(number))
            {
                return number;
            }

            return AddOrSubtractRandomEQ(number);
        }

        private static bool IsDecimalNumber(string number)
        {
            return int.TryParse(number, out _);
        }

        private static string AddOrSubtractRandomEQ(string numberToObf)
        {
            var random = new Random();
            var number1 = random.Next(1, 10000);
            var signs = new[] { "+", "-" };
            var sign1 = signs[random.Next(0, 2)];
            var oppositeSign1 = sign1 == "+" ? "-" : "+";

            var finalNumber = $"{numberToObf} {sign1} {number1}";
            var outFinal = EvaluateExpression(finalNumber);

            //1/4 chance to include a third number
            if (random.Next(0, 4) == 0)
            {
                var number2 = random.Next(1, 10000);
                var sign2 = signs[random.Next(0, 2)];
                var oppositeSign2 = sign2 == "+" ? "-" : "+";

                finalNumber = $"{outFinal} {sign2} {number2}";
                outFinal = EvaluateExpression(finalNumber);

                var newProblem = $"{outFinal}{oppositeSign2}{number2}{oppositeSign1}{number1}";
                return $"({newProblem})";
            }
            else
            {
                var newProblem = $"{outFinal}{oppositeSign1}{number1}";
                return $"({newProblem})";
            }
        }

        private static int EvaluateExpression(string expression)
        {
            var result = 0;
            var data = expression.Split(' ');
            result = int.Parse(data[0]);

            var sign = data[1];
            var number = int.Parse(data[2]);

            if (sign == "+")
            {
                result += number;
            }
            else if (sign == "-")
            {
                result -= number;
            }

            return result;
        }
    }
}
