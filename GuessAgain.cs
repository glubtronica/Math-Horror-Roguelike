using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Exponent Guessing Game (Gliding Tolerance) ===");

        Console.Write("Enter your player name: ");
        string playerName = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "Player";

        const int MinBase = 1;
        const int MaxBase = 100;

        // Safety caps
        const int MaxExponent = 12;
        const int MaxTurns = 50;

        Random rng = new Random();
        int baseNumber = rng.Next(MinBase, MaxBase + 1);

        int exponent = 1;
        int turn = 1;

        Console.WriteLine($"\nAlright {playerName}!");
        Console.WriteLine($"Base is between {MinBase} and {MaxBase}.");
        Console.WriteLine("Target each turn is base^exponent.");
        Console.WriteLine("If you're wrong but 'close enough' to the CURRENT target, exponent drops by 1 (min 1).");
        Console.WriteLine("Otherwise exponent increases by 1.\n");

        while (turn <= MaxTurns)
        {
            BigInteger currentTarget = BigInteger.Pow(baseNumber, exponent);

            BigInteger tolerance = GetGlidingTolerance(currentTarget, exponent);

            Console.WriteLine($"Turn {turn} | Exponent: {exponent}");
            Console.WriteLine($"(Close window: ±{tolerance})");
            Console.Write("Your guess: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (!BigInteger.TryParse(input, out BigInteger guess))
            {
                Console.WriteLine("Invalid input. Enter a whole number.\n");
                continue; // invalid input doesn't cost a turn
            }

            if (guess == currentTarget)
            {
                Console.WriteLine("\n🎉 Correct! You win!");
                Console.WriteLine($"Base was {baseNumber}, exponent was {exponent}, target was {currentTarget}.\n");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            BigInteger diff = BigInteger.Abs(guess - currentTarget);

            if (exponent > 1 && diff <= tolerance)
            {
                exponent -= 1;
                Console.WriteLine($"❌ Wrong — but close (|guess-target|={diff} ≤ {tolerance}). Exponent drops to {exponent}.\n");
            }
            else
            {
                exponent += 1;
                Console.WriteLine($"❌ Wrong — not close enough (|guess-target|={diff} > {tolerance}). Exponent rises to {exponent}.\n");
            }

            if (exponent > MaxExponent)
            {
                Console.WriteLine($"Exponent hit the cap ({MaxExponent}). Game over!");
                Console.WriteLine($"Base was {baseNumber}.");
                break;
            }

            turn++;
        }

        Console.WriteLine("\nGame over!");
        Console.WriteLine($"Base was {baseNumber}.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    // Exponent-based gliding tolerance:
    // - Starts modest at low exponents
    // - Grows as exponent increases
    // - Also scales with target magnitude
    static BigInteger GetGlidingTolerance(BigInteger target, int exponent)
    {
        // Percent window grows with exponent:
        // exponent 1 -> 5%
        // exponent 2 -> 7%
        // exponent 3 -> 9%
        // ...
        // capped at 25%
        int percent = Math.Min(25, 5 + (exponent - 1) * 2);

        // tolerance = target * percent / 100
        BigInteger tol = (target * percent) / 100;

        // never smaller than 10 (so early game isn't brutal)
        if (tol < 10) tol = 10;

        return tol;
    }
}