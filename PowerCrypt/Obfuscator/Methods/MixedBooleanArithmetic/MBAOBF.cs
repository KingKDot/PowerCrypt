namespace PowerCrypt.Obfuscator.Methods.MixedBooleanArithmetic
{
    public class MBAOBF
    {
        public static readonly Dictionary<string, string> OperationRules = new()
        {
            { "Bxor", "(LEFT-BorRIGHT)-(LEFT-BandRIGHT)" },
            { "Plus", "(LEFT-BandRIGHT)+(LEFT-BorRIGHT)" },
            { "Subtract", "(LEFT-Bxor-RIGHT)+2*(LEFT-Band-RIGHT)" },
            { "Band", "(LEFT+RIGHT)-(LEFT-BorRIGHT)" },
            { "Bor", "LEFT+RIGHT+1+(-bnotLEFT-Bor-bnotRIGHT)" },
            { "ShiftLeft", "(LEFT*2)+((RIGHT)*2)" },
            { "ShiftRight", "(LEFT/2)+((RIGHT)/2)" },
            { "Multiply", "(LEFT-BorLEFT-BandRIGHT)-(LEFT-BxorRIGHT)" },
            { "Divide", "(LEFT/RIGHT)+((LEFT-Band1)*RIGHT)" },
            { "Modulo", "(LEFT-(LEFT/RIGHT)*RIGHT)" },
            { "Complex1", "((LEFT-BorRIGHT)*2)-((LEFT-BandRIGHT)/2)" },
            { "Complex2", "(-bnot(LEFT-BandRIGHT))+(LEFT-Bxor-RIGHT)" },
            { "BitwiseMask", "(LEFT-Band0xFFFFFFFF)" },
            { "NegateAndShift", "((-bnotLEFT)-Bor((RIGHT)*2))" },
            // not sure what causes minus to also be picked up along with subtract but we ballin ig
            { "Minus", "(LEFT-Bxor-RIGHT)+2*(LEFT-Band-RIGHT)" }
        };

        public static string? ApplyMBAObfuscation(string left, string right, string operatorKey, int depth = 1)
        {
            if (!OperationRules.TryGetValue(operatorKey, out string? operation))
            {
                Console.WriteLine("Invalid Operation");
                Console.WriteLine(left);
                Console.WriteLine(right);
                Console.WriteLine(operatorKey);
                Console.ReadKey();
                return null;
            }

            //replace placeholders
            operation = operation.Replace("LEFT", left).Replace("RIGHT", right);

            if (depth > 1)
            {
                var expressions = ParseBinaryExpressions(operation);
                foreach (var expr in expressions)
                {
                    string? newOp = expr.Operator switch
                    {
                        "&" => ApplyMBAObfuscation(expr.Left, expr.Right, "Band", depth - 1),
                        "|" => ApplyMBAObfuscation(expr.Left, expr.Right, "Bor", depth - 1),
                        "^" => ApplyMBAObfuscation(expr.Left, expr.Right, "Bxor", depth - 1),
                        "+" => ApplyMBAObfuscation(expr.Left, expr.Right, "Plus", depth - 1),
                        "-" => ApplyMBAObfuscation(expr.Left, expr.Right, "Subtract", depth - 1),
                        _ => null
                    };

                    if (newOp != null)
                    {
                        //random non-linear transformations
                        var randomTransformations = new List<string>
                    {
                        $"((({newOp})-shl1)-shr1)", //shift operations
                        $"(-bnot(-bnot({newOp})))",        //double negation
                        $"((({newOp})+0)-0)",  //dummy additions/subtractions
                        $"(({newOp})-Band0xFFFFFFFF)" //masking
                    };
                        var randomTransformation = randomTransformations[new Random().Next(randomTransformations.Count)];
                        operation = operation.Replace(expr.FullExpression, $"({randomTransformation})");
                    }
                }
            }

            return operation;
        }

        public static List<BinaryExpression> ParseBinaryExpressions(string input)
        {
            //simple expression parser
            var operators = new[] { "&", "|", "^", "+", "-" };
            var expressions = new List<BinaryExpression>();

            foreach (var op in operators)
            {
                int index = input.IndexOf(op);
                if (index > 0)
                {
                    string left = input[..index].Trim();
                    string right = input[(index + 1)..].Trim();
                    expressions.Add(new BinaryExpression(left, right, op, input));
                }
            }

            return expressions;
        }
    }

    public class BinaryExpression
    {
        public string Left { get; }
        public string Right { get; }
        public string Operator { get; }
        public string FullExpression { get; }

        public BinaryExpression(string left, string right, string @operator, string fullExpression)
        {
            Left = left;
            Right = right;
            Operator = @operator;
            FullExpression = fullExpression;
        }
    }
}
