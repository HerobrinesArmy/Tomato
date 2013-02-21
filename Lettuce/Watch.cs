using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tomato;

namespace Lettuce
{
    public static class Watch
    {
        // Grouped by priority, based on C operator precedence
        public static readonly string[][] Operators = new[]
            {
                new[] { "*", "/", "%" },
                new[] { "+", "-" },
                new[] { "<<", ">>" },
                new[] { "<", "<=", ">", ">=" },
                new[] { "==", "!=" },
                new[] { "&" },
                new[] { "^" },
                new[] { "|" },
                new[] { "&&" },
                new[] { "||" }
            };

        public static ulong Evaluate(string expression, DCPU cpu)
        {
            expression = expression.Trim();
            // Check for parenthesis
            while (expression.SafeContains('[') && expression.SafeContains(']'))
            {
                var startIndex = -1;
                var endIndex = -1;
                var depth = 0;
                for (int i = 0; i < expression.Length; i++)
                {
                    if (expression[i] == '[')
                    {
                        if (startIndex == -1)
                            startIndex = i;
                        depth++;
                    }
                    if (expression[i] == ']')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            endIndex = i;
                            break;
                        }
                    }
                }
                var subexpression = expression.Substring(startIndex + 1, endIndex - startIndex - 1);
                var result = cpu.Memory[(ushort)Evaluate(subexpression, cpu)];
                expression = expression.Remove(startIndex, endIndex - startIndex + 1);
                expression = expression.Insert(startIndex, result.ToString());
            }
            if (HasOperators(expression))
            {
                // Recurse
                var parts = SplitExpression(expression);
                if (parts[0] == "" && parts[1] == "-") // Negate
                    return (ulong)-(long)Evaluate(parts[2], cpu);
                if (parts[0] == "" && parts[1] == "~") // NOT
                    return ~Evaluate(parts[2], cpu);
                switch (parts[1]) // Evaluate
                {
                    case "+":
                        return Evaluate(parts[0], cpu)
                               +
                               Evaluate(parts[2], cpu);
                    case "-":
                        return Evaluate(parts[0], cpu)
                               -
                               Evaluate(parts[2], cpu);
                    case "*":
                        return Evaluate(parts[0], cpu)
                               *
                               Evaluate(parts[2], cpu);
                    case "/":
                        return Evaluate(parts[0], cpu)
                               /
                               Evaluate(parts[2], cpu);
                    case "%":
                        return Evaluate(parts[0], cpu)
                               %
                               Evaluate(parts[2], cpu);
                    case "<<":
                        return Evaluate(parts[0], cpu)
                               <<
                               (int)Evaluate(parts[2], cpu);
                    case ">>":
                        return Evaluate(parts[0], cpu)
                               >>
                               (int)Evaluate(parts[2], cpu);
                    case "<":
                        return Evaluate(parts[0], cpu)
                               <
                               Evaluate(parts[2], cpu) ? 1UL : 0UL;
                    case "<=":
                        return Evaluate(parts[0], cpu)
                               <=
                               Evaluate(parts[2], cpu) ? 1UL : 0UL;
                    case ">":
                        return Evaluate(parts[0], cpu)
                               >
                               Evaluate(parts[2], cpu) ? 1UL : 0UL;
                    case ">=":
                        return Evaluate(parts[0], cpu)
                               >=
                               Evaluate(parts[2], cpu) ? 1UL : 0UL;
                    case "==":
                        return Evaluate(parts[0], cpu)
                               ==
                               Evaluate(parts[2], cpu) ? 1UL : 0UL;
                    case "!=":
                        return Evaluate(parts[0], cpu)
                               !=
                               Evaluate(parts[2], cpu) ? 1UL : 0UL;
                    case "&":
                        return Evaluate(parts[0], cpu)
                               &
                               Evaluate(parts[2], cpu);
                    case "^":
                        return Evaluate(parts[0], cpu)
                               ^
                               Evaluate(parts[2], cpu);
                    case "|":
                        return Evaluate(parts[0], cpu)
                               |
                               Evaluate(parts[2], cpu);
                    case "&&":
                        return (Evaluate(parts[0], cpu) == 1
                               &&
                               Evaluate(parts[2], cpu) == 1) ? 1UL : 0UL;
                    case "||":
                        return (Evaluate(parts[0], cpu) == 1
                               ||
                               Evaluate(parts[2], cpu) == 1) ? 1UL : 0UL;
                }
            }
            else
            {
                // Interpret value
                if (expression.StartsWith("0x") || expression.StartsWith("$") || expression.EndsWith("h")) // Hex
                    return Convert.ToUInt64(expression, 16);
                else if (expression.StartsWith("0b") || expression.StartsWith("%") || expression.EndsWith("b")) // Binary
                    return Convert.ToUInt64(expression, 2);
                else if (expression.StartsWith("0o")) // Octal
                    return Convert.ToUInt64(expression, 8);
                else
                {
                    // Check for number
                    bool number = true;
                    for (int i = 0; i < expression.Length; i++)
                        if (!char.IsNumber(expression[i]))
                            number = false;
                    if (number) // Decimal
                        return Convert.ToUInt64(expression);
                    else
                    {
                        // Look up reference, it's a register or something
                        switch (expression.ToUpper())
                        {
                            case "A":
                                return cpu.A;
                            case "B":
                                return cpu.B;
                            case "C":
                                return cpu.C;
                            case "X":
                                return cpu.X;
                            case "Y":
                                return cpu.Y;
                            case "Z":
                                return cpu.Z;
                            case "I":
                                return cpu.I;
                            case "J":
                                return cpu.J;
                            case "PC":
                                return cpu.PC;
                            case "SP":
                                return cpu.SP;
                            case "EX":
                                return cpu.EX;
                            case "IA":
                                return cpu.IA;
                            default:
                                foreach (var kvp in Debugger.KnownLabels)
                                {
                                    if (kvp.Value.ToLower() == expression.ToLower())
                                    {
                                        return kvp.Key;
                                    }
                                }
                                break;
                        }
                        throw new KeyNotFoundException("Symbol not found: " + expression + ".");
                    }
                }
            }
            throw new InvalidOperationException("Invalid expression");
        }

        private static string[] SplitExpression(string expression)
        {
            for (int i = 0; i < Operators.Length; i++)
            {
                for (int j = 0; j < Operators[i].Length; j++)
                {
                    int index = expression.IndexOf(Operators[i][j]);
                    if (index != -1)
                    {
                        // Split along this operator
                        return new[]
                            {
                                expression.Remove(index),
                                Operators[i][j],
                                expression.Substring(index + Operators[i][j].Length)
                            };
                    }
                }
            }
            return null;
        }

        private static bool HasOperators(string expression)
        {
            for (int i = 0; i < Operators.Length; i++)
            {
                for (int j = 0; j < Operators[i].Length; j++)
                {
                    if (expression.Contains(Operators[i][j]))
                        return true;
                }
            }
            return false;
        }

        public static int SafeIndexOf(this string value, char needle)
        {
            value = value.Trim();
            bool inString = false, inChar = false;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == needle && !inString && !inChar)
                    return i;
                if (value[i] == '"' && !inChar)
                    inString = !inString;
                if (value[i] == '\'' && !inString)
                    inChar = !inChar;
            }
            return -1;
        }

        public static int SafeIndexOf(this string value, string needle)
        {
            value = value.Trim();
            bool inString = false, inChar = false;
            for (int i = 0; i < value.Length; i++)
            {
                if (value.Substring(i).StartsWith(needle) && !inString && !inChar)
                    return i;
                if (value[i] == '"' && !inChar)
                    inString = !inString;
                if (value[i] == '\'' && !inString)
                    inChar = !inChar;
            }
            return -1;
        }
    }
}
